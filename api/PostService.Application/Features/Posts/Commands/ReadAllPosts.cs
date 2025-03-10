using MediatR;
using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Domain.Entities;

namespace PostService.Application.Features.Posts.Commands
{
    public class ReadAllPosts
    {
        public record Command() : IRequest<List<Post>>;

        public class Handler : IRequestHandler<Command, List<Post>>
        {
            private readonly IPostDbContext _context;

            public Handler(IPostDbContext context)
            {
                _context = context;
            }

            public async Task<List<Post>> Handle(Command request, CancellationToken cancellationToken)
            {
                return await _context.Posts.ToListAsync(cancellationToken);
            }
        }
    }
}