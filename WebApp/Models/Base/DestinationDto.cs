using System;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Base
{
    public abstract class DestinationDto
    {
        [Required]
        public Guid MediaServiceId { get; set; }

        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string IsoCode { get; set; }
    }
}