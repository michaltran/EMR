using EMR.Domain.Enums;

namespace EMR.Domain.Entities;

public class TaiLieu
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HoSoBenhAnId { get; set; }
    public string LoaiTaiLieu { get; set; } = null!;
    public string TenFile { get; set; } = null!;
    public string DuongDanLuuTru { get; set; } = null!;
    public long KichThuoc { get; set; }
    public string MimeType { get; set; } = "application/pdf";
    public string Sha256 { get; set; } = null!;
    public TrangThaiKyTaiLieu TrangThaiKy { get; set; } = TrangThaiKyTaiLieu.ChuaKy;
    public Guid NguoiUploadId { get; set; }
    public DateTime NgayUpload { get; set; } = DateTime.UtcNow;

    public HoSoBenhAn HoSoBenhAn { get; set; } = null!;
    public NguoiDung NguoiUpload { get; set; } = null!;
    public ICollection<ChuKy> ChuKys { get; set; } = new List<ChuKy>();
}
