namespace EMR.Web.Catalog;

public record BieuMauTemplate(string Ma, string Ten, string MoTa, string VaiTroKyChinh);

public static class BieuMauCatalog
{
    public static readonly IReadOnlyList<BieuMauTemplate> Items =
    [
        new("TO_BIA",            "Tờ bìa hồ sơ",                  "Trang bìa hồ sơ bệnh án (mẫu 01/BV-BYT)",         "BACSI"),
        new("BENH_AN_TONG_HOP",  "Bệnh án tổng hợp",              "Bệnh án nội trú tổng hợp (mẫu 02/BV-BYT)",        "BACSI"),
        new("TONG_KET_BENH_AN",  "Tổng kết bệnh án",              "Tổng kết bệnh án ra viện",                        "BACSI"),
        new("DON_THUOC",         "Đơn thuốc",                      "Đơn thuốc nội/ngoại trú",                         "BACSI"),
        new("KQ_XET_NGHIEM",     "Kết quả xét nghiệm",             "Kết quả XN huyết học, sinh hóa, vi sinh...",      "BACSI"),
        new("KQ_CDHA",           "Kết quả CĐHA",                   "Kết quả Chẩn đoán hình ảnh (X-quang, CT, MRI)",   "BACSI"),
        new("PHIEU_PHAU_THUAT",  "Phiếu phẫu thuật",               "Biên bản phẫu thuật",                             "BACSI"),
        new("BIEN_BAN_HOI_CHAN", "Biên bản hội chẩn",              "Biên bản hội chẩn liên khoa",                     "TRUONGKHOA"),
        new("PHIEU_CHAM_SOC",    "Phiếu chăm sóc",                 "Phiếu chăm sóc của điều dưỡng",                   "DIEUDUONG"),
        new("GIAY_RA_VIEN",      "Giấy ra viện",                   "Giấy ra viện (mẫu 03/BV-BYT)",                    "LANHDAO_BV"),
        new("KHAC",              "Tài liệu khác",                  "Các tài liệu không thuộc danh mục chuẩn",         "BACSI"),
    ];

    public static BieuMauTemplate? Find(string ma) => Items.FirstOrDefault(x => x.Ma == ma);
}
