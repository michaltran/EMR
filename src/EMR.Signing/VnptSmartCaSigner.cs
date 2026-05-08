using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMR.Signing;

/// <summary>
/// Production signer dùng VNPT SmartCA. Thay thế SelfSignedDocumentSigner khi VnptSmartCa:Enabled=true.
/// Lưu ý: SmartCA chỉ ký HASH (không upload file), workflow:
///   1. Tạo transaction_id
///   2. Gọi /v1/credentials/get_certificate (có thể cache theo CCCD trong 1 ngày)
///   3. Gọi /v1/signatures/sign với hash file
///   4. Poll /v1/signatures/sign/{tranCode}/status (hoặc nhận webhook)
///   5. Nhận signature_value -> trả về SignResult
/// User phải confirm trên app SmartCA -> bước 4 sẽ pending tới khi user confirm.
/// </summary>
public class VnptSmartCaSigner(VnptSmartCaClient client, IOptions<VnptSmartCaSettings> opt, ILogger<VnptSmartCaSigner> logger) : IDocumentSigner
{
    private readonly VnptSmartCaSettings _s = opt.Value;

    public async Task<SignResult> SignHashAsync(byte[] sha256Hash, string signerCccd, string signerHoTen, CancellationToken ct = default)
    {
        if (!_s.Enabled) throw new InvalidOperationException("VnptSmartCa not enabled. Bật appsettings: VnptSmartCa:Enabled=true");
        if (string.IsNullOrEmpty(_s.SpId) || string.IsNullOrEmpty(_s.SpPassword))
            throw new InvalidOperationException("Thiếu SpId / SpPassword trong VnptSmartCa settings");

        var tranId = Guid.NewGuid().ToString("N");
        var docId = Guid.NewGuid().ToString("N")[..16];

        // 1. Get cert
        var certRes = await client.GetCertificateAsync(signerCccd, tranId + "_cert", ct: ct);
        if (certRes is null || certRes.Status_Code != 200 || certRes.Data?.User_Certificates is null || certRes.Data.User_Certificates.Count == 0)
            throw new InvalidOperationException($"SmartCA: không lấy được cert cho CCCD {signerCccd}: {certRes?.Message}");

        var smartCaCert = certRes.Data.User_Certificates
            .FirstOrDefault(c => c.Service_Type == "SMARTCA" && c.Cert_Status?.Contains("hoạt động", StringComparison.OrdinalIgnoreCase) == true)
            ?? certRes.Data.User_Certificates.First();

        // 2. Request sign
        var signRes = await client.RequestSignAsync(signerCccd, tranId, $"Ký bệnh án EMR LC - {signerHoTen}", smartCaCert.Serial_Number ?? "", docId, sha256Hash, ct);
        if (signRes is null || signRes.Status_Code != 200 || signRes.Data?.Tran_Code is null)
            throw new InvalidOperationException($"SmartCA: yêu cầu ký thất bại: {signRes?.Message}");

        var tranCode = signRes.Data.Tran_Code;
        logger.LogInformation("SmartCA: chờ user confirm trên app, tranCode={TranCode}", tranCode);

        // 3. Poll status
        var deadline = DateTime.UtcNow.AddSeconds(_s.PollTimeoutSeconds);
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromSeconds(_s.PollIntervalSeconds), ct);
            var status = await client.GetSignStatusAsync(tranCode, ct);
            if (status is null) continue;
            if (status.Status_Code == 200 && status.Data?.Signatures?.Count > 0)
            {
                var sig = status.Data.Signatures.First(s => s.Doc_Id == docId);
                if (string.IsNullOrEmpty(sig.Signature_Value)) continue;
                return new SignResult(
                    SignatureValueBase64: sig.Signature_Value!,
                    CertSubject: smartCaCert.Cert_Subject ?? $"CCCD-{signerCccd}",
                    CertSerialNumber: smartCaCert.Serial_Number ?? "",
                    NotBefore: smartCaCert.Cert_Valid_From ?? DateTime.UtcNow,
                    NotAfter: smartCaCert.Cert_Valid_To ?? DateTime.UtcNow.AddYears(3),
                    LoaiCa: "VNPT_SMARTCA");
            }
        }
        throw new TimeoutException($"SmartCA: quá thời gian chờ user confirm ({_s.PollTimeoutSeconds}s)");
    }

    public Task<VerifyResult> VerifyAsync(byte[] sha256Hash, string signatureBase64, string certSubject, CancellationToken ct = default)
    {
        // Verify với VNPT cert: cần cert_data từ DB để tạo X509 và verify.
        // MVP: trả về Unknown (không bắt được lỗi false-positive). Production: lưu cert_data + chain rồi verify ở đây.
        return Task.FromResult(new VerifyResult(true, certSubject, null, "Verify VNPT cert chưa được implement đầy đủ — cần lưu cert_data từ get_certificate"));
    }
}
