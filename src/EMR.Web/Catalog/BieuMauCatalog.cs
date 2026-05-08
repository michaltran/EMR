namespace EMR.Web.Catalog;

/// <summary>
/// Danh mục biểu mẫu chuẩn theo TT 32/2023/TT-BYT (Phụ lục XXVIII + XXIX) — 77+ mẫu.
/// Mã form theo BYT (vd "01/BV2") được giữ nguyên trong cột MaBYT để đối chiếu.
/// </summary>
/// <param name="Ma">Mã nội bộ (lưu vào TaiLieu.LoaiTaiLieu) — chỉ chữ thường + dấu gạch dưới</param>
/// <param name="MaBYT">Mã BYT chính thức TT 32/2023 (vd "01/BV2", "PL28-NK") — null nếu không có</param>
/// <param name="MaCu">Mã BYT cũ TT 56/2017 (vd "01/BV-01", "29/BV-02") — BV TTYT Liên Chiểu hiện đang dùng mã này</param>
/// <param name="Ten">Tên hiển thị</param>
/// <param name="MoTa">Mô tả ngắn</param>
/// <param name="Nhom">Nhóm phân loại UI</param>
/// <param name="VaiTroKyTuanTu">Thứ tự vai trò ký bắt buộc (rỗng = không bắt buộc ký)</param>
/// <param name="BatBuoc">Có bắt buộc trong HSBA nội trú điển hình không</param>
public record BieuMauTemplate(
    string Ma,
    string? MaBYT,
    string Ten,
    string MoTa,
    string Nhom,
    string[] VaiTroKyTuanTu,
    bool BatBuoc,
    string? MaCu = null);

public static class BieuMauCatalog
{
    public const string ROLE_BACSI = "BACSI";
    public const string ROLE_DIEUDUONG = "DIEUDUONG";
    public const string ROLE_DUOCSI = "DUOCSI";
    public const string ROLE_TRUONGKHOA = "TRUONGKHOA";
    public const string ROLE_KHTH = "KHTH";
    public const string ROLE_LANHDAO_BV = "LANHDAO_BV";

    // Nhóm UI
    public const string GROUP_BENH_AN = "BENH_AN";
    public const string GROUP_KHAM_VAO_VIEN = "KHAM_VAO_VIEN";
    public const string GROUP_XET_NGHIEM = "XET_NGHIEM";
    public const string GROUP_CDHA = "CDHA";
    public const string GROUP_PT_TT = "PHAU_THUAT_THU_THUAT";
    public const string GROUP_THEO_DOI = "THEO_DOI_CHAM_SOC";
    public const string GROUP_CAM_KET = "CAM_KET";
    public const string GROUP_HOI_CHAN_TV = "HOI_CHAN_TU_VONG";
    public const string GROUP_RA_VIEN = "RA_VIEN";
    public const string GROUP_KHAC = "KHAC";

    private static readonly string[] WF_BS = [ROLE_BACSI];
    private static readonly string[] WF_BS_TK = [ROLE_BACSI, ROLE_TRUONGKHOA];
    private static readonly string[] WF_BS_TK_LD = [ROLE_BACSI, ROLE_TRUONGKHOA, ROLE_LANHDAO_BV];
    private static readonly string[] WF_DD = [ROLE_DIEUDUONG];
    private static readonly string[] WF_DD_TK = [ROLE_DIEUDUONG, ROLE_TRUONGKHOA];
    private static readonly string[] WF_NONE = [];

    public static readonly IReadOnlyList<BieuMauTemplate> Items =
    [
        // ============================================================
        // PHỤ LỤC XXVIII — MẪU BỆNH ÁN (24 mẫu)
        // ============================================================
        new("BA_NOIKHOA",        "PL28-NK",  "Bệnh án Nội khoa",                  "Mẫu bệnh án nội trú khoa Nội",                                GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: true, MaCu: "01/BV-01"),
        new("BA_NHIKHOA",        "PL28-NhK", "Bệnh án Nhi khoa",                  "Mẫu bệnh án nội trú khoa Nhi",                                GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_TRUYEN_NHIEM",   "PL28-TN",  "Bệnh án Truyền nhiễm",              "Mẫu bệnh án truyền nhiễm",                                    GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_PHU_KHOA",       "PL28-PhK", "Bệnh án Phụ khoa",                  "Mẫu bệnh án phụ khoa",                                        GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_SAN_KHOA",       "PL28-SK",  "Bệnh án Sản khoa",                  "Mẫu bệnh án sản khoa",                                        GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false, MaCu: "04/BV-01"),
        new("BA_SO_SINH",        "PL28-SS",  "Bệnh án Sơ sinh",                   "Mẫu bệnh án sơ sinh",                                         GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_TAM_THAN",       "PL28-TT",  "Bệnh án Tâm thần",                  "Mẫu bệnh án tâm thần",                                        GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_DA_LIEU",        "PL28-DL",  "Bệnh án Da liễu",                   "Mẫu bệnh án da liễu",                                         GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_NGOAI_KHOA",     "PL28-NgK", "Bệnh án Ngoại khoa",                "Mẫu bệnh án nội trú khoa Ngoại",                              GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_BONG",           "PL28-Bg",  "Bệnh án Bỏng",                      "Mẫu bệnh án bỏng",                                            GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_UNG_BUOU",       "PL28-UB",  "Bệnh án Ung bướu",                  "Mẫu bệnh án ung bướu",                                        GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_RHM",            "PL28-RHM", "Bệnh án Răng - Hàm - Mặt",          "Mẫu bệnh án răng hàm mặt",                                    GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_TMH",            "PL28-TMH", "Bệnh án Tai - Mũi - Họng",          "Mẫu bệnh án tai mũi họng",                                    GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_NGOAI_TRU",      "PL28-NTr", "Bệnh án Ngoại trú",                 "Mẫu bệnh án ngoại trú (chung)",                               GROUP_BENH_AN, WF_BS,        BatBuoc: false),
        new("BA_YHCT_NTRU",      "PL28-YHCT-NT", "Bệnh án Nội trú Y học cổ truyền", "Mẫu bệnh án nội trú YHCT",                                  GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_YHCT_NGOAI_TRU", "PL28-YHCT-NgT", "Bệnh án Ngoại trú Y học cổ truyền", "Mẫu bệnh án ngoại trú YHCT",                              GROUP_BENH_AN, WF_BS,        BatBuoc: false),
        new("BA_YHCT_NHI_NTRU",  "PL28-YHCT-Nhi", "Bệnh án Nội trú Nhi YHCT",     "Mẫu bệnh án nội trú nhi YHCT",                                GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_MAT_1",          "PL28-M1",  "Bệnh án Mắt — Glocom",              "Mẫu bệnh án Mắt biến thể 1",                                  GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_MAT_2",          "PL28-M2",  "Bệnh án Mắt — Đáy mắt/Bồ-nhãn cầu", "Mẫu bệnh án Mắt biến thể 2",                                  GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_MAT_3",          "PL28-M3",  "Bệnh án Mắt — Lác",                 "Mẫu bệnh án Mắt biến thể 3",                                  GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_MAT_4",          "PL28-M4",  "Bệnh án Mắt — Tái lệ",              "Mẫu bệnh án Mắt biến thể 4",                                  GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_MAT_5",          "PL28-M5",  "Bệnh án Mắt — Sụp mi/mộng/TTT",     "Mẫu bệnh án Mắt biến thể 5",                                  GROUP_BENH_AN, WF_BS_TK_LD, BatBuoc: false),
        new("BA_PHCN_NGOAI_TRU", "PL28-PHCN", "Bệnh án Ngoại trú Phục hồi chức năng", "Mẫu bệnh án ngoại trú PHCN",                                GROUP_BENH_AN, WF_BS,        BatBuoc: false),

        // ============================================================
        // PHỤ LỤC XXIX — MẪU GIẤY, PHIẾU Y (53 mẫu)
        // ============================================================
        // -- Cam kết & giấy chứng nhận --
        new("PYL_01",  "01/BV2", "Giấy cam kết chấp thuận PT-TT-GMHS",          "Cam kết phẫu thuật/thủ thuật/gây mê hồi sức",            GROUP_CAM_KET, WF_BS_TK,   BatBuoc: false),
        new("PYL_02",  "02/BV2", "Giấy chứng nhận phẫu thuật",                  "Chứng nhận đã thực hiện phẫu thuật",                     GROUP_PT_TT,   WF_BS_TK,   BatBuoc: false),
        new("PYL_03",  "03/BV2", "Giấy khám/chữa bệnh theo yêu cầu",            "KCB theo yêu cầu",                                       GROUP_KHAM_VAO_VIEN, WF_BS, BatBuoc: false),
        new("PYL_04",  "04/BV2", "Phiếu khám chuyên khoa",                      "Khám hội chẩn chuyên khoa",                              GROUP_KHAM_VAO_VIEN, WF_BS, BatBuoc: false),

        // -- Phẫu thuật, thủ thuật, gây mê --
        new("PYL_05",  "05/BV2", "Phiếu gây mê hồi sức",                        "Phiếu gây mê hồi sức",                                   GROUP_PT_TT, WF_BS,        BatBuoc: false),
        new("PYL_06",  "06/BV2", "Phiếu phẫu thuật / thủ thuật",                "Biên bản PT/TT",                                         GROUP_PT_TT, WF_BS_TK,     BatBuoc: false),
        new("PYL_30",  "30/BV2", "Phiếu phẫu thuật ghép giác mạc",              "PT ghép giác mạc",                                       GROUP_PT_TT, WF_BS_TK,     BatBuoc: false),
        new("PYL_31",  "31/BV2", "Phiếu phẫu thuật bộ mặt nhãn cầu",            "PT bộ mặt nhãn cầu",                                     GROUP_PT_TT, WF_BS_TK,     BatBuoc: false),
        new("PYL_32",  "32/BV2", "Phiếu phẫu thuật Glocom",                     "PT Glocom",                                              GROUP_PT_TT, WF_BS_TK,     BatBuoc: false),
        new("PYL_33",  "33/BV2", "Phiếu phẫu thuật lác",                        "PT lác",                                                 GROUP_PT_TT, WF_BS_TK,     BatBuoc: false),
        new("PYL_34",  "34/BV2", "Phiếu phẫu thuật tái lệ",                     "PT tái lệ",                                              GROUP_PT_TT, WF_BS_TK,     BatBuoc: false),
        new("PYL_35",  "35/BV2", "Phiếu phẫu thuật sụp mi, mộng, TTT, Sapejko", "PT sụp mi/mộng/thể thuỷ tinh/Sapejko",                   GROUP_PT_TT, WF_BS_TK,     BatBuoc: false),

        // -- Theo dõi điều trị --
        new("PYL_07",  "07/BV2", "Phiếu theo dõi truyền dịch",                  "Phiếu theo dõi truyền dịch",                             GROUP_THEO_DOI, WF_DD,    BatBuoc: false),
        new("PYL_36",  "36/BV2", "Phiếu theo dõi điều trị",                     "Phiếu theo dõi điều trị nội trú",                        GROUP_THEO_DOI, WF_BS,    BatBuoc: false),
        new("PYL_37",  "37/BV2", "Phiếu chăm sóc cấp 1",                        "Phiếu chăm sóc cấp 1 (điều dưỡng)",                      GROUP_THEO_DOI, WF_DD_TK, BatBuoc: false),
        new("PYL_38",  "38/BV2", "Phiếu chăm sóc cấp 2",                        "Phiếu chăm sóc cấp 2 (điều dưỡng)",                      GROUP_THEO_DOI, WF_DD_TK, BatBuoc: false),
        new("PYL_39",  "39/BV2", "Phiếu phân loại NB tại khoa Cấp cứu",         "Nhận định phân loại tại cấp cứu",                        GROUP_THEO_DOI, WF_DD,    BatBuoc: false),
        new("PYL_42",  "42/BV2", "Phiếu thông tin NB tại HSTC",                 "Cung cấp thông tin tại Hồi sức tích cực",                GROUP_THEO_DOI, WF_BS,    BatBuoc: false),
        new("PYL_43",  "43/BV2", "Phiếu bàn giao NB chuyển khoa (BS)",          "BS bàn giao chuyển khoa",                                GROUP_THEO_DOI, WF_BS,    BatBuoc: false),
        new("PYL_44",  "44/BV2", "Phiếu bàn giao NB chuyển khoa (ĐD)",          "ĐD bàn giao chuyển khoa",                                GROUP_THEO_DOI, WF_DD,    BatBuoc: false),

        // -- Chẩn đoán hình ảnh & thăm dò chức năng --
        new("PYL_08",  "08/BV2", "Phiếu chiếu/chụp X-quang",                    "X-quang",                                                GROUP_CDHA, WF_BS, BatBuoc: false),
        new("PYL_09",  "09/BV2", "Phiếu chụp cắt lớp vi tính (CT)",             "Chụp CT",                                                GROUP_CDHA, WF_BS, BatBuoc: false),
        new("PYL_10",  "10/BV2", "Phiếu chụp cộng hưởng từ (MRI)",              "Chụp MRI",                                               GROUP_CDHA, WF_BS, BatBuoc: false),
        new("PYL_11",  "11/BV2", "Phiếu siêu âm",                                "Siêu âm",                                                GROUP_CDHA, WF_BS, BatBuoc: false),
        new("PYL_12",  "12/BV2", "Phiếu điện tim (ECG)",                        "Điện tim đồ",                                            GROUP_CDHA, WF_BS, BatBuoc: false),
        new("PYL_13",  "13/BV2", "Phiếu điện não (EEG)",                        "Điện não đồ",                                            GROUP_CDHA, WF_BS, BatBuoc: false),
        new("PYL_14",  "14/BV2", "Phiếu nội soi",                                "Nội soi (tiêu hoá, hô hấp...)",                          GROUP_CDHA, WF_BS, BatBuoc: false),
        new("PYL_15",  "15/BV2", "Phiếu đo chức năng hô hấp",                   "Đo chức năng hô hấp",                                    GROUP_CDHA, WF_BS, BatBuoc: false),

        // -- Xét nghiệm --
        new("PYL_16",  "16/BV2", "Phiếu xét nghiệm (chung)",                    "XN tổng quát",                                           GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_17",  "17/BV2", "Phiếu xét nghiệm Huyết học",                  "XN huyết học",                                           GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_18",  "18/BV2", "Phiếu xét nghiệm huyết-tủy đồ",               "XN huyết-tủy đồ",                                        GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_19",  "19/BV2", "Phiếu XN chẩn đoán RL đông cầm máu",          "XN đông cầm máu",                                        GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_20",  "20/BV2", "Phiếu XN sinh thiết tủy xương",               "Sinh thiết tủy xương",                                   GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_21",  "21/BV2", "Phiếu xét nghiệm nước dịch",                  "XN nước dịch (não tủy, dịch chọc)",                      GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_22",  "22/BV2", "Phiếu xét nghiệm hóa sinh máu",               "Hóa sinh máu",                                           GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_23",  "23/BV2", "Phiếu XN hóa sinh nước tiểu/phân/dịch",       "Hóa sinh nước tiểu, phân, dịch chọc dò",                 GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_24",  "24/BV2", "Phiếu xét nghiệm vi sinh",                    "XN vi sinh",                                             GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_25",  "25/BV2", "Phiếu XN giải phẫu bệnh sinh thiết",          "GPB sinh thiết",                                         GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),
        new("PYL_26",  "26/BV2", "Phiếu XN giải phẫu bệnh tử thi",              "GPB khám nghiệm tử thi",                                 GROUP_XET_NGHIEM, WF_BS, BatBuoc: false),

        // -- Hội chẩn / Tử vong --
        new("PYL_27",  "27/BV2", "Trích biên bản hội chẩn",                     "Trích biên bản hội chẩn liên khoa",                      GROUP_HOI_CHAN_TV, WF_BS_TK, BatBuoc: false),
        new("PYL_28",  "28/BV2", "Trích biên bản kiểm thảo tử vong",            "Trích biên bản kiểm thảo TV",                            GROUP_HOI_CHAN_TV, WF_BS_TK, BatBuoc: false),
        new("PYL_47",  "47/BV2", "Biên bản kiểm thảo tử vong",                  "Biên bản kiểm thảo tử vong (đầy đủ)",                    GROUP_HOI_CHAN_TV, WF_BS_TK_LD, BatBuoc: false),

        // -- Khám vào viện / Cam kết / Sơ sinh / Khám thai --
        new("PYL_29",  "29/BV2", "Phiếu khám bệnh vào viện (chung)",            "Phiếu khám vào viện",                                    GROUP_KHAM_VAO_VIEN, WF_BS, BatBuoc: true),
        new("PYL_40",  "40/BV2", "Cam kết chung nhập viện nội trú",             "Cung cấp thông tin và cam kết nhập viện",                GROUP_CAM_KET, WF_BS,        BatBuoc: false),
        new("PYL_41",  "41/BV2", "Cam kết từ chối sử dụng dịch vụ KCB",         "Cam kết từ chối KCB",                                    GROUP_CAM_KET, WF_BS,        BatBuoc: false),
        new("PYL_45",  "45/BV2", "Cam kết chuyển cơ sở KCB",                    "Cam kết chuyển cơ sở",                                   GROUP_CAM_KET, WF_BS_TK,     BatBuoc: false),
        new("PYL_46",  "46/BV2", "Cam kết ra viện không theo chỉ định BS",      "Cam kết ra viện không theo CĐ",                          GROUP_CAM_KET, WF_BS,        BatBuoc: false),
        new("PYL_48",  "48/BV2", "Cam kết chấp thuận điều trị Hóa - Xạ trị",    "Cam kết hóa-xạ trị",                                     GROUP_CAM_KET, WF_BS_TK,     BatBuoc: false),
        new("PYL_49",  "49/BV2", "Cam kết chấp thuận điều trị bằng Xạ trị",     "Cam kết xạ trị",                                         GROUP_CAM_KET, WF_BS_TK,     BatBuoc: false),
        new("PYL_50",  "50/BV2", "Phiếu điều trị trẻ sơ sinh sau sinh",         "ĐT trẻ sơ sinh",                                         GROUP_THEO_DOI, WF_BS,       BatBuoc: false),
        new("PYL_51",  "51/BV2", "Phiếu khám thai",                              "Khám thai",                                              GROUP_KHAM_VAO_VIEN, WF_BS,  BatBuoc: false),

        // -- Ra viện / Tóm tắt HSBA --
        new("PYL_52",  "52/BV2", "Bản tóm tắt hồ sơ bệnh án (CV-01)",           "Tóm tắt HSBA — bắt buộc khi ra viện/chuyển tuyến",       GROUP_RA_VIEN, WF_BS_TK_LD, BatBuoc: true),
        new("PYL_53",  "53/BV2", "Đề nghị cung cấp tóm tắt HSBA / tài liệu",    "Đề nghị cung cấp tóm tắt HSBA",                          GROUP_RA_VIEN, WF_BS,       BatBuoc: false),

        // ============================================================
        // BV TTYT Liên Chiểu — biểu mẫu nội bộ ngoài TT 32/2023
        // ============================================================
        new("PYL_BV_10",   null,  "Phiếu công khai thuốc nội trú",     "Phiếu công khai thuốc/VTYT nội trú (mẫu BV)",     GROUP_THEO_DOI, WF_BS,    BatBuoc: false, MaCu: "10/BV-01"),
        new("PYL_BV_17",   null,  "Phiếu công khai chi phí KCB",       "Công khai chi phí KCB cho BN/BHYT (mẫu BV)",      GROUP_KHAC,     WF_NONE,  BatBuoc: false, MaCu: "17/BV-01"),

        // ============================================================
        // HSBA TỔNG (mode upload nguyên file PDF từ HIS FPT)
        // ============================================================
        new("HSBA_TONG",   null,  "HSBA tổng (1 file PDF từ HIS)",     "Bộ HSBA in ra từ HIS FPT, ký 1 chữ ký cho cả file (workflow hiện tại của BV trước khi tách per-form)",
            GROUP_KHAC, WF_BS_TK_LD, BatBuoc: false),

        // ============================================================
        // KHÁC (không thuộc 82 mẫu chuẩn)
        // ============================================================
        new("DON_THUOC",  null,  "Đơn thuốc",                  "Đơn thuốc nội/ngoại trú (theo TT 52/2017 và sửa đổi)",          GROUP_KHAC, WF_BS, BatBuoc: false),
        new("KHAC",       null,  "Tài liệu khác",              "Tài liệu không thuộc danh mục chuẩn",                            GROUP_KHAC, WF_NONE, BatBuoc: false),
    ];

    public static BieuMauTemplate? Find(string ma) => Items.FirstOrDefault(x => x.Ma == ma);

    public static IEnumerable<IGrouping<string, BieuMauTemplate>> GroupedForUI() =>
        Items.GroupBy(x => x.Nhom).OrderBy(g => GroupOrder(g.Key));

    public static string GroupLabel(string group) => group switch
    {
        GROUP_BENH_AN          => "Bệnh án (theo chuyên khoa)",
        GROUP_KHAM_VAO_VIEN    => "Khám / Vào viện",
        GROUP_XET_NGHIEM       => "Xét nghiệm",
        GROUP_CDHA             => "Chẩn đoán hình ảnh & Thăm dò chức năng",
        GROUP_PT_TT            => "Phẫu thuật / Thủ thuật / Gây mê",
        GROUP_THEO_DOI         => "Theo dõi điều trị / Chăm sóc",
        GROUP_CAM_KET          => "Cam kết / Đồng thuận",
        GROUP_HOI_CHAN_TV      => "Hội chẩn / Tử vong",
        GROUP_RA_VIEN          => "Ra viện / Tóm tắt HSBA",
        GROUP_KHAC             => "Khác",
        _ => group
    };

    private static int GroupOrder(string g) => g switch
    {
        GROUP_KHAM_VAO_VIEN => 1,
        GROUP_BENH_AN => 2,
        GROUP_XET_NGHIEM => 3,
        GROUP_CDHA => 4,
        GROUP_PT_TT => 5,
        GROUP_THEO_DOI => 6,
        GROUP_HOI_CHAN_TV => 7,
        GROUP_CAM_KET => 8,
        GROUP_RA_VIEN => 9,
        GROUP_KHAC => 10,
        _ => 99
    };
}
