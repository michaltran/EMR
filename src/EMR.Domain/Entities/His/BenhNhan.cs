namespace EMR.Domain.Entities.His;

public class BenhNhan
{
    public string Ma { get; set; } = null!;
    public string HoTen { get; set; } = null!;
    public DateTime? NgaySinh { get; set; }
    public byte? GioiTinh { get; set; }
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }
    public string? CCCD { get; set; }

    public ICollection<LanKham> LanKhams { get; set; } = new List<LanKham>();
}
