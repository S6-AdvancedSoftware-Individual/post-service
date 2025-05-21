using MediatR;
using Microsoft.AspNetCore.Mvc;
using PostService.Application.Features.Posts.Commands;
using PostService.Domain.Entities;
using Serilog;
using static PostService.Application.Features.Posts.Commands.SearchPosts;

namespace PostService.Api.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [EndpointDescription("Search for top 10 matching posts based on the 'query' parameter.")]
        [HttpGet("search")]
        public async Task<ActionResult<List<Post>>> Search([FromQuery] string q)
        {
            if(String.IsNullOrEmpty(q))
            {
                return BadRequest("No query supplied or query was poorly formatted.");
            } 

            if(q.Length > 50)
            {
                return BadRequest("Your search query has too many characters");
            }

            var result = await _mediator.Send(new SearchPostsCommand(q));

            if (result == null || result.Count == 0)
            {
                Log.Information("No posts found for search query '{Query}'", q);
                return Ok($"No posts found for search query '{q}'.");
            }

            Log.Information("Retrieved {Count} posts for search query '{Query}'", result.Count, q);

            return Ok(result);
        }

        [EndpointDescription("Create a new post with the given title, content, and author ID.")]
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePost.Command command)
        {
            var result = await _mediator.Send(command);

            if (result == Guid.Empty)
            {
                Log.Warning("Failed to create post with command {@Command}", command);
                return BadRequest("Failed to create post.");
            }

            Log.Information("Account created with ID '{Id}'", result);

            return Created(string.Empty, result);
        }

        [EndpointDescription("Retrieves the latest 50 posts.")]
        [HttpGet]
        public async Task<ActionResult<List<Post>>> GetAll()
        {
            var result = await _mediator.Send(new ReadAllPosts.Command());

            if (result == null || result.Count == 0)
            {
                Log.Warning("No posts found.");
                return NotFound("No posts found.");
            }

            Log.Information("Retrieved {Count} posts", result.Count);
            return Ok(result);
        }

        [EndpointDescription("Deletes a post by its ID.")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new DeletePost.Command(id));

            if (!result)
            {
                Log.Warning("Could not find post with ID '{Id}', deletion cancelled.", id);
                return NotFound();
            }

            Log.Information("Post with ID '{Id}' deleted successfully", id);
            return NoContent();
        }
    }
}