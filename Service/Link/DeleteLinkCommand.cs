using Repository.Interfaces;
using Service.Interfaces.Commands;
using Service.Interfaces.Storage;
using System;

namespace Service.Link
{
    public class DeleteLinkCommand : ICommand<DeleteLinkArgument>
    {
        private readonly IStorage _storageService;
        private readonly IDomainRepository _domainRepository;
        private readonly IMediaServiceRepository _mediaServiceRepository;
        private readonly ILinkRepository _linkRepository;

        public DeleteLinkCommand(
            IStorage storageService,
            IDomainRepository domainRepository,
            IMediaServiceRepository mediaServiceRepository,
            ILinkRepository linkRepository)
        {
            _storageService = storageService;
            _domainRepository = domainRepository;
            _mediaServiceRepository = mediaServiceRepository;
            _linkRepository = linkRepository;
        }
        public void Execute(DeleteLinkArgument argument)
        {
            var link = _linkRepository.DeleteLink(argument.LinkId);
            var shortLink = LinkHelper.ShortLinkTemplate(link.Domain.Name, link.Code);
            string generalLinkPath = LinkHelper.LinkGeneralFilenameTemplate(shortLink);
            _storageService.Delete(shortLink);
        }
    }

    public class DeleteLinkArgument
    {
        public Guid LinkId { get; set; }
    }
}
