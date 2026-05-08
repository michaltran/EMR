namespace EMR.Domain.Entities;

public class NguoiDung
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenDangNhap { get; set; } = null!;
    public string MatKhauHash { get; set; } = null!;
    public string HoTen { get; set; } = null!;
    public string? CCCD { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public Guid? KhoaId { get; set; }
    public byte TrangThai { get; set; } = 1;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

    public Khoa? Khoa { get; set; }
    public ICollection<NguoiDungVaiTro> VaiTros { get; set; } = new List<NguoiDungVaiTro>();
}
