using System;

namespace WebApp.Models
{
    public class LinkDto : CreateLinkDto
	{
		public Guid Id { get; set; }

		public string Web { get; set; }

		public string Mobile { get; set; }

		public string Artist { get; set; }

		public string Album { get; set; }

		public string SongTitle { get; set; }

        public bool IsActive { get; set; }
	}
}