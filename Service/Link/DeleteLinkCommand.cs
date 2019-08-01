using System;
using System.Collections.Generic;
using System.Linq;
using Repository.Entities;
using Repository.Entities.Enums;
using Repository.Interfaces;
using Service.Interfaces.Commands;
using Service.Interfaces.Storage;
using Service.Models.StorageModel.Music;

namespace Service.Link
{
    public class DeleteLinkCommand : ICommand<DeleteLinkArgument>
    {

        private readonly IStorage _storageService;
        private readonly ILinkRepository _linkRepository;

        public DeleteLinkCommand(
            IStorage storageService,
            ILinkRepository linkRepository)
        {
            _storageService = storageService;
            _linkRepository = linkRepository;
        }

        public void Execute(DeleteLinkArgument argument)
        {
            var dbLink = _linkRepository.GetLink(argument.LinkId);

            //if link is not active or not exists exception should be thrown
            if (!dbLink.IsActive)
            {
                throw new Exception(string.Format("The Link {0} is Inactive", argument.LinkId));
            }

            _linkRepository.DeleteLink(argument.LinkId);

            //link files should be moved to domain/{linkId}
            var newShortLink = LinkHelper.ShortLinkTemplate(dbLink.Domain.Name, argument.LinkId.ToString());
            var oldShortLink = LinkHelper.ShortLinkTemplate(dbLink.Domain.Name, dbLink.Code);

            var generalLinkPathToRead = LinkHelper.LinkGeneralFilenameTemplate(oldShortLink);
            var generalLinkPathToSave = LinkHelper.LinkGeneralFilenameTemplate(newShortLink);

            switch (dbLink.MediaType)
            {
                case MediaType.Music:

                    var musicStorage = _storageService.Get<Models.StorageModel.Music.StorageModel>(generalLinkPathToRead);
                    _storageService.Save(generalLinkPathToSave, musicStorage);

                    break;

                case MediaType.Ticket:

                    var ticketStorage = _storageService.Get<Models.StorageModel.Ticket.StorageModel>(generalLinkPathToRead);
                    _storageService.Save(generalLinkPathToSave, ticketStorage);

                    break;
            }

            /*
             * So the Link is not being phisically removed but moved to another folder. (Line 49, 56). Might create a IStorage.Move
             * implementation so we avoid the "Save -> Delete" at this layer.
             */
            _storageService.Delete(oldShortLink);
        }
    }

    public class DeleteLinkArgument
    {
        public Guid LinkId { get; set; }
    }
}
