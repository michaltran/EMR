using EMR.Domain.Enums;

namespace EMR.Domain.Entities;

public class HoSoBenhAn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MaHoSo { get; set; } = null!;
    public string MaBenhNhanHIS { get; set; } = null!;
    public string? MaLanKhamHIS { get; set; }
    public string HoTenBenhNhan { get; set; } = null!;
    public DateTime? NgaySinh { get; set; }
    public byte? GioiTinh { get; set; }
    public Guid KhoaId { get; set; }
    public Guid BacSiTaoId { get; set; }
    public TrangThaiHoSo TrangThai { get; set; } = TrangThaiHoSo.Draft;
    public KhoLuuTru KhoLuuTru { get; set; } = KhoLuuTru.Khoa;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

    public Khoa Khoa { get; set; } = null!;
    public NguoiDung BacSiTao { get; set; } = null!;
    public ICollection<TaiLieu> TaiLieus { get; set; } = new List<TaiLieu>();
}
