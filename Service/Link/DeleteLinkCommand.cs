using System;
using Repository.Interfaces;
using Service.Interfaces.Commands;
using Service.Interfaces.Storage;
using Service.Models.Link;

namespace Service.Link
{
	public class DeleteLinkCommand: ICommand<LinkModel, DeleteLinkArgument>
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
        public LinkModel Execute(DeleteLinkArgument argument)
		{
            var dbLink = _linkRepository.DeleteLink(argument.LinkId);
            return LinkHelper.GenerateLinkModel(dbLink);
        }
    }

	public class DeleteLinkArgument
	{
		public Guid LinkId { get; set; }
	}
}
