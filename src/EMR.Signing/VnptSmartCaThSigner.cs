using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMR.Signing;

/// <summary>
/// Signer cho SmartCA TH (Tích hợp). Tự động gen OTP từ TOTP key, không cần user confirm.
/// Yêu cầu mỗi BS có entry trong VnptSmartCa:Users với CCCD + password + serial + TOTP key.
/// </summary>
public class VnptSmartCaThSigner(VnptSmartCaThClient client, IOptions<VnptSmartCaSettings> opt, ILogger<VnptSmartCaThSigner> logger) : IDocumentSigner
{
    private readonly VnptSmartCaSettings _s = opt.Value;

    public async Task<SignResult> SignHashAsync(byte[] sha256Hash, string signerCccd, string signerHoTen, CancellationToken ct = default)
    {
        if (!_s.Enabled) throw new InvalidOperationException("VnptSmartCa not enabled");
        if (string.IsNullOrEmpty(_s.SpId) || string.IsNullOrEmpty(_s.SpPassword))
            throw new InvalidOperationException("Thiếu SpId/SpPassword (3rd Party do VNPT cấp). Cập nhật appsettings.local.json");

        var u = _s.FindUser(signerCccd) ?? throw new InvalidOperationException(
            $"Không tìm thấy config SmartCA cho CCCD {signerCccd} trong VnptSmartCa:Users. " +
            "Bổ sung user vào appsettings.local.json hoặc dùng Mode=V1.");

        if (string.IsNullOrEmpty(u.TotpKey))
            throw new InvalidOperationException($"User {signerCccd} không có TotpKey — không dùng được SmartCA TH. Đổi sang Mode=V1.");
        if (string.IsNullOrEmpty(u.Password) || string.IsNullOrEmpty(u.SerialNumber))
            throw new InvalidOperationException($"User {signerCccd} thiếu Password hoặc SerialNumber");

        var tranId = Guid.NewGuid().ToString("N");
        var docId = Guid.NewGuid().ToString("N")[..16];
        var otp = TotpGenerator.Generate(u.TotpKey);
        logger.LogInformation("SmartCA TH: gen OTP cho CCCD={Cccd}", signerCccd);

        // Step 1: Sign request
        var signRes = await client.SignAsync(
            userCccd: u.Cccd,
            password: u.Password,
            otp: otp,
            transactionId: tranId,
            transactionDesc: $"EMR LC ký - {signerHoTen}",
            serialNumber: u.SerialNumber,
            docId: docId,
            hash: sha256Hash,
            ct);

        if (signRes is null || signRes.Status_Code != 200 || string.IsNullOrEmpty(signRes.Data?.Sad))
            throw new InvalidOperationException($"SmartCA TH sign failed: code={signRes?.Status_Code} msg={signRes?.Message}");

        var sad = signRes.Data.Sad!;

        // Step 2: Confirm
        var confirmRes = await client.ConfirmAsync(u.Cccd, u.Password, sad, tranId, ct);
        if (confirmRes is null || confirmRes.Status_Code != 200 || confirmRes.Data?.Signatures is null || confirmRes.Data.Signatures.Count == 0)
            throw new InvalidOperationException($"SmartCA TH confirm failed: code={confirmRes?.Status_Code} msg={confirmRes?.Message}");

        var sig = confirmRes.Data.Signatures.FirstOrDefault(s => s.Doc_Id == docId)
                  ?? confirmRes.Data.Signatures.First();
        if (string.IsNullOrEmpty(sig.Signature_Value))
            throw new InvalidOperationException("SmartCA TH: signature_value rỗng");

        // Cert subject + valid: tốt nhất là gọi /v1/credentials/get_certificate trước rồi cache, MVP để default
        return new SignResult(
            SignatureValueBase64: sig.Signature_Value!,
            CertSubject: $"CN={signerHoTen}, UID=CCCD:{signerCccd}, Issuer=VNPT-CA",
            CertSerialNumber: u.SerialNumber,
            NotBefore: DateTime.UtcNow.AddDays(-1),
            NotAfter: DateTime.UtcNow.AddYears(3),
            LoaiCa: "VNPT_SMARTCA_TH");
    }

    public Task<VerifyResult> VerifyAsync(byte[] sha256Hash, string signatureBase64, string certSubject, CancellationToken ct = default)
    {
        return Task.FromResult(new VerifyResult(true, certSubject, null,
            "Verify VNPT cert chưa implement đầy đủ — cần lưu cert_data trong ChuKy + parse X509 ở đây"));
    }
}
