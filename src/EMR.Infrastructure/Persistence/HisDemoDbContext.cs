using EMR.Domain.Entities.His;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Persistence;

public class HisDemoDbContext(DbContextOptions<HisDemoDbContext> options) : DbContext(options)
{
    public DbSet<BenhNhan> BenhNhans => Set<BenhNhan>();
    public DbSet<LanKham> LanKhams => Set<LanKham>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<BenhNhan>(e =>
        {
            e.ToTable("BenhNhan");
            e.HasKey(x => x.Ma);
            e.Property(x => x.Ma).HasMaxLength(50);
            e.Property(x => x.HoTen).HasMaxLength(200).IsRequired();
            e.Property(x => x.DiaChi).HasMaxLength(500);
            e.Property(x => x.SoDienThoai).HasMaxLength(20);
            e.Property(x => x.CCCD).HasMaxLength(20);
        });

        b.Entity<LanKham>(e =>
        {
            e.ToTable("LanKham");
            e.HasKey(x => x.Ma);
            e.Property(x => x.Ma).HasMaxLength(50);
            e.Property(x => x.MaBenhNhan).HasMaxLength(50).IsRequired();
            e.Property(x => x.KhoaDieuTri).HasMaxLength(200);
            e.Property(x => x.ChanDoanRaVien).HasMaxLength(500);
            e.HasOne(x => x.BenhNhan).WithMany(b => b.LanKhams).HasForeignKey(x => x.MaBenhNhan);
        });
    }
}
