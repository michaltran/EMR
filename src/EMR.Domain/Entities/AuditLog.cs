namespace EMR.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public DateTime ThoiGian { get; set; } = DateTime.UtcNow;
    public Guid? ActorId { get; set; }
    public string? ActorTen { get; set; }
    public string HanhDong { get; set; } = null!;
    public string? LoaiDoiTuong { get; set; }
    public Guid? DoiTuongId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Chitiet { get; set; }
}
