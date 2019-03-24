using System;

namespace Service.Models.Link.Ticket
{
    public class DestinationModel : Base.DestinationModel
    {
		/// <summary>
		/// unique identifier for ticket destination
		/// </summary>
		public Guid ShowId { get; set; }

		public string Url { get; set; }

		public DateTime Date { get; set; }

		public string Venue { get; set; }

		public string Location { get; set; }
	}
}
