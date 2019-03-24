using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Repository.Entities;

namespace Repository.Interfaces
{
    public interface IMediaServiceRepository
	{
		Task<List<MediaService>> GetUniqueAsync(IEnumerable<Guid> uniqueList);
	}
}
