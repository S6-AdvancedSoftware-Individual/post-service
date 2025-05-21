using PostService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PostService.Application.Common.Interfaces;

public interface IPostDbContextSecondary
{
    DbSet<Post> Posts { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
