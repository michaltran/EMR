namespace EMR.Domain.Entities.His;

public class LanKham
{
    public string Ma { get; set; } = null!;
    public string MaBenhNhan { get; set; } = null!;
    public DateTime NgayVaoVien { get; set; }
    public DateTime? NgayRaVien { get; set; }
    public string? KhoaDieuTri { get; set; }
    public string? ChanDoanRaVien { get; set; }

    public BenhNhan BenhNhan { get; set; } = null!;
}
