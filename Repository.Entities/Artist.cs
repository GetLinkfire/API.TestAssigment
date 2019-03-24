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

        public override bool Equals(object obj)
        {
            var artist = obj as Artist;
            return Name == artist?.Name && Label == artist?.Label;
        }

        public override int GetHashCode()
        {
            var hashCode = -1091674989;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Label);
            return hashCode;
        }
    }
}
