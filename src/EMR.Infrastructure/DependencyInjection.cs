using EMR.Infrastructure.Persistence;
using EMR.Infrastructure.Services;
using EMR.Infrastructure.Storage;
using EMR.Signing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EMR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEmrInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<EmrDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("Emr")));

        services.AddDbContext<HisDemoDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("HisDemo")));

        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IDocumentSigner, SelfSignedDocumentSigner>();
        services.AddScoped<HoSoCodeGenerator>();

        return services;
    }
}
