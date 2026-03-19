using GPMS.Infrastructure.Data.Seeding;
using GPMS.Infrastructure.Data.Seeding.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GPMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Data Seeders
        services.AddScoped<IDataSeeder, SchemaPatchSeeder>();
        services.AddScoped<IDataSeeder, UserSeeder>();
        services.AddScoped<IDataSeeder, ProjectSeeder>();
        services.AddScoped<IDataSeeder, GroupSeeder>();
        services.AddScoped<IDataSeeder, ReviewRoundSeeder>();
        services.AddScoped<IDataSeeder, ReviewerAssignmentSeeder>();
        services.AddScoped<IDataSeeder, SubmissionSeeder>();
        services.AddScoped<IDataSeeder, EvaluationSeeder>();
        services.AddScoped<IDataSeeder, NotificationSeeder>();
        services.AddScoped<DataSeederRunner>();

        return services;
    }
}
