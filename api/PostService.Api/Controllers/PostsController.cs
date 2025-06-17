using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PostService.Application.Features.Posts.Commands;
using PostService.Application.Features.Posts.Queries;
using PostService.Domain.Entities;
using Serilog;
using System.Linq;
using static PostService.Application.Features.Posts.Commands.SearchPosts;

namespace PostService.Api.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostsController(IMediator mediator, IMemoryCache cache) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly IMemoryCache _cache = cache;

        [EndpointDescription("Search for top 10 matching posts based on the 'query' parameter.")]
        [HttpGet("search")]
        public async Task<ActionResult<List<Post>>> Search([FromQuery] string q)
        {
            if (String.IsNullOrEmpty(q))
            {
                return BadRequest("No query supplied or query was poorly formatted.");
            }

            if (q.Length > 50)
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
            const string cacheKey = "latest_posts";
            if (_cache.TryGetValue(cacheKey, out List<Post> cachedPosts))
            {
                Log.Information("Returned posts from cache.");
                return Ok(cachedPosts);
            }

            var result = await _mediator.Send(new ReadAllPosts.Query());

            if (result == null || result.Count == 0)
            {
                Log.Warning("No posts found.");
                return NotFound("No posts found.");
            }

            // Cache for 1 minute
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(1));

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

        [EndpointDescription("Deletes a post by its ID.")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFromUser([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new ReadRecentPostsByUser.Query(id));
            Log.Information("Retrieved {Count} posts by {Id}", result.Count, id);
            return Ok(result);
        }
    }
}