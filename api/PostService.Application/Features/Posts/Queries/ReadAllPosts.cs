using MediatR;
using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Domain.Entities;

namespace PostService.Application.Features.Posts.Queries
{
    public class ReadAllPosts
    {
        public record Query() : IRequest<List<Post>>;

        public class Handler : IRequestHandler<Query, List<Post>>
        {
            private readonly IPostDbContext _context;

            public Handler(IPostDbContext context)
            {
                _context = context;
            }

            public async Task<List<Post>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.Posts
                    .OrderByDescending(p => p.UpdatedAt)
                    .Take(25)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}