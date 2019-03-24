using System.ComponentModel.DataAnnotations;
using Repository.Entities.Enums;

namespace WebApp.Models
{
    public class CreateLinkDto : UpdateLinkDto
    {
        [Required]
        public MediaType MediaType { get; set; }
    }
}