using EMR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMR.Infrastructure.Persistence.Configurations;

public class NguoiDungConfig : IEntityTypeConfiguration<NguoiDung>
{
    public void Configure(EntityTypeBuilder<NguoiDung> e)
    {
        e.ToTable("NguoiDung");
        e.HasKey(x => x.Id);
        e.Property(x => x.TenDangNhap).HasMaxLength(50).IsRequired();
        e.Property(x => x.MatKhauHash).HasMaxLength(500).IsRequired();
        e.Property(x => x.HoTen).HasMaxLength(200).IsRequired();
        e.Property(x => x.CCCD).HasMaxLength(20);
        e.Property(x => x.Email).HasMaxLength(200);
        e.Property(x => x.SoDienThoai).HasMaxLength(20);

        e.HasIndex(x => x.TenDangNhap).IsUnique();
        e.HasIndex(x => x.CCCD).IsUnique().HasFilter("[CCCD] IS NOT NULL");
        e.HasIndex(x => x.KhoaId);

        e.HasOne(x => x.Khoa).WithMany(k => k.NguoiDungs).HasForeignKey(x => x.KhoaId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class VaiTroConfig : IEntityTypeConfiguration<VaiTro>
{
    public void Configure(EntityTypeBuilder<VaiTro> e)
    {
        e.ToTable("VaiTro");
        e.HasKey(x => x.Id);
        e.Property(x => x.Ma).HasMaxLength(50).IsRequired();
        e.Property(x => x.Ten).HasMaxLength(100).IsRequired();
        e.Property(x => x.MoTa).HasMaxLength(500);
        e.HasIndex(x => x.Ma).IsUnique();
    }
}

public class NguoiDungVaiTroConfig : IEntityTypeConfiguration<NguoiDungVaiTro>
{
    public void Configure(EntityTypeBuilder<NguoiDungVaiTro> e)
    {
        e.ToTable("NguoiDung_VaiTro");
        e.HasKey(x => new { x.NguoiDungId, x.VaiTroId });
        e.HasOne(x => x.NguoiDung).WithMany(n => n.VaiTros).HasForeignKey(x => x.NguoiDungId);
        e.HasOne(x => x.VaiTro).WithMany(v => v.NguoiDungs).HasForeignKey(x => x.VaiTroId);
    }
}
