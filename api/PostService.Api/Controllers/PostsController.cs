using MediatR;
using Microsoft.AspNetCore.Mvc;
using PostService.Application.Features.Posts.Commands;
using PostService.Domain.Entities;

namespace PostService.Api.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePost.Command command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<Post>>> GetAll()
        {
            var result = await _mediator.Send(new ReadAllPosts.Command());
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeletePost.Command(id));

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}