using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Repository.Entities
{
    public class Artist : IComparable<Artist>
    {

        public Guid Id { get; set; }
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Label { get; set; }

        public virtual ICollection<Link> Links { get; set; }

        public int CompareTo(Artist other)
        {
            var sum = Name.CompareTo(other.Name) + Label.CompareTo(other.Label) + Id.CompareTo(other.Id);
            if (sum != 0) return 1;
            return 0;
        }
    }    
}
