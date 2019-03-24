using System;
using System.ComponentModel.DataAnnotations;
using Repository.Entities.Interfaces;

namespace Repository.Entities
{
    public class Domain : IEntity<Guid>
	{
        public Guid Id { get; set; }

        [MaxLength(255)]
		public string Name { get; set; }
	}
}
