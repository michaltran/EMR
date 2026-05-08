using EMR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMR.Infrastructure.Persistence.Configurations;

public class KhoaConfig : IEntityTypeConfiguration<Khoa>
{
    public void Configure(EntityTypeBuilder<Khoa> e)
    {
        e.ToTable("Khoa");
        e.HasKey(x => x.Id);
        e.Property(x => x.Ma).HasMaxLength(20).IsRequired();
        e.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        e.Property(x => x.Nhom).HasConversion<byte>();
        e.HasIndex(x => x.Ma).IsUnique();
        e.HasOne(x => x.KhoaCha).WithMany(x => x.KhoaCons).HasForeignKey(x => x.KhoaChaId).OnDelete(DeleteBehavior.NoAction);
    }
}
