using System;
using System.Diagnostics.CodeAnalysis;
using FizzWare.NBuilder;
using NUnit.Framework;
using Repository.Entities;
using Repository.Entities.Enums;
using Repository.Interfaces;
using Rhino.Mocks;
using Service.Interfaces.Storage;
using Service.Link;

namespace Service.Tests.Link
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class DeleteLinkCommandTest
	{
        private IStorage _storageService;

        private ILinkRepository _linkRepository;
        private IDomainRepository _domainRepository;
        private IMediaServiceRepository _mediaServiceRepository;

        private DeleteLinkCommand _deleteLinkCommand;

        [SetUp]
        public void Init()
        {
            _linkRepository = MockRepository.GenerateMock<ILinkRepository>();
            _domainRepository = MockRepository.GenerateMock<IDomainRepository>();
            _mediaServiceRepository = MockRepository.GenerateMock<IMediaServiceRepository>();

            _storageService = MockRepository.GenerateMock<IStorage>();

            _deleteLinkCommand = new DeleteLinkCommand(_storageService, _domainRepository, _mediaServiceRepository, _linkRepository);
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
        public void Execute_Delete_Valid_Link()
        {
            var domainId = Guid.NewGuid();
            var domain = Builder<Domain>.CreateNew()
                .With(x => x.Id, domainId)
                .Build();

            Guid linkId = Guid.NewGuid();
            var existDbLink = Builder<Repository.Entities.Link>.CreateNew()
                .With(x => x.Id, linkId)
                .With(x => x.IsActive, true)
                .With(x => x.MediaType, MediaType.Music)
                .With(x => x.Code, "oldcode")
                .With(x => x.DomainId, domainId)
                .With(x => x.Domain, domain)
                .Build();

            var argument = Builder<DeleteLinkArgument>.CreateNew()
                .With(x => x.LinkId, linkId)
                .Build();

            var result = _deleteLinkCommand.Execute(argument);

            Assert.IsNotNull(result);
        }

        [Test]
        public void Execute_Delete_Invalid_Link()
        {
            var invalidId = Guid.NewGuid();

            var argument = Builder<DeleteLinkArgument>.CreateNew()
                .With(x => x.LinkId, invalidId)
                .Build();

            var result = _deleteLinkCommand.Execute(argument);

            Assert.IsNull(result);
        }
    }
}
