using MediatR;
using Microsoft.AspNetCore.Mvc;
using PostService.Application.Features.Posts.Commands;
using PostService.Domain.Entities;

namespace PostService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PostsController(IMediator mediator)
        {
            _mediator = mediator;
        }

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
    }
}