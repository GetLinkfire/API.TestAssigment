using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Repository.Interfaces;
using Service.Helpers;
using Service.Interfaces;
using Service.Interfaces.Commands;
using Service.Interfaces.Storage;
using Service.Link.Arguments;
using Service.Models.Link;

using MediaType = Service.Models.Enums.MediaType;

namespace Service.Link
{
    public class CreateLinkCommand : ICommand<LinkModel, CreateLinkArgument>
	{
		private readonly IStorage storageService;
		private readonly IDomainRepository domainRepository;
		private readonly IMediaServiceRepository mediaServiceRepository;
		private readonly ILinkRepository linkRepository;
        private readonly IUniqueLinkService uniqueLinkService;

        public CreateLinkCommand(
			IStorage storageService,
			IDomainRepository domainRepository,
			IMediaServiceRepository mediaServiceRepository,
			ILinkRepository linkRepository,
            IUniqueLinkService uniqueLinkService)
		{
			this.storageService = storageService;
			this.domainRepository = domainRepository;
			this.mediaServiceRepository = mediaServiceRepository;
			this.linkRepository = linkRepository;
            this.uniqueLinkService = uniqueLinkService;
		}


		public async Task<LinkModel> ExecuteAsync(CreateLinkArgument argument)
		{
			var domain = await domainRepository.GetByIdAsync(argument.Link.DomainId);

			var code = argument.Link.Code;
			if (!string.IsNullOrEmpty(code))
			{
				if (!uniqueLinkService.IsValidLinkCode(domain.Name, argument.Link.Code))
				{
					throw new ArgumentException($"Shortlink {domain.Name}/{argument.Link.Code} is already in use.");
				}
			}
			else
			{
				code = uniqueLinkService.GetUniqueLinkShortCode(domain.Name);
			}

            Models.StorageModel.Base.StorageModel storageModel;
            switch (argument.Link.MediaType)
			{
				case MediaType.Music:
					var uniqMediaServiceIds = argument.MusicDestinations.SelectMany(x => x.Value.Select(d => d.MediaServiceId)).Distinct();
                    var mediaServices = await mediaServiceRepository.GetUniqueAsync(uniqMediaServiceIds);

                    storageModel = Mapper.Map<LinkModel, Models.StorageModel.Music.StorageModel>(argument.Link, o => o.AfterMap((link, storage) =>
                    {
                        storage.Destinations = argument.MusicDestinations.ToDictionary(
                            md => md.Key,
                            md => md.Value.Where(d => mediaServices.Select(m => m.Id).Contains(d.MediaServiceId))
                                        .Select(d => Mapper.Map<Models.Link.Music.DestinationModel, Models.StorageModel.Music.DestinationStorageModel>(d, opt => opt.AfterMap((music, str) =>
                                         {
                                             str.TrackingInfo.MediaServiceName = mediaServices.First(m => m.Id == d.MediaServiceId).Name;
                                         })))
                                         .ToList());
                    }));
					break;

				case MediaType.Ticket:
                    storageModel = Mapper.Map<LinkModel, Models.StorageModel.Ticket.StorageModel>(argument.Link, o => o.AfterMap((link, storage) =>
                    {
                        storage.Destinations = argument.TicketDestinations.ToDictionary(
                            md => md.Key,
                            md => md.Value.Select(Mapper.Map< Models.StorageModel.Ticket.DestinationStorageModel>).ToList());
                    }));
					break;

				default:
					throw new NotSupportedException($"Link type {argument.Link.MediaType} is not supported.");
			}
            
            var shortLink = LinkHelper.GetShortLink(domain.Name, code);
            var generalLinkPath = LinkHelper.GetLinkGeneralFilename(shortLink);

            try
            {
                await storageService.SaveAsync(generalLinkPath, storageModel);
            }
            catch (Exception ex)
            {
                // TODO: Normal logging like Serilog's Log.Error(ex, "...");
                Debug.WriteLine(ex);

                // No need to go further since we could not save the file on disk
                throw;
            }

            var dbLink = Mapper.Map<Repository.Entities.Link>(argument.Link);
            dbLink.MediaType = Mapper.Map<Repository.Entities.Enums.MediaType>(argument.Link.MediaType);
            dbLink.IsActive = true;
            dbLink.Id = Guid.NewGuid();
            dbLink.UpdatedAt = DateTime.UtcNow;

            try
            {
                dbLink = await linkRepository.CreateAsync(dbLink);
            }
            catch (Exception ex)
            {
                // TODO: Normal logging like Serilog's Log.Error(ex, "...");
                Debug.WriteLine(ex);

                // We need to delete the file since it is not valid as we have no db entity created
                storageService.Delete(shortLink);
                throw;
            }

            return Mapper.Map<LinkModel>(dbLink);
		}
	}
}
