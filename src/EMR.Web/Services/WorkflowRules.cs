using EMR.Domain.Entities;
using EMR.Domain.Enums;
using EMR.Web.Catalog;

namespace EMR.Web.Services;

public record SignAvailability(
    string? VaiTroKyKeTiep,
    bool DangChoCapNhat,
    string? LyDoChan);

public static class WorkflowRules
{
    /// <summary>
    /// Tính vai trò TIẾP THEO cần ký cho 1 tài liệu, dựa trên loại biểu mẫu + chữ ký đã có.
    /// Trả về null nếu đã ký đủ.
    /// </summary>
    public static string? VaiTroKyKeTiep(string loaiTaiLieu, IEnumerable<ChuKy> chuKysHoanTat)
    {
        var bm = BieuMauCatalog.Find(loaiTaiLieu);
        if (bm is null || bm.VaiTroKyTuanTu.Length == 0) return null;

        var daKy = chuKysHoanTat.Where(c => c.TrangThai == TrangThaiChuKy.DaKy)
                                .Select(c => c.VaiTroKy).ToHashSet();
        return bm.VaiTroKyTuanTu.FirstOrDefault(r => !daKy.Contains(r));
    }

    /// <summary>
    /// Kiểm tra xem 1 user có thể ký 1 tài liệu cụ thể với vai trò gì.
    /// </summary>
    public static SignAvailability KiemTraQuyenKy(
        TaiLieu taiLieu,
        IEnumerable<string> userRoles,
        Guid userId,
        Guid hoSoKhoaId,
        Guid? userKhoaId)
    {
        if (taiLieu.TrangThaiKy == TrangThaiKyTaiLieu.DangKy)
            return new(null, true, "Tài liệu đang trong quá trình ký");

        var keTiep = VaiTroKyKeTiep(taiLieu.LoaiTaiLieu, taiLieu.ChuKys);
        if (keTiep is null)
            return new(null, false, "Tài liệu đã đủ chữ ký theo workflow");

        // Người dùng phải có vai trò ke tiep
        if (!userRoles.Contains(keTiep))
            return new(keTiep, false, $"Bạn không có vai trò {keTiep} (vai trò cần ký kế tiếp)");

        // Cùng người không ký 2 chữ ký với cùng vai trò
        var daKyVaiTroNay = taiLieu.ChuKys.Any(c =>
            c.TrangThai == TrangThaiChuKy.DaKy && c.VaiTroKy == keTiep && c.NguoiKyId == userId);
        if (daKyVaiTroNay)
            return new(keTiep, false, "Bạn đã ký tài liệu này với vai trò đó rồi");

        // BS và TK chỉ ký được hồ sơ thuộc khoa của mình; LĐ và KHTH ký được mọi khoa
        if (keTiep is BieuMauCatalog.ROLE_BACSI or BieuMauCatalog.ROLE_DIEUDUONG or
            BieuMauCatalog.ROLE_DUOCSI or BieuMauCatalog.ROLE_TRUONGKHOA)
        {
            if (userKhoaId.HasValue && userKhoaId.Value != hoSoKhoaId)
                return new(keTiep, false, "Hồ sơ không thuộc khoa của bạn");
        }

        return new(keTiep, false, null);
    }

    /// <summary>
    /// Kiểm tra hồ sơ đã đủ điều kiện hoàn tất chưa (mọi biểu mẫu BatBuoc đã đủ chữ ký).
    /// </summary>
    public static (bool DuDieuKien, string[] ThieuBieuMau, string[] ChuaDuChuKy) DanhGiaHoanTat(HoSoBenhAn hoSo)
    {
        var thieu = new List<string>();
        var thieuChuKy = new List<string>();

        foreach (var bm in BieuMauCatalog.Items.Where(x => x.BatBuoc))
        {
            var tls = hoSo.TaiLieus.Where(t => t.LoaiTaiLieu == bm.Ma).ToList();
            if (tls.Count == 0)
            {
                thieu.Add(bm.Ten);
                continue;
            }
            // tài liệu đầu tiên (hoặc bất kỳ) phải đủ chữ ký
            var hasFullySigned = tls.Any(t => VaiTroKyKeTiep(t.LoaiTaiLieu, t.ChuKys) is null);
            if (!hasFullySigned) thieuChuKy.Add(bm.Ten);
        }

        return (thieu.Count == 0 && thieuChuKy.Count == 0, thieu.ToArray(), thieuChuKy.ToArray());
    }

    /// <summary>
    /// Tính trạng thái mới của hồ sơ sau khi 1 chữ ký được tạo.
    /// </summary>
    public static TrangThaiHoSo TinhTrangThaiHoSo(HoSoBenhAn hoSo)
    {
        var (ok, _, _) = DanhGiaHoanTat(hoSo);
        if (ok) return TrangThaiHoSo.HoanTat;

        // chọn trạng thái cao nhất theo cấp ký đã có
        var allRoles = hoSo.TaiLieus.SelectMany(t => t.ChuKys)
            .Where(c => c.TrangThai == TrangThaiChuKy.DaKy)
            .Select(c => c.VaiTroKy).ToHashSet();

        if (allRoles.Contains(BieuMauCatalog.ROLE_LANHDAO_BV)) return TrangThaiHoSo.DaKyLanhDao;
        if (allRoles.Contains(BieuMauCatalog.ROLE_TRUONGKHOA)) return TrangThaiHoSo.DaKyTruongKhoa;
        if (allRoles.Contains(BieuMauCatalog.ROLE_BACSI) || allRoles.Contains(BieuMauCatalog.ROLE_DIEUDUONG))
            return TrangThaiHoSo.DaKyBacSi;
        return hoSo.TaiLieus.Any() ? TrangThaiHoSo.ChoKy : TrangThaiHoSo.Draft;
    }
}
