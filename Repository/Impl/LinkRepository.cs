using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Repository.Entities;
using Repository.Exceptions;
using Repository.Interfaces;

namespace Repository
{
    internal sealed class LinkRepository : Base.Repository<Link>, ILinkRepository
    {
        public LinkRepository(LinksContext context) : base(context)
        {
        }

        public new async Task<Link> GetByIdAsync(Guid linkId)
        {
            var entity = await Context.Links
                .Include(l => l.Domain)
                .Include(l => l.Artists)
                .SingleOrDefaultAsync(l => l.Id == linkId);

            EnsureLinkExists(entity, linkId);

            return entity;
        }

        public async Task<Link> CreateAsync(Link link)
		{
            await ProcessArtistsAsync(link);

            Context.Links.Add(link);

			await Context.SaveChangesAsync();

			return link;
		}

		public async Task<Link> UpdateAsync(Link link)
		{
            EnsureLinkIsActive(link);

            await ProcessArtistsAsync(link);

            // I'm not sure this should be here
            var entry = Context.Entry(link);
            // make sure that next fields will be never modified on update
            entry.Property(x => x.MediaType).IsModified = false;
            entry.Property(x => x.IsActive).IsModified = false;

            await Context.SaveChangesAsync();

            return link;
		}

		public async Task<Link> DeleteAsync(Guid id)
		{
            var link = await GetByIdAsync(id);

            EnsureLinkIsActive(link);
            
            link.IsActive = false;

            await Context.SaveChangesAsync();

            return link;
		}

        private async Task ProcessArtistsAsync(Link link)
        {
            if (link.Artists != null && link.Artists.Any())
            {
                var artistNames = link.Artists.Select(a => a.Name);
                var artistLabels = link.Artists.Select(a => a.Label);

                // TODO: think about, since there could many artists attached to the link, EF-generated query may be slow
                // ways to resolve: another approach (do not do bulk update of referenced entities at all) or play with Db Index
                var existingArtists = await Context.Artists.Where(a => artistNames.Contains(a.Name) && artistLabels.Contains(a.Label)).ToListAsync();

                foreach (var artist in link.Artists)
                {
                    var existing = existingArtists.FirstOrDefault(a => a.Name == artist.Name && a.Label == artist.Label);
                    if (existing == null)
                    {
                        artist.Id = Guid.NewGuid();
                        existingArtists.Add(artist);
                    }
                }

                link.Artists = existingArtists;

                //var linkArtistIds = link.Artists.Select(x => x.Id);
                //var artists = Context.Artists.Where(x => linkArtistIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x);

                //foreach (var artist in link.Artists)
                //{
                //    if (!artists.ContainsKey(artist.Id))
                //    {
                //        artists.Add(artist.Id, artist);
                //    }
                //}

                //link.Artists = artists.Select(x => x.Value).ToList();
            }
        }

        private static void EnsureLinkExists(Link link, Guid linkId)
        {
            if (link == null)
            {
                throw new NotFoundException(typeof(Link), linkId);
            }
        }

        private static void EnsureLinkIsActive(Link link)
        {
            if (!link.IsActive)
            {
                throw new IllegalArgumentException(typeof(Link), link.Id);
            }
        }
	}
}
