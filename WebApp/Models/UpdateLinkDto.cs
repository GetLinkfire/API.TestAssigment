using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Service.Models;

namespace WebApp.Models
{
    public class UpdateLinkDto
    {
        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(100, MinimumLength = 2)]
        [RegularExpression("^(?=.*[a-zA-Z0-9])([a-zA-Z0-9-_]+)$")]
        public string Code { get; set; }

        [Required]
        public Guid DomainId { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }

        public List<ArtistModel> Artists { get; set; }

        public List<MusicDestinationDto> MusicDestinations { get; set; }

        public List<TicketDestinationDto> TicketDestinations { get; set; }
    }
}