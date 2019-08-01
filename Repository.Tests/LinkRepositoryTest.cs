using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FizzWare.NBuilder;
using NUnit.Framework;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class LinkRepositoryTest
    {
        private Context _context;
        private ILinkRepository _repository;

        [SetUp]
        public void Init()
        {
            _context = new Context();
            _repository = new LinkRepository(_context);
        }

        [TearDown]
        public void Cleanup()
        {
            _context.Dispose();
        }

        [Test]
        public void CreateLink_DomainExist_OneArtistExist()
        {
            var domain = Builder<Domain>.CreateNew()
                .With(d => d.Id, Guid.NewGuid())
                .With(d => d.Name, "domain")
                .Build();
            var artists = Builder<Artist>.CreateListOfSize(2)
                .All()
                .Do(x => x.Id = Guid.NewGuid())
                .Build().ToList();

            // imitates existing entries
            using (var context = new Context())
            {
                context.Domains.Add(domain);
                context.Artists.Add(artists.First());
                context.SaveChanges();
            }

            var link = Builder<Link>.CreateNew()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Domain, domain)
                .With(x => x.DomainId, domain.Id)
                .With(x => x.Artists, artists)
                .Build();

            var entity = _repository.CreateLink(link);

            var saved = _context.Links.Find(entity.Id);

            Assert.IsTrue(entity.IsActive);

            Assert.IsNotNull(saved);
            Assert.AreEqual(link.Artists.Count, saved.Artists.Count);

        }

        [Test]
        public void CreateLink_DomainExist_ArtistsAreNotExists()
        {
            var domain = Builder<Domain>.CreateNew()
                .With(d => d.Id, Guid.NewGuid())
                .With(d => d.Name, "domain")
                .Build();
            var artists = Builder<Artist>.CreateListOfSize(2)
                .All()
                .Do(x => x.Id = Guid.NewGuid())
                .Build().ToList();

            // imitates existing entries
            using (var context = new Context())
            {
                context.Domains.Add(domain);
                context.SaveChanges();
            }

            var link = Builder<Link>.CreateNew()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Domain, domain)
                .With(x => x.DomainId, domain.Id)
                .With(x => x.Artists, artists)
                .Build();

            var entity = _repository.CreateLink(link);

            var saved = _context.Links.Find(entity.Id);

            Assert.IsTrue(entity.IsActive);

            Assert.IsNotNull(saved);
            Assert.AreEqual(link.Artists.Count, saved.Artists.Count);
        }
                

        [Test]
        public void UpdateLink_BasicPropertiesUpdated()
        {
            SetupContext();

            var link = _context.Links.FirstOrDefault();

            link.Title = "TitleUpdated";
            link.Url = "UrlUpdated";

            _repository.UpdateLink(link);
            var updated = _context.Links.Find(link.Id);

            Assert.AreEqual(link.Title, updated.Title);
            Assert.AreEqual(link.Url, updated.Url);
        }

        /// <summary>
        /// Sets the Context to have one Link created with Media Type Ticket.
        /// </summary>
        private void SetupContext()
        {
            //Basically the same piece from above to Create a Link with Nonexistant Artists
            var domain = Builder<Domain>.CreateNew()
               .With(d => d.Id, Guid.NewGuid())
               .With(d => d.Name, "domain")
               .Build();
            var artists = Builder<Artist>.CreateListOfSize(2)
                .All()
                .Do(x => x.Id = Guid.NewGuid())
                .Build().ToList();

            // imitates existing entries
            using (var context = new Context())
            {
                context.Domains.Add(domain);
                context.SaveChanges();
            }

            var link = Builder<Link>.CreateNew()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Domain, domain)
                .With(x => x.DomainId, domain.Id)
                .With(x => x.Artists, artists)
                .With(x => x.MediaType, Entities.Enums.MediaType.Ticket)
                .Build();

            _repository.CreateLink(link);
        }
    }
}
