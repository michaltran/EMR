namespace EMR.Domain.Entities;

public class VaiTro
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ma { get; set; } = null!;
    public string Ten { get; set; } = null!;
    public string? MoTa { get; set; }

    public ICollection<NguoiDungVaiTro> NguoiDungs { get; set; } = new List<NguoiDungVaiTro>();
}
