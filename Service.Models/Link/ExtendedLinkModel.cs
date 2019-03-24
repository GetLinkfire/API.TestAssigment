using System.Collections.Generic;

namespace Service.Models.Link
{
    public class ExtendedLinkModel : LinkModel
	{
		public Music.TrackingModel TrackingInfo { get; set; }

		public Dictionary<string, List<Music.DestinationModel>> MusicDestinations { get; set; }

		public Dictionary<string, List<Ticket.DestinationModel>> TicketDestinations { get; set; }
	}
}
