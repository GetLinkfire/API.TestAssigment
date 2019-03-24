using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public sealed class MusicDestinationDto : Base.DestinationDto
	{
		[Required]
		[Url]
		public string Web { get; set; }

		[Url]
		public string Mobile { get; set; }
		
		public string Artist { get; set; }

		public string Album { get; set; }

		public string SongTitle { get; set; }
	}
}