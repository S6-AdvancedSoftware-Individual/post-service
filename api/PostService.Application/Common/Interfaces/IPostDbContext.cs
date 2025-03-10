using PostService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PostService.Application.Common.Interfaces
{
    public interface IPostDbContext
    {
        DbSet<Post> Posts { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
} 