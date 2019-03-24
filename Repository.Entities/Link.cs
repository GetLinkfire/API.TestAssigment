using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Repository.Entities.Enums;
using Repository.Entities.Interfaces;

namespace Repository.Entities
{
    public class Link : IConcurrentEntity<Guid>
    {
        private const string MainIndexName = "IX_Links_Title_Code_IsActive";
        private const string LinksCodeIndexName = "IX_Links_Code";

        public Guid Id { get; set; }

		[Index(MainIndexName, 1)]
		[StringLength(255)]
		public string Title { get; set; }

		[StringLength(100)]
		[Index(LinksCodeIndexName)]
		[Index(MainIndexName, 2)]
		public string Code { get; set; }

		[Index(MainIndexName, 3)]
		public bool IsActive { get; set; }

		public string Url { get; set; }

		public MediaType MediaType { get; set; }

		public Guid DomainId { get; set; }

		[ForeignKey(nameof(DomainId))]
		public Domain Domain { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public ICollection<Artist> Artists { get; set; } = new HashSet<Artist>();
    }
}
