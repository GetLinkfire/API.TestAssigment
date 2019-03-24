using System;
using System.Collections.Generic;
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
    public class UpdateLinkCommand : ICommand<ExtendedLinkModel, UpdateLinkArgument>
	{
		private readonly IStorage storageService;
		private readonly IDomainRepository domainRepository;
		private readonly IMediaServiceRepository mediaServiceRepository;
		private readonly ILinkRepository linkRepository;
        private readonly IUniqueLinkService uniqueLinkService;

        public UpdateLinkCommand(
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

		public async Task<ExtendedLinkModel> ExecuteAsync(UpdateLinkArgument argument)
		{
			var dbLink = await linkRepository.GetByIdAsync(argument.Link.Id);

			var domain = await domainRepository.GetByIdAsync(argument.Link.DomainId);
			string oldShortLink = null;

			if (dbLink.DomainId != argument.Link.DomainId ||
				!dbLink.Code.Equals(argument.Link.Code, StringComparison.InvariantCultureIgnoreCase))
			{
				oldShortLink = LinkHelper.GetShortLink(dbLink.Domain.Name, dbLink.Code);
				if (!uniqueLinkService.IsValidLinkCode(domain.Name, argument.Link.Code))
				{
					throw new ArgumentException($"Shortlink {domain.Name}/{argument.Link.Code} is already in use.");
				}
			}

            Mapper.Map(argument.Link, dbLink);
			dbLink.Domain = domain;
            dbLink.UpdatedAt = DateTime.UtcNow;

            dbLink = await linkRepository.UpdateAsync(dbLink);
            var result = Mapper.Map<ExtendedLinkModel>(dbLink);

			var shortLink = LinkHelper.GetShortLink(domain.Name, argument.Link.Code);
			var generalLinkPathToRead = LinkHelper.GetLinkGeneralFilename(oldShortLink ?? shortLink);
			var generalLinkPathToSave = LinkHelper.GetLinkGeneralFilename(shortLink);

			switch (result.MediaType)
			{
				case MediaType.Music:
                    var uniqMediaServiceIds = argument.Link.MusicDestinations.SelectMany(x => x.Value.Select(d => d.MediaServiceId)).Distinct();
                    var mediaServices = await mediaServiceRepository.GetUniqueAsync(uniqMediaServiceIds);

					var musicStorage = await storageService.GetAsync<Models.StorageModel.Music.StorageModel>(generalLinkPathToRead);

                    Mapper.Map(argument.Link, musicStorage, o => o.AfterMap((link, storage) =>
                    {
                        storage.Destinations = link.MusicDestinations.ToDictionary(
                            md => md.Key,
                            md => md.Value.Where(d => mediaServices.Select(m => m.Id).Contains(d.MediaServiceId))
                                        .Select(d => Mapper.Map<Models.Link.Music.DestinationModel, Models.StorageModel.Music.DestinationStorageModel>(d, opt => opt.AfterMap((music, str) =>
                                        {
                                            str.TrackingInfo.MediaServiceName = mediaServices.First(m => m.Id == d.MediaServiceId).Name;
                                        })))
                                         .ToList());
                    }));

                    await storageService.SaveAsync(generalLinkPathToSave, musicStorage);

                    result.TrackingInfo = Mapper.Map<Models.Link.Music.TrackingModel>(musicStorage.TrackingInfo);

					result.MusicDestinations =
						musicStorage.Destinations?.ToDictionary(
                            x => x.Key,
							x => x.Value.Select(Mapper.Map< Models.Link.Music.DestinationModel>).ToList());

					break;

				case MediaType.Ticket:
					var ticketStorage = await storageService.GetAsync<Models.StorageModel.Ticket.StorageModel>(generalLinkPathToRead);

					ticketStorage.Title = argument.Link.Title;
					ticketStorage.Url = argument.Link.Url;

					// removes keys that are not in argument
					var keysToRemove = new List<string>();
					foreach (var existedIsoCode in ticketStorage.Destinations.Keys)
					{
						if (!argument.Link.TicketDestinations.Any(x =>
							x.Key.Equals(existedIsoCode, StringComparison.InvariantCultureIgnoreCase)))
						{
							keysToRemove.Add(existedIsoCode);
						}
					}

					foreach (var key in keysToRemove)
					{
						ticketStorage.Destinations.Remove(key);
					}

					foreach (var ticketDestination in argument.Link.TicketDestinations)
					{
                        if (!ticketStorage.Destinations.Any(md => md.Key.Equals(ticketDestination.Key, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            ticketStorage.Destinations.Add(ticketDestination.Key, ticketDestination.Value.Select(Mapper.Map<Models.StorageModel.Ticket.DestinationStorageModel>).ToList());
                        }
                        else
                        {
                            var existedIsoCode = ticketStorage.Destinations
                                .Where(x => x.Key.Equals(ticketDestination.Key, StringComparison.InvariantCultureIgnoreCase))
                                .Select(x => x.Key).First();
                            var existedExternalIds = ticketStorage.Destinations[existedIsoCode].ToDictionary(x => x.ShowId, x => x.ExternalId);

                            ticketStorage.Destinations[existedIsoCode] = ticketDestination.Value
                                .Select(v => Mapper.Map<Models.Link.Ticket.DestinationModel, Models.StorageModel.Ticket.DestinationStorageModel>(v, o => o.AfterMap((src, target) =>
                               {
                                   target.ExternalId = existedExternalIds.ContainsKey(v.ShowId) ? existedExternalIds[v.ShowId] : null;
                               })))
                                .ToList();
                        }
					}

                    await storageService.SaveAsync(generalLinkPathToSave, ticketStorage);

                    result.TicketDestinations =
                        ticketStorage.Destinations?.ToDictionary(x => x.Key,
                            x => x.Value.Select(Mapper.Map<Models.Link.Ticket.DestinationModel>).ToList());
					break;
				default:
					throw new NotSupportedException($"Link type {argument.Link.MediaType} is not supported.");
			}

            if (oldShortLink != null)
            {
                storageService.Delete(oldShortLink);
            }

			return result;
		}
	}	
}
