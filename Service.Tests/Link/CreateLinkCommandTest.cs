using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NUnit.Framework;
using Repository.Entities;
using Repository.Interfaces;
using Rhino.Mocks;
using Service.Interfaces;
using Service.Interfaces.Storage;
using Service.Link;
using Service.Link.Arguments;
using Service.Models;
using Service.Models.Enums;
using Service.Models.Link;
using Service.Models.Link.Music;
using Service.Models.StorageModel.Music;

namespace Service.Tests.Link
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class CreateLinkCommandTest
	{
		private IStorage _storageService;
        private IUniqueLinkService _uniqueLinkService;

        private ILinkRepository _linkRepository;
		private IDomainRepository _domainRepository;
		private IMediaServiceRepository _mediaServiceRepository;

		private CreateLinkCommand _createLinkCommand;


		[SetUp]
		public void Init()
		{
            AutoMapperConfig.Configure();

            _linkRepository = MockRepository.GenerateMock<ILinkRepository>();
			_domainRepository = MockRepository.GenerateMock<IDomainRepository>();
			_mediaServiceRepository = MockRepository.GenerateMock<IMediaServiceRepository>();

			_storageService = MockRepository.GenerateMock<IStorage>();
            _uniqueLinkService = new UniqueLinkService(_storageService);

            _createLinkCommand = new CreateLinkCommand(_storageService, _domainRepository, _mediaServiceRepository, _linkRepository, _uniqueLinkService);

		}

		[TearDown]
		public void VerifyAllExpectations()
		{
			_storageService.VerifyAllExpectations();
			_linkRepository.VerifyAllExpectations();
			_domainRepository.VerifyAllExpectations();
			_mediaServiceRepository.VerifyAllExpectations();
		}

		[Test]
		public async Task Execute_WithoutCode_Music()
		{
			var domainId = Guid.NewGuid();
			var domain = Builder<Domain>.CreateNew()
				.With(x => x.Id, domainId)
				.Build();
			var mediaServices = Builder<MediaService>.CreateListOfSize(1).Build().ToList();

			var argument = Builder<CreateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<LinkModel>.CreateNew()
					.With(x => x.Code, null)
					.With(x => x.DomainId, domainId)
					.With(x => x.MediaType, MediaType.Music)
					.Build())
				.With(x => x.TicketDestinations, null)
				.With(x => x.MusicDestinations, new Dictionary<string, List<DestinationModel>>()
				{
					{
						"all",
						Builder<DestinationModel>.CreateListOfSize(3)
							.TheFirst(1)
							.With(x => x.MediaServiceId, mediaServices.First().Id)
							.Build().ToList()
					}
				})
				.Build();

			_domainRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(domainId))).Return(Task.FromResult(domain));
			_storageService.Expect(x => x.GetFileList(Arg<string>.Is.Equal(domain.Name), Arg<string>.Is.Anything)).Return(Enumerable.Empty<string>().ToList());

			_mediaServiceRepository.Expect(x => x.GetUniqueAsync(Arg<IEnumerable<Guid>>.Is.Anything)).Return(Task.FromResult(mediaServices));
			_linkRepository.Expect(x =>
				x.CreateAsync(Arg<Repository.Entities.Link>.Matches(entity =>
					entity.DomainId == domainId
					&& (byte)entity.MediaType == (byte)argument.Link.MediaType)))
				.Return(null)
				.WhenCalled(x =>
				{
					x.ReturnValue = Task.FromResult((Repository.Entities.Link)x.Arguments[0]);
				});

			_storageService.Expect(x =>
				x.SaveAsync(Arg<string>.Matches(path => path.StartsWith(domain.Name) && path.EndsWith("/general.json")),
					Arg<Models.StorageModel.Base.StorageModel>.Matches(st =>
						((StorageModel)st).Destinations.ContainsKey("all")
						&& ((StorageModel)st).Destinations["all"].Count == mediaServices.Count)))
                 .Return(Task.FromResult(0));

			var result = await _createLinkCommand.ExecuteAsync(argument);

			Assert.IsTrue(result.IsActive);
			Assert.AreEqual(domainId, result.DomainId);
			Assert.AreEqual(argument.Link.MediaType, result.MediaType);
			Assert.AreEqual(argument.Link.Title, result.Title);
			Assert.AreEqual(argument.Link.Url, result.Url);
			Assert.IsEmpty(result.Artists);
		}

		[Test]
		public async Task Execute_WithoutCode_Ticket()
		{
			var domainId = Guid.NewGuid();
			var domain = Builder<Domain>.CreateNew()
				.With(x => x.Id, domainId)
				.Build();

			var artists = Builder<ArtistModel>.CreateListOfSize(2).Build().ToList();

			var argument = Builder<CreateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<LinkModel>.CreateNew()
					.With(x => x.Code, null)
					.With(x => x.DomainId, domainId)
					.With(x => x.MediaType, MediaType.Ticket)
					.With(x => x.Artists, artists)
					.Build())
				.With(x => x.TicketDestinations, new Dictionary<string, List<Models.Link.Ticket.DestinationModel>>()
				{
					{
						"all",
						Builder<Models.Link.Ticket.DestinationModel>.CreateListOfSize(3).Build().ToList()
					}
				})
				.With(x => x.MusicDestinations, null)
				.Build();

			
			_domainRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(domainId))).Return(Task.FromResult(domain));
			_storageService.Expect(x => x.GetFileList(Arg<string>.Is.Equal(domain.Name), Arg<string>.Is.Anything)).Return(Enumerable.Empty<string>().ToList());

            _mediaServiceRepository.Expect(x => x.GetUniqueAsync(Arg<IEnumerable<Guid>>.Is.Anything)).Repeat.Never();
            _linkRepository.Expect(x =>
				x.CreateAsync(Arg<Repository.Entities.Link>.Matches(entity =>
					entity.DomainId == domainId
					&& (byte)entity.MediaType == (byte)argument.Link.MediaType)))
				.Return(null)
				.WhenCalled(x =>
				{
                    x.ReturnValue = Task.FromResult((Repository.Entities.Link)x.Arguments[0]);
                });

			_storageService.Expect(x =>
				x.SaveAsync(Arg<string>.Matches(path => path.StartsWith(domain.Name) && path.EndsWith("/general.json")),
					Arg<Models.StorageModel.Base.StorageModel>.Matches(st =>
						((Models.StorageModel.Ticket.StorageModel)st).Destinations.ContainsKey("all")
						&& ((Models.StorageModel.Ticket.StorageModel)st).Destinations["all"].Count == argument.TicketDestinations["all"].Count)))
                .Return(Task.FromResult(0)); ;

			var result = await _createLinkCommand.ExecuteAsync(argument);

			Assert.IsTrue(result.IsActive);
			Assert.AreEqual(domainId, result.DomainId);
			Assert.AreEqual(argument.Link.MediaType, result.MediaType);
			Assert.AreEqual(argument.Link.Title, result.Title);
			Assert.AreEqual(argument.Link.Url, result.Url);
			Assert.AreEqual(argument.Link.Artists.Count, result.Artists.Count);
		}

		[Test]
		public void Execute_NewCode_Invalid()
		{
			var domainId = Guid.NewGuid();
			var domain = Builder<Domain>.CreateNew()
				.With(x => x.Id, domainId)
				.Build();
			
			var argument = Builder<CreateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<LinkModel>.CreateNew()
					.With(x => x.MediaType, MediaType.Music)
					.With(x => x.Code, "codere")
					.With(x => x.DomainId, domainId)
					.Build())
				.Build();
			
			_domainRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.DomainId))).Return(Task.FromResult(domain));

			_storageService.Expect(x => x.GetFileList(Arg<string>.Is.Equal($"{domain.Name}"), Arg<string>.Is.Equal("code")))
				.Return(new List<string>() { $"{domain.Name}{Path.DirectorySeparatorChar}code" });

			Assert.ThrowsAsync<ArgumentException>(async () => { await _createLinkCommand.ExecuteAsync(argument); });
		}
	}
}
