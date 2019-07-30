using FizzWare.NBuilder;
using NUnit.Framework;
using Repository.Entities;
using Repository.Entities.Enums;
using Repository.Interfaces;
using Rhino.Mocks;
using Service.Interfaces.Storage;
using Service.Link;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Service.Tests.Link
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class DeleteLinkCommandTest
    {

        // TODO: implement delete link command + unit tests


        private IStorage _storageService;

        private ILinkRepository _linkRepository;
        private IDomainRepository _domainRepository;
        private IMediaServiceRepository _mediaServiceRepository;

        private CreateLinkCommand _createLinkCommand;
        private DeleteLinkCommand _deleteLinkCommand;

        [TearDown]
        public void VerifyAllExpectations()
        {
            _storageService.VerifyAllExpectations();
            _linkRepository.VerifyAllExpectations();
            _domainRepository.VerifyAllExpectations();
            _mediaServiceRepository.VerifyAllExpectations();
        }

        [SetUp]
        public void Init()
        {
            _linkRepository = MockRepository.GenerateMock<ILinkRepository>();
            _domainRepository = MockRepository.GenerateMock<IDomainRepository>();
            _mediaServiceRepository = MockRepository.GenerateMock<IMediaServiceRepository>();

            _storageService = MockRepository.GenerateMock<IStorage>();

            _createLinkCommand = new CreateLinkCommand(_storageService, _domainRepository, _mediaServiceRepository, _linkRepository);
            _deleteLinkCommand = new DeleteLinkCommand(_storageService, _domainRepository, _mediaServiceRepository, _linkRepository);

        }

        [Test]
        public void Execute_DeleteLink_Command()
        {
            var domainId = Guid.NewGuid();
            var domain = Builder<Domain>.CreateNew()
                .With(x => x.Id, domainId)
                .Build();

            var linkId = Guid.NewGuid();

            var existDbLink = Builder<Repository.Entities.Link>.CreateNew()
                .With(x => x.IsActive, true)
                .With(x => x.MediaType, MediaType.Music)
                .With(x => x.Code, "oldcode")
                .With(x => x.DomainId, domainId)
                .With(x => x.Domain, domain)
                .With(x => x.Id, linkId)
                .Build();

            var deleteLinkCommandArgument = Builder<DeleteLinkArgument>.CreateNew()
              .With(x => x.LinkId, linkId)
              .Build();
            _linkRepository.Expect(x => x.DeleteLink(Arg<Guid>.Is.Equal(linkId))).Return(existDbLink);


            _deleteLinkCommand.Execute(deleteLinkCommandArgument);

            _linkRepository.AssertWasCalled(x => x.DeleteLink(linkId));
        }
    }
}
