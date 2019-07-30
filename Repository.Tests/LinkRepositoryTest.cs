using FizzWare.NBuilder;
using NUnit.Framework;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
            using (var contex = new Context())
            {
                contex.Domains.Add(domain);
                contex.Artists.Add(artists.First());
                contex.SaveChanges();
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
            using (var contex = new Context())
            {
                contex.Domains.Add(domain);
                contex.SaveChanges();
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

        // TODO: implement DB link update + unit tests


        [Test]
        [TestCase("title1", "url1", 1)]
        [TestCase("title2", "url2", 2)]
        public void UpdateLink(string title, string url, int artistCount)
        {
            var domain = Builder<Domain>.CreateNew()
                .With(d => d.Id, Guid.NewGuid())
                .With(d => d.Name, "domain")
                .Build();

            var newDomain = Builder<Domain>.CreateNew()
                .With(d => d.Id, Guid.NewGuid())
                .With(d => d.Name, "new domain")
                .Build();



            // imitates existing entries
            using (var contex = new Context())
            {
                contex.Domains.Add(domain);
                contex.Domains.Add(newDomain);
                contex.SaveChanges();
            }

            var link = Builder<Link>.CreateNew()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Domain, domain)
                .With(x => x.DomainId, domain.Id)
                .Build();

            var entity = _repository.CreateLink(link);

            var artists = Builder<Artist>.CreateListOfSize(artistCount)
               .All()
               .Do(x => x.Id = Guid.NewGuid())
               .Build().ToList();


            link.Url = url;
            link.DomainId = newDomain.Id;
            link.Domain = newDomain;
            link.Artists = artists;
            link.Title = title;

            var updateEntity = _repository.UpdateLink(link);
            var saved = _context.Links.Find(entity.Id);

            Assert.AreEqual(saved.Url, url);
            Assert.AreEqual(saved.DomainId, newDomain.Id);
            Assert.AreEqual(saved.Artists.Count, link.Artists.Count);
            Assert.AreEqual(link.Title, title);
        }
    }
}
