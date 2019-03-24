using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Repository.Entities;
using Repository.Exceptions;

namespace Repository.Tests
{
    // InMemory tests example that might be used for Database-related tests
    // Init is still slow for unit-tests (4 sec) but still faster and no I/O with disk, DB is created in memory
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class LinkRepositoryInMemoryTests
    {
        private DbConnection connection;

        [SetUp]
        public void Setup()
        {
            connection = Effort.DbConnectionFactory.CreateTransient();

            var context = new LinksContext(connection);
            context.Database.CreateIfNotExists();
        }

        [Test]
        public async Task LinkRepository_Create_Should_Save_Artists([Values(0, 1, 2)]int presetArtistsCount)
        {
            var artists = presetArtistsCount > 0
                ? Builder<Artist>.CreateListOfSize(presetArtistsCount)
                    .All()
                    .Do(x => x.Id = Guid.NewGuid())
                    .Build().ToList()
                : Enumerable.Empty<Artist>().ToList();

            using (var context = await GetContextAsync())
            {
                if (artists.Any())
                {
                    context.Artists.Add(artists.First());
                    await context.SaveChangesAsync();
                }

                var link = GetLink(null, true, artists);

                var repo = new LinkRepository(context);

                // act
                await repo.CreateAsync(link);

                // assert
                var saved = await repo.GetByIdAsync(link.Id);

                saved.Should().NotBeNull();
                saved.IsActive.Should().BeTrue();
                saved.Artists.Should().BeEquivalentTo(artists);
            }
        }

        [Test]
        public async Task LinkRepository_GetById_Should_Return_If_Exists()
        {
            var id = Guid.NewGuid();
            var link = GetLink(id, true);

            using (var context = await GetContextAsync())
            {
                context.Links.Add(link);
                await context.SaveChangesAsync();

                // act
                var entity = await new LinkRepository(context).GetByIdAsync(id);

                // assert
                entity.Should().NotBeNull();
                entity.IsActive.Should().BeTrue();
            }
        }

        [Test]
        public async Task LinkRepository_GetById_Should_Throw_If_Not_Exists()
        {
            var id = Guid.NewGuid();
            using (var context = await GetContextAsync())
            {
                // act
                Func<Task> act = () => new LinkRepository(context).GetByIdAsync(id);

                // assert
                await act.Should().ThrowAsync<NotFoundException>();
            }
        }

        [Test]
        public async Task LinkRepository_Update_Should_Throw_If_Not_Active()
        {
            var link = GetLink(null, false);

            using (var context = await GetContextAsync())
            {
                // act
                Func<Task> act = () => new LinkRepository(context).UpdateAsync(link);

                // assert
                await act.Should().ThrowAsync<IllegalArgumentException>();
            }
        }

        [Test]
        public async Task LinkRepository_Delete_Should_Throw_If_Not_Active()
        {
            var id = Guid.NewGuid();
            var link = GetLink(id, false);

            using (var context = await GetContextAsync())
            {
                context.Links.Add(link);
                await context.SaveChangesAsync();

                // act
                Func<Task> act = () => new LinkRepository(context).DeleteAsync(id);

                // assert
                await act.Should().ThrowAsync<IllegalArgumentException>();
            }
        }

        private async Task<LinksContext> GetContextAsync()
        {
            var context = new LinksContext(connection);
            var domain = Builder<Domain>.CreateNew()
                .With(d => d.Id, DomainId)
                .With(d => d.Name, "domain")
                .Build();

            context.Domains.Add(domain);
            await context.SaveChangesAsync();

            return context;
        }

        private static Link GetLink(Guid? id, bool isActive, IEnumerable<Artist> artists = null) 
            => Builder<Link>.CreateNew()
                    .With(x => x.Id, id ?? Guid.NewGuid())
                    .With(x => x.IsActive, isActive)
                    .With(x => x.DomainId, DomainId)
                    .With(x => x.Artists, artists ?? Enumerable.Empty<Artist>())
                    .Build();

        private static Guid DomainId = Guid.NewGuid();
    }
}
