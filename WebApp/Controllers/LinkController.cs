using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using Service.Interfaces.Commands;
using Service.Link.Arguments;
using Service.Models.Link;
using WebApp.Helpers;
using WebApp.Models;

using Music = Service.Models.Link.Music;
using Ticket = Service.Models.Link.Ticket;

namespace WebApp.Controllers
{
    [RoutePrefix("links")]
	public class LinkController : ApiController
	{
		private readonly ICommand<LinkModel, CreateLinkArgument> createCommand;
		private readonly ICommand<ExtendedLinkModel, UpdateLinkArgument> updateCommand;
		private readonly ICommand<ExtendedLinkModel, GetLinkArgument> getCommand;
		private readonly ICommand<DeleteLinkArgument> deleteCommand;

		public LinkController(
			ICommand<LinkModel, CreateLinkArgument> createCommand,
			ICommand<ExtendedLinkModel, UpdateLinkArgument> updateCommand,
			ICommand<ExtendedLinkModel, GetLinkArgument> getCommand,
			ICommand<DeleteLinkArgument> deleteCommand)
		{
			this.createCommand = createCommand;
			this.updateCommand = updateCommand;
			this.getCommand = getCommand;
			this.deleteCommand = deleteCommand;
		}

		[HttpGet]
		[Route("{linkId:guid}")]
		[ResponseType(typeof(LinkDto))]
		public async Task<IHttpActionResult> Get([FromUri]Guid linkId)
		{
			var result = await getCommand.ExecuteAsync(new GetLinkArgument() { LinkId = linkId });

			var mapped = Mapper.Map<LinkDto>(result);
            MapDestinations(mapped, result);

            return Ok(mapped);
		}

		[HttpPost]
		[Route("")]
		[ResponseType(typeof(LinkDto))]
		public async Task<IHttpActionResult> Create([FromBody]CreateLinkDto link)
		{
            var result = await createCommand.ExecuteAsync(new CreateLinkArgument()
            {
                Link = Mapper.Map<LinkModel>(link),
                MusicDestinations = link.MusicDestinations.ToModelDictionary<Music.DestinationModel, MusicDestinationDto>(),
                TicketDestinations = link.TicketDestinations.ToModelDictionary<Ticket.DestinationModel, TicketDestinationDto>()
            });

            return Ok(Mapper.Map<LinkDto>(result));
		}

		[HttpPut]
		[Route("{linkId:guid}")]
		[ResponseType(typeof(LinkDto))]
		public async Task<IHttpActionResult> Update([FromUri]Guid linkId, [FromBody]UpdateLinkDto link)
		{
            var innerLink = Mapper.Map<ExtendedLinkModel>(link);
            innerLink.Id = linkId;

            var argument = new UpdateLinkArgument() { Link = innerLink };
            argument.Link.MusicDestinations = link.MusicDestinations.ToModelDictionary<Music.DestinationModel, MusicDestinationDto>();
            argument.Link.TicketDestinations = link.TicketDestinations.ToModelDictionary<Ticket.DestinationModel, TicketDestinationDto>();

			var result = await updateCommand.ExecuteAsync(argument);

            var mapped = Mapper.Map<LinkDto>(result);
            MapDestinations(mapped, result);

            return Ok(mapped);
		}

		[HttpDelete]
		[Route("{linkId:guid}")]
		public async Task<IHttpActionResult> Delete([FromUri]Guid linkId)
		{
			await deleteCommand.ExecuteAsync(new DeleteLinkArgument() { LinkId = linkId });
            return Ok();
		}

        private static void MapDestinations(LinkDto link, ExtendedLinkModel model)
        {
            link.MusicDestinations = model.MusicDestinations.ToDtoList<Music.DestinationModel, MusicDestinationDto>();
            link.TicketDestinations = model.TicketDestinations.ToDtoList<Ticket.DestinationModel, TicketDestinationDto>();
        }
	}
}
