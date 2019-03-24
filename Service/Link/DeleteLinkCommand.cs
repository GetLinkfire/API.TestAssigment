using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Repository.Entities.Enums;
using Repository.Interfaces;
using Service.Helpers;
using Service.Interfaces.Commands;
using Service.Interfaces.Storage;
using Service.Link.Arguments;

namespace Service.Link
{
    public class DeleteLinkCommand: ICommand<DeleteLinkArgument>
	{
        private readonly IStorage storageService;
        private readonly ILinkRepository linkRepository;

        public DeleteLinkCommand(
            ILinkRepository linkRepository,
            IStorage storageService)
        {
            this.linkRepository = linkRepository;
            this.storageService = storageService;
        }

        public async Task ExecuteAsync(DeleteLinkArgument argument)
        {
            var link = await linkRepository.DeleteAsync(argument.LinkId);

            try
            {
                // Read content
                var oldShortLink = LinkHelper.GetShortLink(link.Domain.Name, link.Code);
                var content = await storageService.GetAsync(LinkHelper.GetLinkGeneralFilename(oldShortLink));
                
                // Save under domain/linkId folder
                var folder = LinkHelper.GetShortLink(link.Domain.Name, link.Id.ToString());
                await storageService.SaveAsync(LinkHelper.GetLinkGeneralFilename(folder), content);

                // remove old data
                storageService.Delete(oldShortLink);
            }
            catch (Exception ex)
            {
                // TODO: Logging
                Debug.WriteLine(ex);
                throw;
            }
        }
    }
}
