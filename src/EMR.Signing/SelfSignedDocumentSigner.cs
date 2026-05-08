using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EMR.Signing;

/// <summary>
/// Self-signed signer cho MVP — sinh 1 cert RSA-2048 cho mỗi BS (theo CCCD), lưu PFX vào storage.
/// Sau này thay bằng VnptSmartCaSigner (gọi VNPT API).
/// </summary>
public class SelfSignedDocumentSigner : IDocumentSigner
{
    private readonly string _certDir;
    private readonly string _pfxPassword;
    private readonly ILogger<SelfSignedDocumentSigner> _logger;
    private static readonly ConcurrentDictionary<string, X509Certificate2> Cache = new();

    public SelfSignedDocumentSigner(IConfiguration config, ILogger<SelfSignedDocumentSigner> logger)
    {
        var root = config["Storage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "storage");
        _certDir = Path.Combine(root, "_dev_certs");
        Directory.CreateDirectory(_certDir);
        _pfxPassword = config["Signing:DevPfxPassword"] ?? "DevOnly!2026";
        _logger = logger;
    }

    public Task<SignResult> SignHashAsync(byte[] sha256Hash, string signerCccd, string signerHoTen, CancellationToken ct = default)
    {
        var cert = GetOrCreateCert(signerCccd, signerHoTen);
        using var rsa = cert.GetRSAPrivateKey() ?? throw new InvalidOperationException("Cert không có private key");
        var sig = rsa.SignHash(sha256Hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return Task.FromResult(new SignResult(
            SignatureValueBase64: Convert.ToBase64String(sig),
            CertSubject: cert.Subject,
            CertSerialNumber: cert.SerialNumber,
            NotBefore: cert.NotBefore.ToUniversalTime(),
            NotAfter: cert.NotAfter.ToUniversalTime(),
            LoaiCa: "SELF_SIGNED"));
    }

    public Task<VerifyResult> VerifyAsync(byte[] sha256Hash, string signatureBase64, string certSubject, CancellationToken ct = default)
    {
        var cccd = ExtractCccdFromSubject(certSubject);
        if (cccd is null) return Task.FromResult(new VerifyResult(false, certSubject, null, "Không trích xuất được CCCD từ subject"));

        var path = Path.Combine(_certDir, $"{cccd}.pfx");
        if (!File.Exists(path)) return Task.FromResult(new VerifyResult(false, certSubject, null, "Không tìm thấy cert"));

        var cert = new X509Certificate2(path, _pfxPassword);
        using var rsa = cert.GetRSAPublicKey()!;
        var sig = Convert.FromBase64String(signatureBase64);
        var ok = rsa.VerifyHash(sha256Hash, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return Task.FromResult(new VerifyResult(
            IsValid: ok && DateTime.UtcNow <= cert.NotAfter.ToUniversalTime(),
            CertSubject: cert.Subject,
            NotAfter: cert.NotAfter.ToUniversalTime(),
            Reason: ok ? null : "Chữ ký không khớp hoặc cert hết hạn"));
    }

    private X509Certificate2 GetOrCreateCert(string cccd, string hoTen)
    {
        return Cache.GetOrAdd(cccd, _ =>
        {
            var path = Path.Combine(_certDir, $"{cccd}.pfx");
            if (File.Exists(path))
                return new X509Certificate2(path, _pfxPassword, X509KeyStorageFlags.Exportable);

            using var rsa = RSA.Create(2048);
            var subject = $"CN={EscapeDn(RemoveDiacritics(hoTen))}, OU=CCCD-{cccd}, O=TTYT Lien Chieu (DEV), C=VN";
            var req = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation, true));
            req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(3));

            File.WriteAllBytes(path, cert.Export(X509ContentType.Pfx, _pfxPassword));
            _logger.LogInformation("Tạo self-signed cert cho CCCD={Cccd} HoTen={HoTen} -> {Path}", cccd, hoTen, path);
            return new X509Certificate2(path, _pfxPassword, X509KeyStorageFlags.Exportable);
        });
    }

    private static string? ExtractCccdFromSubject(string subject)
    {
        var marker = "CCCD-";
        var i = subject.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (i < 0) return null;
        var s = subject[(i + marker.Length)..];
        var end = s.IndexOfAny([',', ' ']);
        return end < 0 ? s : s[..end];
    }

    private static string RemoveDiacritics(string s)
    {
        var norm = s.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(norm.Length);
        foreach (var c in norm)
        {
            var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }

    private static string EscapeDn(string s) => s.Replace(",", "\\,").Replace("+", "\\+").Replace("\"", "\\\"").Replace("\\", "\\\\").Replace("<", "\\<").Replace(">", "\\>").Replace(";", "\\;").Replace("=", "\\=");
}
