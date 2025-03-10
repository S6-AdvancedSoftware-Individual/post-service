using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PostService.Application.Common.Interfaces;
using PostService.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace PostService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            services.AddDbContext<PostDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IPostDbContext>(provider => 
                provider.GetRequiredService<PostDbContext>());

            return services;
        }
    }
} 