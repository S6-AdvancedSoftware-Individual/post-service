using BetterStack.Logs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Domain;
using PostService.Domain.Entities;
using Serilog;
using System.Transactions;

namespace PostService.Application.Features.Posts.Commands;

public class CreatePost
{
    public record Command(string Title, string Content, Guid AuthorId) : IRequest<Guid>;

    public class Handler : IRequestHandler<Command, Guid>
    {
        private readonly IPostDbContext _context;
        private readonly IPostDbContextSecondary _contextSecondary;
        private readonly IAccountService _accountService;

        public Handler(IPostDbContext context, IAccountService accountService, IPostDbContextSecondary contextSecondary)
        {
            _context = context;
            _accountService = accountService;
            _contextSecondary = contextSecondary;
        }


        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            var authorName = await _accountService.GetAuthorNameAsync(request.AuthorId);

            var post = new Post
            {
                Id = Guid.Parse("08269e6c-cddb-4a0d-82f4-f33aeb02fce3"),
                AuthorId = request.AuthorId,
                AuthorName = authorName,
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync(cancellationToken);

                _contextSecondary.Posts.Add(post);
                await _contextSecondary.SaveChangesAsync(cancellationToken);

                scope.Complete();
            }

            return post.Id;
        }

    }
}
