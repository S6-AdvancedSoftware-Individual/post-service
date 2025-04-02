using MediatR;
using PostService.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PostService.Application.Features.Posts.Commands
{
    public class DeletePost
    {
        public record Command(Guid PostId) : IRequest<bool>;

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly IPostDbContext _context;

            public Handler(IPostDbContext context)
            {
                _context = context;
            }

            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var post = await _context.Posts.FindAsync(request.PostId);

                if (post == null)
                {
                    return false;
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }
}
