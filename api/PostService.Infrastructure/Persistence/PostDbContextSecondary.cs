using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Domain.Entities;

namespace PostService.Infrastructure.Persistence;

public class PostDbContextSecondary : DbContext, IPostDbContextSecondary
{
    public PostDbContextSecondary(DbContextOptions<PostDbContextSecondary> options) : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Content).IsRequired();
        });
    }
}
