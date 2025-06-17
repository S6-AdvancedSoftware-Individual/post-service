using MediatR;
using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Domain;
using PostService.Domain.Entities;
using Serilog;

namespace PostService.Application.Features.Posts.Commands;

public class CreatePost
{
    public record Command(string Title, string Content, string AuthorName, Guid AuthorId) : IRequest<Guid>;

    public class Handler : IRequestHandler<Command, Guid>
    {
        private readonly IPostDbContext _context;
        private readonly IPostDbContextSecondary _contextSecondary;

        public Handler(IPostDbContext context, IAccountService accountService, IPostDbContextSecondary contextSecondary)
        {
            _context = context;
            _contextSecondary = contextSecondary;
        }

        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {

            var post = new Post
            {
                Id = Guid.NewGuid(), // Use new Guid each time
                AuthorId = request.AuthorId,
                AuthorName = request.AuthorName,
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync(cancellationToken);

                try
                {
                    _contextSecondary.Posts.Add(post);
                    await _contextSecondary.SaveChangesAsync(cancellationToken);
                }
                catch (Exception exSecondary)
                {
                    Log.Error(exSecondary, "Failed to write to secondary database. Attempting to rollback primary insert.");

                    try
                    {
                        var postInPrimary = await _context.Posts.FirstOrDefaultAsync(p => p.Id == post.Id, cancellationToken);
                        if (postInPrimary != null)
                        {
                            _context.Posts.Remove(postInPrimary);
                            await _context.SaveChangesAsync(cancellationToken);
                        }

                        throw new ApplicationException("Post creation failed due to secondary DB error. Rolled back primary insert.");
                    }
                    catch (Exception rollbackEx)
                    {
                        Log.Error(rollbackEx, "Rollback of primary insert failed. Manual intervention may be required.");
                        throw;
                    }
                }

                return post.Id;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception in CreatePost handler.");
                throw;
            }
        }
    }
}
