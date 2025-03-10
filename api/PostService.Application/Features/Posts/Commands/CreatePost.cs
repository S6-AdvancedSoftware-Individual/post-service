using MediatR;
using PostService.Application.Common.Interfaces;
using PostService.Domain.Entities;

namespace PostService.Application.Features.Posts.Commands
{
    public class CreatePost
    {
        public record Command(string Title, string Content) : IRequest<Guid>;

        public class Handler : IRequestHandler<Command, Guid>
        {
            private readonly IPostDbContext _context;

            public Handler(IPostDbContext context)
            {
                _context = context;
            }

            public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
            {
                var post = new Post
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Content = request.Content,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync(cancellationToken);

                return post.Id;
            }
        }
    }
} 