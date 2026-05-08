using EMR.Domain.Entities;
using EMR.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Persistence;

public class EmrDbContext(DbContextOptions<EmrDbContext> options) : DbContext(options)
{
    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<NguoiDungVaiTro> NguoiDungVaiTros => Set<NguoiDungVaiTro>();
    public DbSet<Khoa> Khoas => Set<Khoa>();
    public DbSet<HoSoBenhAn> HoSoBenhAns => Set<HoSoBenhAn>();
    public DbSet<TaiLieu> TaiLieus => Set<TaiLieu>();
    public DbSet<ChuKy> ChuKys => Set<ChuKy>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(EmrDbContext).Assembly);
        EmrSeedData.Apply(b);
    }
}
