using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NUnit.Framework;
using Repository.Entities;
using Repository.Exceptions;
using Repository.Interfaces;
using Rhino.Mocks;
using Service.Interfaces;
using Service.Interfaces.Storage;
using Service.Link;
using Service.Link.Arguments;
using Service.Models.Enums;
using Service.Models.Link;
using Service.Models.Link.Music;
using Service.Models.StorageModel.Music;

namespace Service.Tests.Link
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	public class UpdateLinkCommandTest
	{
		private IStorage _storageService;
        private IUniqueLinkService _uniqueLinkService;

        private ILinkRepository _linkRepository;
		private IDomainRepository _domainRepository;
		private IMediaServiceRepository _mediaServiceRepository;

		private UpdateLinkCommand _updateLinkCommand;

		[SetUp]
		public void Init()
		{
            AutoMapperConfig.Configure();

            _linkRepository = MockRepository.GenerateMock<ILinkRepository>();
			_domainRepository = MockRepository.GenerateMock<IDomainRepository>();
			_mediaServiceRepository = MockRepository.GenerateMock<IMediaServiceRepository>();

			_storageService = MockRepository.GenerateMock<IStorage>();
            _uniqueLinkService = new UniqueLinkService(_storageService);

            _updateLinkCommand = new UpdateLinkCommand(_storageService, _domainRepository, _mediaServiceRepository, _linkRepository, _uniqueLinkService);

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
		public async Task Execute_SameCode_Music()
		{
			var domainId = Guid.NewGuid();
			var code = "test";
			var domain = Builder<Domain>.CreateNew()
				.With(x => x.Id, domainId)
				.Build();
			var mediaServices = Builder<MediaService>.CreateListOfSize(2).Build().ToList();

			var existDbLink = Builder<Repository.Entities.Link>.CreateNew()
				.With(x => x.IsActive, true)
				.With(x => x.Code, code)
				.With(x => x.Domain, domain)
				.With(x => x.DomainId, domainId)
				.With(x => x.MediaType, Repository.Entities.Enums.MediaType.Music)
				.Build();
			var existStorageLink = Builder<StorageModel>.CreateNew()
				.With(x => x.MediaType, (MediaType)existDbLink.MediaType)
				.With(x => x.Id, existDbLink.Id)
				.With(x => x.Title, existDbLink.Title)
				.With(x => x.Url, existDbLink.Url)
				.With(x => x.TrackingInfo, Builder<TrackingStorageModel>.CreateNew().Build())
				.With(x => x.Destinations, new Dictionary<string, List<DestinationStorageModel>>()
				{
					{
						"all",
						Builder<DestinationStorageModel>.CreateListOfSize(1)
							.TheFirst(1)
							.With(x => x.MediaServiceId, mediaServices.First().Id)
							.With(x => x.TrackingInfo, Builder<TrackingStorageModel>.CreateNew()
								.With(x=>x.MediaServiceName,  mediaServices.First().Name)
								.With(x=>x.Web,  "url")
								.With(x=>x.Mobile, null)
								.Build())
							.Build().ToList()
					}
				})
				.Build();

			var argument = Builder<UpdateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<ExtendedLinkModel>.CreateNew()
					.With(x => x.Code, existDbLink.Code)
					.With(x => x.DomainId, existDbLink.DomainId)
					.With(x => x.MediaType, (MediaType)existDbLink.MediaType)
					.With(x => x.TicketDestinations, null)
					.With(x => x.TrackingInfo, null)
					.With(x => x.MusicDestinations, new Dictionary<string, List<DestinationModel>>()
					{
						{
							"all",
							Builder<DestinationModel>.CreateListOfSize(3)
								.All()
								.Do(x=>x.TrackingInfo = Builder<TrackingModel>.CreateNew().Build())
								.TheFirst(1)
								.With(x => x.MediaServiceId, mediaServices.First().Id)
								.TheNext(1)
								.With(x => x.MediaServiceId, mediaServices.Last().Id)
								.Build().ToList()
						}
					})
					.Build())
				.Build();

			_linkRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.Id))).Return(Task.FromResult(existDbLink));
			_domainRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(domainId))).Return(Task.FromResult(domain));

			_linkRepository.Expect(x =>
				x.UpdateAsync(Arg<Repository.Entities.Link>.Matches(entity =>
					entity.DomainId == existDbLink.DomainId
					&& entity.IsActive
					&& entity.MediaType == existDbLink.MediaType
					&& entity.Url == argument.Link.Url
					&& entity.Title == argument.Link.Title)))
				.Return(null)
				.WhenCalled(x =>
				{
                    x.ReturnValue = Task.FromResult((Repository.Entities.Link)x.Arguments[0]);
                });

			_mediaServiceRepository.Expect(x => x.GetUniqueAsync(Arg<IEnumerable<Guid>>.Is.Anything)).Return(Task.FromResult(mediaServices));

			_storageService.Expect(x => x.GetAsync<StorageModel>(Arg<string>.Matches(path => path.Equals($"{domain.Name}/{code}/general.json"))))
				.Return(Task.FromResult(existStorageLink));
			_storageService.Expect(x =>
				x.SaveAsync(Arg<string>.Matches(path => path.Equals($"{domain.Name}/{code}/general.json")),
					Arg<StorageModel>.Matches(st =>
						st.Destinations.ContainsKey("all")
						&& st.Destinations["all"].Count == mediaServices.Count
						&& st.Destinations["all"].First().TrackingInfo.MediaServiceName == mediaServices.First().Name)))
                 .Return(Task.FromResult(0));

			var result = await _updateLinkCommand.ExecuteAsync(argument);

			Assert.IsTrue(result.IsActive);
			Assert.AreEqual(domainId, result.DomainId);
			Assert.AreEqual(argument.Link.MediaType, result.MediaType);
			Assert.AreEqual(argument.Link.Title, result.Title);
			Assert.AreEqual(argument.Link.Url, result.Url);
			Assert.IsEmpty(result.Artists);
			Assert.IsNull(result.TicketDestinations);
			Assert.IsNotNull(result.TrackingInfo);

			Assert.AreEqual(2, result.MusicDestinations["all"].Count);
			Assert.AreEqual(argument.Link.MusicDestinations["all"].First().MediaServiceId, result.MusicDestinations["all"].First().MediaServiceId);
			Assert.AreEqual(argument.Link.MusicDestinations["all"].First().TrackingInfo.Web, result.MusicDestinations["all"].First().TrackingInfo.Web);
			Assert.AreEqual(argument.Link.MusicDestinations["all"].First().TrackingInfo.Mobile, result.MusicDestinations["all"].First().TrackingInfo.Mobile);
		}

		[Test]
		public async Task Execute_NewCode_Music()
		{
			var domainId = Guid.NewGuid();
			var oldCode = "test";
			var newCode = "test_updated";
			var domain = Builder<Domain>.CreateNew()
				.With(x => x.Id, domainId)
				.Build();
			var mediaServices = Builder<MediaService>.CreateListOfSize(2).Build().ToList();

			var existDbLink = Builder<Repository.Entities.Link>.CreateNew()
				.With(x => x.IsActive, true)
				.With(x => x.Code, oldCode)
				.With(x => x.Domain, domain)
				.With(x => x.DomainId, domainId)
				.With(x => x.MediaType, Repository.Entities.Enums.MediaType.Music)
				.Build();
			var existStorageLink = Builder<StorageModel>.CreateNew()
				.With(x => x.MediaType, (MediaType)existDbLink.MediaType)
				.With(x => x.Id, existDbLink.Id)
				.With(x => x.Title, existDbLink.Title)
				.With(x => x.Url, existDbLink.Url)
				.With(x => x.TrackingInfo, Builder<TrackingStorageModel>.CreateNew().Build())
				.With(x => x.Destinations, new Dictionary<string, List<DestinationStorageModel>>()
				{
					{
						"all",
						Builder<DestinationStorageModel>.CreateListOfSize(1)
							.TheFirst(1)
							.With(x => x.MediaServiceId, mediaServices.First().Id)
							.With(x => x.TrackingInfo, Builder<TrackingStorageModel>.CreateNew()
								.With(x=>x.MediaServiceName,  mediaServices.First().Name)
								.With(x=>x.Web,  "url")
								.With(x=>x.Mobile, null)
								.Build())
							.Build().ToList()
					}
				})
				.Build();

			var argument = Builder<UpdateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<ExtendedLinkModel>.CreateNew()
					.With(x => x.Code, newCode)
					.With(x => x.DomainId, existDbLink.DomainId)
					.With(x => x.MediaType, (MediaType)existDbLink.MediaType)
					.With(x => x.TicketDestinations, null)
					.With(x => x.TrackingInfo, Builder<TrackingModel>.CreateNew().Build())
					.With(x => x.MusicDestinations, new Dictionary<string, List<DestinationModel>>()
					{
						{
							"all",
							Builder<DestinationModel>.CreateListOfSize(3)
								.All()
								.Do(x=>x.TrackingInfo = Builder<TrackingModel>.CreateNew().Build())
								.TheFirst(1)
								.With(x => x.MediaServiceId, mediaServices.First().Id)
								.TheNext(1)
								.With(x => x.MediaServiceId, mediaServices.Last().Id)
								.Build().ToList()
						}
					})
					.Build())
				.Build();

			_linkRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.Id))).Return(Task.FromResult(existDbLink));
			_domainRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(domainId))).Return(Task.FromResult(domain));

			_linkRepository.Expect(x =>
				x.UpdateAsync(Arg<Repository.Entities.Link>.Matches(entity =>
					entity.DomainId == existDbLink.DomainId
					&& entity.IsActive
					&& entity.MediaType == existDbLink.MediaType
					&& entity.Url == argument.Link.Url
					&& entity.Title == argument.Link.Title)))
				.Return(null)
				.WhenCalled(x =>
				{
					x.ReturnValue = Task.FromResult((Repository.Entities.Link)x.Arguments[0]);
				});
			_storageService.Expect(x => x.GetFileList(Arg<string>.Is.Equal(domain.Name), Arg<string>.Is.Equal("test_updat"))).Return(Enumerable.Empty<string>().ToList());

			_mediaServiceRepository.Expect(x => x.GetUniqueAsync(Arg<IEnumerable<Guid>>.Is.Anything)).Return(Task.FromResult(mediaServices));

			_storageService.Expect(x => x.GetAsync<StorageModel>(Arg<string>.Matches(path => path.Equals($"{domain.Name}/{oldCode}/general.json"))))
				.Return(Task.FromResult(existStorageLink));
			_storageService.Expect(x =>
				x.SaveAsync(Arg<string>.Is.Equal($"{domain.Name}/{newCode}/general.json"),
					Arg<StorageModel>.Matches(st =>
						st.Destinations.ContainsKey("all")
						&& st.Destinations["all"].Count == mediaServices.Count
						&& st.Destinations["all"].First().TrackingInfo.MediaServiceName == mediaServices.First().Name)))
                .Return(Task.FromResult(0));
			_storageService.Expect(x => x.Delete(Arg<string>.Is.Equal($"{domain.Name}/{oldCode}")));

			var result = await _updateLinkCommand.ExecuteAsync(argument);

			Assert.IsTrue(result.IsActive);
			Assert.AreEqual(domainId, result.DomainId);
			Assert.AreEqual(argument.Link.MediaType, result.MediaType);
			Assert.AreEqual(argument.Link.Title, result.Title);
			Assert.AreEqual(argument.Link.Url, result.Url);
			Assert.IsEmpty(result.Artists);
			Assert.IsNull(result.TicketDestinations);

			Assert.AreEqual(argument.Link.TrackingInfo.Web, result.TrackingInfo.Web);
			Assert.AreEqual(argument.Link.TrackingInfo.Mobile, result.TrackingInfo.Mobile);

			Assert.AreEqual(2, result.MusicDestinations["all"].Count);
			Assert.AreEqual(argument.Link.MusicDestinations["all"].First().MediaServiceId, result.MusicDestinations["all"].First().MediaServiceId);
			Assert.AreEqual(argument.Link.MusicDestinations["all"].First().TrackingInfo.Web, result.MusicDestinations["all"].First().TrackingInfo.Web);
			Assert.AreEqual(argument.Link.MusicDestinations["all"].First().TrackingInfo.Mobile, result.MusicDestinations["all"].First().TrackingInfo.Mobile);
		}

		[Test]
		public async Task Execute_NewCode_Ticket()
		{
			var domainId = Guid.NewGuid();
			var oldCode = "test";
			var newCode = "test_updated";
			var domain = Builder<Domain>.CreateNew()
				.With(x => x.Id, domainId)
				.Build();

			var existDbLink = Builder<Repository.Entities.Link>.CreateNew()
				.With(x => x.IsActive, true)
				.With(x => x.Code, oldCode)
				.With(x => x.Domain, domain)
				.With(x => x.DomainId, domainId)
				.With(x => x.MediaType, Repository.Entities.Enums.MediaType.Ticket)
				.Build();
			var existStorageLink = Builder<Models.StorageModel.Ticket.StorageModel>.CreateNew()
				.With(x => x.MediaType, (MediaType)existDbLink.MediaType)
				.With(x => x.Id, existDbLink.Id)
				.With(x => x.Title, existDbLink.Title)
				.With(x => x.Url, existDbLink.Url)
				.With(x => x.Destinations, new Dictionary<string, List<Models.StorageModel.Ticket.DestinationStorageModel>>()
				{
					{
						"all",
						Builder<Models.StorageModel.Ticket.DestinationStorageModel>.CreateListOfSize(1)
							.Build().ToList()
					},
					{
						"se",
						Builder<Models.StorageModel.Ticket.DestinationStorageModel>.CreateListOfSize(2)
							.Build().ToList()
					}
				})
				.Build();

			int destinationInAllCount = 3;
			int destinationInDKCount = 1;
			var argument = Builder<UpdateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<ExtendedLinkModel>.CreateNew()
					.With(x => x.Code, newCode)
					.With(x => x.DomainId, existDbLink.DomainId)
					.With(x => x.MediaType, (MediaType)existDbLink.MediaType)
					.With(x => x.TicketDestinations, new Dictionary<string, List<Models.Link.Ticket.DestinationModel>>()
					{
						{
							"all",
							Builder<Models.Link.Ticket.DestinationModel>.CreateListOfSize(destinationInAllCount)
								.All()
								.Do(x=>x.ShowId = Guid.NewGuid())
								.TheFirst(1)
								.With(x => x.ShowId, existStorageLink.Destinations["all"].First().ShowId)
								.Build().ToList()
						},
						{
							"dk",
							Builder<Models.Link.Ticket.DestinationModel>.CreateListOfSize(destinationInDKCount)
								.All()
								.Do(x=>x.ShowId = Guid.NewGuid())
								.Build().ToList()
						}
					})
					.With(x => x.MusicDestinations, null)
					.Build())
				.Build();

			_linkRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.Id))).Return(Task.FromResult(existDbLink));
			_domainRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(domainId))).Return(Task.FromResult(domain));

			_linkRepository.Expect(x =>
				x.UpdateAsync(Arg<Repository.Entities.Link>.Matches(entity =>
					entity.DomainId == existDbLink.DomainId
					&& entity.IsActive
					&& entity.MediaType == existDbLink.MediaType
					&& entity.Url == argument.Link.Url
					&& entity.Title == argument.Link.Title)))
				.Return(null)
				.WhenCalled(x =>
				{
                    x.ReturnValue = Task.FromResult((Repository.Entities.Link)x.Arguments[0]);
                });
			_storageService.Expect(x => x.GetFileList(Arg<string>.Is.Equal(domain.Name), Arg<string>.Is.Equal("test_updat"))).Return(Enumerable.Empty<string>().ToList());


			_storageService.Expect(x => x.GetAsync<Models.StorageModel.Ticket.StorageModel>(Arg<string>.Matches(path => path.Equals($"{domain.Name}/{oldCode}/general.json"))))
				.Return(Task.FromResult(existStorageLink));
			_storageService.Expect(x =>
				x.SaveAsync(Arg<string>.Is.Equal($"{domain.Name}/{newCode}/general.json"),
					Arg<Models.StorageModel.Ticket.StorageModel>.Matches(st =>
						st.Destinations.ContainsKey("all")
						&& st.Destinations["all"].Count == destinationInAllCount
						&& st.Destinations["all"].Any(d => !string.IsNullOrEmpty(d.ExternalId)) // check if externalId is there
						&& st.Destinations["dk"].Count == destinationInDKCount
						&& !st.Destinations.ContainsKey("se"))))
                   .Return(Task.FromResult(0)); ;
			_storageService.Expect(x => x.Delete(Arg<string>.Is.Equal($"{domain.Name}/{oldCode}")));

			var result = await _updateLinkCommand.ExecuteAsync(argument);

			Assert.IsTrue(result.IsActive);
			Assert.AreEqual(domainId, result.DomainId);
			Assert.AreEqual(argument.Link.MediaType, result.MediaType);
			Assert.AreEqual(argument.Link.Title, result.Title);
			Assert.AreEqual(argument.Link.Url, result.Url);
			Assert.IsEmpty(result.Artists);
			Assert.IsNull(result.MusicDestinations);
		
			Assert.AreEqual(destinationInAllCount, result.TicketDestinations["all"].Count);
			Assert.AreEqual(argument.Link.TicketDestinations["all"].First().MediaServiceId, result.TicketDestinations["all"].First().MediaServiceId);
			Assert.AreEqual(argument.Link.TicketDestinations["all"].First().Url, result.TicketDestinations["all"].First().Url);
		}

		[Test]
		public void Execute_LinkNotFound()
		{
			var argument = Builder<UpdateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<ExtendedLinkModel>.CreateNew()
					.Build())
				.Build();

            _linkRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.Id)))
                .Throw(new NotFoundException(typeof(Repository.Entities.Link), argument.Link.Id));

            Assert.ThrowsAsync<NotFoundException>(async () =>
			{
				await _updateLinkCommand.ExecuteAsync(argument);
			});
		}

        // This test is not relevant anymore since MediaType is mapped by Automapper
        /*
		[Test]
		public void Execute_MediaTypeUpdateNotSupported()
		{
            var domainId = Guid.NewGuid();
            var domain = Builder<Domain>.CreateNew()
                .With(x => x.Id, domainId)
                .Build();

            var existDbLink = Builder<Repository.Entities.Link>.CreateNew()
				.With(x => x.IsActive, true)
				.With(x => x.MediaType, Repository.Entities.Enums.MediaType.Music)
				.Build();

			var argument = Builder<UpdateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<ExtendedLinkModel>.CreateNew()
					.With(x => x.MediaType, MediaType.Ticket)
					.Build())
				.Build();

            _domainRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.DomainId))).Return(Task.FromResult(domain));
            _linkRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.Id))).Return(Task.FromResult(existDbLink));

			Assert.ThrowsAsync<NotSupportedException>(async () =>
			{
				await _updateLinkCommand.ExecuteAsync(argument);
			});
		}
        */

		[Test]
		public void Execute_DomainNotFound()
		{
			var existDbLink = Builder<Repository.Entities.Link>.CreateNew()
				.With(x => x.IsActive, true)
				.With(x => x.MediaType, Repository.Entities.Enums.MediaType.Music)
				.Build();

			var argument = Builder<UpdateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<ExtendedLinkModel>.CreateNew()
					.With(x => x.MediaType, MediaType.Music)
					.Build())
				.Build();

			_linkRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.Id))).Return(Task.FromResult(existDbLink));

            _domainRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.DomainId)))
                .Throw(new NotFoundException(typeof(Domain), argument.Link.DomainId));

			Assert.ThrowsAsync<NotFoundException>(async () =>
			{
				await _updateLinkCommand.ExecuteAsync(argument);
			});
		}

		[Test]
		public void Execute_NewCode_Invalid()
		{
			var domainId = Guid.NewGuid();
			var domain = Builder<Domain>.CreateNew()
				.With(x => x.Id, domainId)
				.Build();

			var existDbLink = Builder<Repository.Entities.Link>.CreateNew()
				.With(x => x.IsActive, true)
				.With(x => x.MediaType, Repository.Entities.Enums.MediaType.Music)
				.With(x => x.Code, "oldcode")
				.With(x => x.DomainId, domainId)
				.With(x => x.Domain, domain)
				.Build();

			var argument = Builder<UpdateLinkArgument>.CreateNew()
				.With(x => x.Link, Builder<ExtendedLinkModel>.CreateNew()
					.With(x => x.MediaType, MediaType.Music)
					.With(x => x.Code, "codeup")
					.With(x => x.DomainId, domainId)
					.Build())
				.Build();

			_linkRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.Id))).Return(Task.FromResult(existDbLink));
			_domainRepository.Expect(x => x.GetByIdAsync(Arg<Guid>.Is.Equal(argument.Link.DomainId))).Return(Task.FromResult(domain));

			_storageService.Expect(x => x.GetFileList(Arg<string>.Is.Equal($"{domain.Name}"), Arg<string>.Is.Equal("code")))
				.Return(new List<string>() { $"{domain.Name}{Path.DirectorySeparatorChar}code" });

			Assert.ThrowsAsync<ArgumentException>(async () => { await _updateLinkCommand.ExecuteAsync(argument); });
		}
	}
}
