using EMR.Domain.Enums;

namespace EMR.Domain.Entities;

public class Khoa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Ma { get; set; } = null!;
    public string Ten { get; set; } = null!;
    public NhomKhoa Nhom { get; set; }
    public Guid? KhoaChaId { get; set; }
    public int ThuTu { get; set; }

    public Khoa? KhoaCha { get; set; }
    public ICollection<Khoa> KhoaCons { get; set; } = new List<Khoa>();
    public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
    public ICollection<HoSoBenhAn> HoSoBenhAns { get; set; } = new List<HoSoBenhAn>();
}
