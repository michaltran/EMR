using EMR.Domain.Enums;

namespace EMR.Domain.Entities;

public class ChuKy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaiLieuId { get; set; }
    public Guid NguoiKyId { get; set; }
    public string VaiTroKy { get; set; } = null!;
    public LoaiCa LoaiCa { get; set; }

    public string? SmartCa_TransactionId { get; set; }
    public string? SmartCa_TranCode { get; set; }
    public string? SmartCa_CertId { get; set; }
    public string? SmartCa_SerialNumber { get; set; }

    public string? CertSubject { get; set; }
    public DateTime? CertNotBefore { get; set; }
    public DateTime? CertNotAfter { get; set; }

    public string Sha256TruocKy { get; set; } = null!;
    public string? SignatureValue { get; set; }
    public string? TimestampSignature { get; set; }
    public string? DuongDanFileSau { get; set; }

    public TrangThaiChuKy TrangThai { get; set; } = TrangThaiChuKy.ChoXacNhan;
    public DateTime NgayYeuCau { get; set; } = DateTime.UtcNow;
    public DateTime? NgayHoanTat { get; set; }
    public string? LyDoLoi { get; set; }
    public string? WebhookPayloadRaw { get; set; }

    public TaiLieu TaiLieu { get; set; } = null!;
    public NguoiDung NguoiKy { get; set; } = null!;
}
