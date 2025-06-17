using MediatR;
using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Domain.Entities;

namespace PostService.Application.Features.Posts.Queries;

public class ReadRecentPostsByUser
{
    public record Query(Guid AuthorId) : IRequest<List<Post>>;

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
                .Where(p => p.AuthorId == request.AuthorId)
                .Take(10)
                .ToListAsync(cancellationToken);
        }
    }
}
