using MediatR;
using PostService.Application.Common.Interfaces;
using PostService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace PostService.Application.Features.Posts.Commands;

public class SearchPosts
{
    public record SearchPostsCommand(string Query) : IRequest<List<Post>>;

    public class SearchPostsHandler : IRequestHandler<SearchPostsCommand, List<Post>>
    {
        private readonly IPostDbContextSecondary _context;

        public SearchPostsHandler(IPostDbContextSecondary context)
        {
            _context = context;
        }

        public async Task<List<Post>> Handle(SearchPostsCommand request, CancellationToken cancellationToken)
        {
            var sanitizedQuery = SanitizeSearchQuery(request.Query);

            var sql = @"
                SELECT * 
                FROM posts
                WHERE fts @@ to_tsquery({0})
                ORDER BY ""CreatedAt"" DESC
                LIMIT 5;
            ";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var posts = await _context.Posts
                .FromSqlRaw(sql, sanitizedQuery)
                .ToListAsync(cancellationToken);

            stopwatch.Stop();
            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            // You can log or output elapsedSeconds as needed, e.g.:
            Console.WriteLine($"Query took {elapsedSeconds} seconds.");

            return posts;
        }

        private string SanitizeSearchQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return string.Empty;
            }

            var sanitized = query
                .Replace("'", "''") // Escape single quotes
                .Replace(";", "")   // Remove semicolons
                .Replace("--", "")  // Remove SQL single-line comments
                .Replace("/*", "")  // Remove SQL block comment start
                .Replace("*/", "")  // Remove SQL block comment end
                .Trim();

            // Remove any non-alphanumeric characters except spaces
            sanitized = string.Concat(sanitized.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)));

            // Collapse multiple spaces to a single space
            sanitized = Regex.Replace(sanitized, @"\s+", " ");

            sanitized = string.Join(" & ", sanitized.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            return sanitized;
        }
    }
}