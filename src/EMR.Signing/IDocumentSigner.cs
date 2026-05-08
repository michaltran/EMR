namespace EMR.Signing;

public record SignResult(
    string SignatureValueBase64,
    string CertSubject,
    string CertSerialNumber,
    DateTime NotBefore,
    DateTime NotAfter,
    string LoaiCa);

public record VerifyResult(bool IsValid, string? CertSubject, DateTime? NotAfter, string? Reason);

public interface IDocumentSigner
{
    Task<SignResult> SignHashAsync(byte[] sha256Hash, string signerCccd, string signerHoTen, CancellationToken ct = default);
    Task<VerifyResult> VerifyAsync(byte[] sha256Hash, string signatureBase64, string certSubject, CancellationToken ct = default);
}
