using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository
{
    internal class MediaServiceRepository : Base.Repository<MediaService>, IMediaServiceRepository
	{
        public MediaServiceRepository(LinksContext context) : base(context)
        {
        }

        public Task<List<MediaService>> GetUniqueAsync(IEnumerable<Guid> uniqueList) => FindAsync(x => uniqueList.Contains(x.Id));
    }
}
