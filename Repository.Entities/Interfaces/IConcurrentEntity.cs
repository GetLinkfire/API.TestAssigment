using System;

namespace Repository.Entities.Interfaces
{
    public interface IConcurrentEntity<TKey> : IEntity<TKey>
    {
        byte[] RowVersion { get; set; }

        DateTimeOffset UpdatedAt { get; set; }
    }
}
