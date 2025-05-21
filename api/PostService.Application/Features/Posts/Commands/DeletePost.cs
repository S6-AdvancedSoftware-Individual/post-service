using MediatR;
using PostService.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace PostService.Application.Features.Posts.Commands
{
    public class DeletePost
    {
        public record Command(Guid PostId) : IRequest<bool>;

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly IPostDbContext _context;
            private readonly IPostDbContextSecondary _contextSecondary;

            public Handler(IPostDbContext context, IPostDbContextSecondary contextSecondary)
            {
                _context = context;
                _contextSecondary = contextSecondary;

            }

            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var post = await _context.Posts.FindAsync(request.PostId);

                if (post == null)
                {
                    return false;
                }

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    _context.Posts.Remove(post);
                    await _context.SaveChangesAsync(cancellationToken);

                    _contextSecondary.Posts.Remove(post);
                    await _contextSecondary.SaveChangesAsync(cancellationToken);

                    scope.Complete();
                }

                return true;
            }
        }
    }
}
