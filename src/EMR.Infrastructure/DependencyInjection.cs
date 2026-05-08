using EMR.Infrastructure.Persistence;
using EMR.Infrastructure.Services;
using EMR.Infrastructure.Storage;
using EMR.Signing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.AddScoped<HoSoCodeGenerator>();

        // Signing strategy: chọn theo VnptSmartCa:Enabled
        services.Configure<VnptSmartCaSettings>(config.GetSection("VnptSmartCa"));
        services.AddHttpClient<VnptSmartCaClient>();

        services.AddSingleton<IDocumentSigner>(sp =>
        {
            var smartCaOpt = sp.GetRequiredService<IOptions<VnptSmartCaSettings>>().Value;
            if (smartCaOpt.Enabled)
            {
                // VnptSmartCaSigner cần HttpClient + IOptions + Logger -> tạo qua scope khi cần
                // Không thể giữ singleton scope của HttpClient → wrap thành scoped factory
                return new SmartCaProxySigner(sp);
            }
            return ActivatorUtilities.CreateInstance<SelfSignedDocumentSigner>(sp);
        });

        return services;
    }
}

/// <summary>
/// Wrapper để hỗ trợ HttpClient (scoped) trong context singleton IDocumentSigner.
/// Mỗi lần gọi sẽ resolve VnptSmartCaSigner từ DI scope mới.
/// </summary>
internal class SmartCaProxySigner(IServiceProvider sp) : IDocumentSigner
{
    public async Task<SignResult> SignHashAsync(byte[] sha256Hash, string signerCccd, string signerHoTen, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var inner = ActivatorUtilities.CreateInstance<VnptSmartCaSigner>(scope.ServiceProvider);
        return await inner.SignHashAsync(sha256Hash, signerCccd, signerHoTen, ct);
    }

    public async Task<VerifyResult> VerifyAsync(byte[] sha256Hash, string signatureBase64, string certSubject, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var inner = ActivatorUtilities.CreateInstance<VnptSmartCaSigner>(scope.ServiceProvider);
        return await inner.VerifyAsync(sha256Hash, signatureBase64, certSubject, ct);
    }
}
