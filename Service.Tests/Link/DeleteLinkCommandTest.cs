using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FizzWare.NBuilder;
using NUnit.Framework;
using Repository.Entities;
using Repository.Entities.Enums;
using Repository.Interfaces;
using Rhino.Mocks;
using Service.Interfaces.Storage;
using Service.Link;
using Service.Models.Link;
using Service.Models.Link.Music;
using Service.Models.StorageModel.Music;

namespace Service.Tests.Link
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class DeleteLinkCommandTest
    {
        private IStorage _storageService;
        private ILinkRepository _linkRepository;

        private DeleteLinkCommand _deleteLinkCommand;

        [SetUp]
        public void Init()
        {
            _linkRepository = MockRepository.GenerateMock<ILinkRepository>();
            _storageService = MockRepository.GenerateMock<IStorage>();

            _deleteLinkCommand = new DeleteLinkCommand(_storageService, _linkRepository);
        }

        [TearDown]
        public void VerifyAllExpectations()
        {
            _storageService.VerifyAllExpectations();
            _linkRepository.VerifyAllExpectations();
        }

        [Test]
        public void Execute_ActiveLink_Music()
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
                .With(x => x.MediaType, MediaType.Music)
                .Build();

            var existStorageLink = Builder<StorageModel>.CreateNew()
                .With(x => x.MediaType, existDbLink.MediaType)
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

            var argument = Builder<DeleteLinkArgument>.CreateNew()
                .With(x => x.LinkId, existDbLink.Id)
                .Build();

            _linkRepository.Expect(x => x.GetLink(Arg<Guid>.Is.Equal(argument.LinkId))).Return(existDbLink);

            _linkRepository.Expect(x =>
                x.DeleteLink(Arg<Guid>.Is.Equal(argument.LinkId))).Return(existDbLink);

            _storageService.Expect(x =>
                x.Get<StorageModel>(Arg<string>.Matches(path => path.Equals($"{domain.Name}/{code}/general.json"))))
                .Return(existStorageLink);

            //Set the Expectation for the Change of Folder.
            _storageService.Expect(x =>
                x.Save(Arg<string>.Matches(path => path.Equals($"{domain.Name}/{argument.LinkId.ToString()}/general.json")),
                    Arg<StorageModel>.Matches(st =>
                        st.Destinations.ContainsKey("all"))));

            _deleteLinkCommand.Execute(argument);
        }
    }
}
