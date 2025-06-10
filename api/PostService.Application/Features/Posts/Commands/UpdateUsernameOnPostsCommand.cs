using MediatR;
using PostService.Application.Common.Interfaces;

namespace PostService.Application.Features.Posts.Commands;

public class UpdateUsernameOnPostsCommand
{
    public record Command(string UpdatedUsername, Guid AuthorId) : IRequest<Guid>;

    public class Handler : IRequestHandler<Command, Guid>
    {
        private readonly IPostDbContext _context;
        private readonly IPostDbContextSecondary _contextSecondary;

        public Handler(IPostDbContext context, IPostDbContextSecondary contextSecondary)
        {
            _context = context;
            _contextSecondary = contextSecondary;
        }

        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            // Get posts from primary context
            var posts = _context.Posts.Where(p => p.AuthorId == request.AuthorId).ToList();

            foreach (var post in posts)
            {
                post.AuthorName = request.UpdatedUsername;
            }

            // Get posts from secondary context
            var postsSecondary = _contextSecondary.Posts.Where(p => p.AuthorId == request.AuthorId).ToList();

            foreach (var post in postsSecondary)
            {
                post.AuthorName = request.UpdatedUsername;
            }

            // Save changes in both contexts
            await _context.SaveChangesAsync(cancellationToken);
            await _contextSecondary.SaveChangesAsync(cancellationToken);

            return request.AuthorId;
        }

    }
}
