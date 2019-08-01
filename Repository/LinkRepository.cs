using System;
using System.Linq;
using Repository.Entities;
using Repository.Interfaces;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Repository
{
    public class LinkRepository : ILinkRepository
    {
        private readonly Context _context;

        public LinkRepository(Context context)
        {
            _context = context;
        }


        public Link GetLink(Guid linkId)
        {
            var link = _context.Links.Include(x => x.Domain).FirstOrDefault(x => x.Id == linkId && x.IsActive);
            if (link == null)
            {
                throw new Exception($"Link {linkId} not found.");
            }

            return link;
        }

        public Link CreateLink(Link link)
        {
            _context.Domains.Attach(link.Domain);

            if (link.Artists?.Any() == true)
            {
                var linkArtistIds = link.Artists.Select(x => x.Id).ToList();
                var artists = _context.Artists.Where(x => linkArtistIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x);

                foreach (var artist in link.Artists)
                {
                    if (!artists.ContainsKey(artist.Id))
                        artists.Add(artist.Id, artist);
                }

                link.Artists = artists.Select(x => x.Value).ToList();
            }

            link.IsActive = true;
            _context.Links.Add(link);
            _context.SaveChanges();
            return link;
        }

        public Link UpdateLink(Link link)
        {
            //This would be a double check for Existing/Active Link
            var linkCheck = _context.Links.Include(x => x.Domain).FirstOrDefault(x => x.Id == link.Id && x.IsActive);

            if (linkCheck == null)
            {
                throw new Exception($"Link {link.Id} not found.");
            }

            var entry = _context.Entry(link);

            _context.Domains.Attach(link.Domain);

            //Entity State set to Modified so when Save Changes is called, it
            entry.State = EntityState.Modified;

            // make sure that next fields will be never modified on update
            entry.Property(x => x.MediaType).IsModified = false;
            entry.Property(x => x.IsActive).IsModified = false;

            /*
             * Snippet retrieved from https://docs.microsoft.com/en-us/ef/ef6/saving/concurrency
             * To handle EF Concurrency, it is, how to handle values updated at the same time
             * Following approach is Database Wins. Where the Values on the Database will prevail over the Client values.
             */
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    // Update the values of the entity that failed to save from the store
                    ex.Entries.Single().Reload();
                }

            } while (saveFailed);
            
            return link;
        }

        public Link DeleteLink(Guid linkId)
        {
            var link = _context.Links.Include(x => x.Domain).FirstOrDefault(x => x.Id == linkId);
            if (link == null)
            {
                throw new Exception($"Link {linkId} not found.");
            }
            link.IsActive = false;
            _context.SaveChanges();
            return link;
        }
    }
}
