using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NUnit.Framework;
using Repository.Entities;

namespace Repository.Tests
{
    // This is not a unit test, this is integration test, or system test or whatever but not a unit-test
    // Unit-test are supposed to be instantly fast and work in memory
    // It works with actual SQL database on disk (!!!), it is REALLY slow (!!!)
    // If I have time I'll create a repository unit test with InMemory db using Effort
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class LinkRepositoryTest
	{
        [Test]
        public async Task CreateLink_DomainExist_OneArtistExist()
        {
            var domain = Builder<Domain>.CreateNew()
                .With(d => d.Id, Guid.NewGuid())
                .With(d => d.Name, "domain")
                .Build();
            var artists = Builder<Artist>.CreateListOfSize(2)
                .All()
                .Do(x => x.Id = Guid.NewGuid())
                .Build().ToList();

            using (var context = new LinksContext())
            {
                // imitates existing entries
                context.Domains.Add(domain);
                context.Artists.Add(artists.First());
                await context.SaveChangesAsync();

                var link = Builder<Link>.CreateNew()
                    .With(x => x.Id, Guid.NewGuid())
                    .With(x => x.IsActive, true)
                    .With(x => x.Domain, domain)
                    .With(x => x.DomainId, domain.Id)
                    .With(x => x.Artists, artists)
                    .Build();

                var entity = await new LinkRepository(context).CreateAsync(link);

                var saved = context.Links.Find(entity.Id);

                Assert.IsTrue(entity.IsActive);

                Assert.IsNotNull(saved);
                Assert.AreEqual(link.Artists.Count, saved.Artists.Count);
            }
        }

        [Test]
        public async Task CreateLink_DomainExist_ArtistsAreNotExists()
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
            using (var context = new LinksContext())
            {
                context.Domains.Add(domain);
                await context.SaveChangesAsync();

                var link = Builder<Link>.CreateNew()
                    .With(x => x.Id, Guid.NewGuid())
                    .With(x => x.Domain, domain)
                    .With(x => x.IsActive, true)
                    .With(x => x.DomainId, domain.Id)
                    .With(x => x.Artists, artists)
                    .Build();

                var entity = await new LinkRepository(context).CreateAsync(link);

                var saved = context.Links.Find(entity.Id);

                Assert.IsTrue(entity.IsActive);

                Assert.IsNotNull(saved);
                Assert.AreEqual(link.Artists.Count, saved.Artists.Count);
            }
        }

		// TODO: implement DB link update + unit tests
	}
}
