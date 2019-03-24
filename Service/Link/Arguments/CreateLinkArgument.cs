using System.Collections.Generic;
using Service.Models.Link;

namespace Service.Link.Arguments
{
    /// <summary>
    /// Data required for link creation
    /// </summary>
    public class CreateLinkArgument
    {
        public LinkModel Link { get; set; }

        public Dictionary<string, List<Models.Link.Music.DestinationModel>> MusicDestinations { get; set; }

        public Dictionary<string, List<Models.Link.Ticket.DestinationModel>> TicketDestinations { get; set; }
    }
}
