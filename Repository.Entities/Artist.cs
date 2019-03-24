using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Repository.Entities.Interfaces;

namespace Repository.Entities
{
    public class Artist : IEntity<Guid>
	{
        public Guid Id { get; set; }

		[StringLength(255)]
		public string Name { get; set; }
		
		[StringLength(255)]
		public string Label { get; set; }

        public ICollection<Link> Links { get; set; } = new HashSet<Link>();
    }
}
