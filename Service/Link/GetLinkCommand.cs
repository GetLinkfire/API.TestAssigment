using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Repository.Interfaces;
using Service.Helpers;
using Service.Interfaces.Commands;
using Service.Interfaces.Storage;
using Service.Link.Arguments;
using Service.Models.Link;

using MediaType = Service.Models.Enums.MediaType;

namespace Service.Link
{
    public class GetLinkCommand : ICommand<ExtendedLinkModel, GetLinkArgument>
	{
		private readonly ILinkRepository linkRepository;
		private readonly IStorage storageService;

		public GetLinkCommand(
			IStorage storageService,
			ILinkRepository linkRepository)
		{
			this.storageService = storageService;
			this.linkRepository = linkRepository;
		}

		public async Task<ExtendedLinkModel> ExecuteAsync(GetLinkArgument argument)
		{
			var dbLink = await linkRepository.GetByIdAsync(argument.LinkId);

			var shortLink = LinkHelper.GetShortLink(dbLink.Domain.Name, dbLink.IsActive ? dbLink.Code : dbLink.Id.ToString());
			var generalLinkPath = LinkHelper.GetLinkGeneralFilename(shortLink);

            var result = Mapper.Map<ExtendedLinkModel>(dbLink);

			switch (result.MediaType)
			{
				case MediaType.Music:
					var musicStorage = await storageService.GetAsync<Models.StorageModel.Music.StorageModel>(generalLinkPath);

                    result.TrackingInfo = Mapper.Map<Models.Link.Music.TrackingModel>(musicStorage.TrackingInfo);

					result.MusicDestinations =
						musicStorage.Destinations?.ToDictionary(
                            x => x.Key,
							x => x.Value.Select(Mapper.Map<Models.Link.Music.DestinationModel>).ToList());

					break;
				case MediaType.Ticket:
					var ticketStorage = await storageService.GetAsync<Models.StorageModel.Ticket.StorageModel>(generalLinkPath);

					result.TicketDestinations =
						ticketStorage.Destinations?.ToDictionary(
                            x => x.Key,
							x => x.Value.Select(Mapper.Map<Models.Link.Ticket.DestinationModel>).ToList());
					break;
				default:
					throw new NotSupportedException($"Link type {dbLink.MediaType} is not supported.");
			}

			return result;
		}
	}
}
