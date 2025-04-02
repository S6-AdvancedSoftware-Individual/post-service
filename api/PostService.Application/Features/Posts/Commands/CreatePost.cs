using MediatR;
using PostService.Application.Common.Interfaces;
using PostService.Domain;
using PostService.Domain.Entities;

namespace PostService.Application.Features.Posts.Commands
{
    public class CreatePost
    {
        public record Command(string Title, string Content, Guid AuthorId) : IRequest<Guid>;

        public class Handler : IRequestHandler<Command, Guid>
        {
            private readonly IPostDbContext _context;
            private readonly IAccountService _accountService;

            public Handler(IPostDbContext context, IAccountService accountService)
            {
                _context = context;
                _accountService = accountService;
            }

            public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
            {
                var authorName = await _accountService.GetAuthorNameAsync(request.AuthorId);

                var post = new Post
                {
                    Id = Guid.NewGuid(),
                    AuthorId = request.AuthorId,
                    AuthorName = authorName,
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
