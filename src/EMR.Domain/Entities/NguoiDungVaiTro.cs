namespace EMR.Domain.Entities;

public class NguoiDungVaiTro
{
    public Guid NguoiDungId { get; set; }
    public Guid VaiTroId { get; set; }

    public NguoiDung NguoiDung { get; set; } = null!;
    public VaiTro VaiTro { get; set; } = null!;
}
