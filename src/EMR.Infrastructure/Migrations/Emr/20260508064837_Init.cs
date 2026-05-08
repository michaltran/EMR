using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EMR.Infrastructure.Migrations.Emr
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorTen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HanhDong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoaiDoiTuong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DoiTuongId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Chitiet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Khoa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nhom = table.Column<byte>(type: "tinyint", nullable: false),
                    KhoaChaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThuTu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Khoa_Khoa_KhoaChaId",
                        column: x => x.KhoaChaId,
                        principalTable: "Khoa",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CCCD = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    KhoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NguoiDung_Khoa_KhoaId",
                        column: x => x.KhoaId,
                        principalTable: "Khoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HoSoBenhAn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaHoSo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaBenhNhanHIS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaLanKhamHIS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HoTenBenhNhan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<byte>(type: "tinyint", nullable: true),
                    KhoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BacSiTaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    KhoLuuTru = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoBenhAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoSoBenhAn_Khoa_KhoaId",
                        column: x => x.KhoaId,
                        principalTable: "Khoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoSoBenhAn_NguoiDung_BacSiTaoId",
                        column: x => x.BacSiTaoId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung_VaiTro",
                columns: table => new
                {
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaiTroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung_VaiTro", x => new { x.NguoiDungId, x.VaiTroId });
                    table.ForeignKey(
                        name: "FK_NguoiDung_VaiTro_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NguoiDung_VaiTro_VaiTro_VaiTroId",
                        column: x => x.VaiTroId,
                        principalTable: "VaiTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoSoBenhAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaiTaiLieu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenFile = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DuongDanLuuTru = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    KichThuoc = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TrangThaiKy = table.Column<byte>(type: "tinyint", nullable: false),
                    NguoiUploadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayUpload = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiLieu_HoSoBenhAn_HoSoBenhAnId",
                        column: x => x.HoSoBenhAnId,
                        principalTable: "HoSoBenhAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaiLieu_NguoiDung_NguoiUploadId",
                        column: x => x.NguoiUploadId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChuKy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaiLieuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiKyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaiTroKy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoaiCa = table.Column<byte>(type: "tinyint", nullable: false),
                    SmartCa_TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SmartCa_TranCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SmartCa_CertId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SmartCa_SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CertSubject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CertNotBefore = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CertNotAfter = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sha256TruocKy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SignatureValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimestampSignature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuongDanFileSau = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayHoanTat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoLoi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WebhookPayloadRaw = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuKy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChuKy_NguoiDung_NguoiKyId",
                        column: x => x.NguoiKyId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChuKy_TaiLieu_TaiLieuId",
                        column: x => x.TaiLieuId,
                        principalTable: "TaiLieu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Khoa",
                columns: new[] { "Id", "KhoaChaId", "Ma", "Nhom", "Ten", "ThuTu" },
                values: new object[,]
                {
                    { new Guid("0ab26272-8a7f-280f-b020-a7b625f449fb"), null, "K_KSNK", (byte)2, "Khoa Kiểm soát nhiễm khuẩn", 33 },
                    { new Guid("12fc9f46-a4d8-4e05-fae7-8e326462bbd5"), null, "K_HSCC", (byte)2, "Khoa Hồi sức Cấp cứu", 27 },
                    { new Guid("1f759b61-e86c-aaa8-62d8-7aff4e85e726"), null, "K_COVID", (byte)2, "Khoa Điều trị COVID-19", 22 },
                    { new Guid("23e8b814-4ea7-95c1-ead4-5816cad0305f"), null, "K_NGOAI", (byte)2, "Khoa Ngoại", 20 },
                    { new Guid("24875405-ea4e-5840-dd0a-a0e7514ab909"), null, "TYT_HHB", (byte)3, "TYT Hòa Hiệp Bắc", 52 },
                    { new Guid("26b92fd3-497c-1ced-92cd-f7f6224c779f"), null, "P_KSBT", (byte)1, "Phòng Khám KSBT", 17 },
                    { new Guid("2f3a2d7e-64f5-99e3-0391-25ff75e33c15"), null, "TYT_HKB", (byte)3, "TYT Hòa Khánh Bắc", 50 },
                    { new Guid("3bc299d3-2aee-db12-1e36-49c93da36c2c"), null, "TYT_HM", (byte)3, "TYT Hòa Minh", 54 },
                    { new Guid("3e004de5-8106-c976-5116-1b483e1883aa"), null, "P_TCHC", (byte)1, "Phòng Tổ chức Hành chính", 10 },
                    { new Guid("70ae4769-9041-a4d8-f32a-6d7be3c79bf3"), null, "P_TCCB", (byte)1, "Phòng Tổ chức cán bộ", 14 },
                    { new Guid("7bbb8bff-8be8-3761-b7de-0f21ab3109f4"), null, "K_YTCC", (byte)2, "Khoa YTCC - DD & ATTP", 32 },
                    { new Guid("7e5c20b5-3662-5d7f-03ee-0a45982596f9"), null, "TYT_HKN", (byte)3, "TYT Hòa Khánh Nam", 51 },
                    { new Guid("83bcb35b-4eb0-3fc1-025e-72b0afec0224"), null, "K_DUOC", (byte)2, "Khoa Dược-TTB-VTYT", 30 },
                    { new Guid("896f4c73-8760-e97f-d3f4-8860274c530e"), null, "P_TC", (byte)1, "Phòng Tiêm chủng", 16 },
                    { new Guid("95bd14d9-3244-d67f-ccd5-07eabe1b4790"), null, "K_LCK", (byte)2, "Khoa Liên Chuyên Khoa", 25 },
                    { new Guid("98df8e63-89da-bcfb-6a45-8554f7693fa8"), null, "K_YHCT", (byte)2, "Khoa YHCT - VLTL&PHCN", 26 },
                    { new Guid("9b4cc343-8361-c219-e74c-b7d550c1e61c"), null, "BGD", (byte)0, "Ban Giám đốc", 1 },
                    { new Guid("9dcb52dd-4679-99e7-2e80-14ff1168df41"), null, "K_CDHA", (byte)2, "Khoa Chẩn đoán Hình ảnh", 28 },
                    { new Guid("a2aa8c07-8476-7977-f349-13d58988027b"), null, "TYT_HV", (byte)3, "Trạm y tế phường Hải Vân", 55 },
                    { new Guid("a4c8df16-b15f-6552-b491-ff256b2982ee"), null, "K_XN", (byte)2, "Khoa Xét nghiệm", 29 },
                    { new Guid("b44544bf-dc24-1efa-1c6b-3c5af9548611"), null, "K_SAN", (byte)2, "Khoa Sản", 23 },
                    { new Guid("c24ed824-3c6a-efbd-d2d2-73b6df7f6e29"), null, "P_TCKT", (byte)1, "Phòng Tài chính kế toán", 12 },
                    { new Guid("c75ef115-a297-a18e-feb3-e59c439d0865"), null, "K_NHI", (byte)2, "Khoa Nhi", 24 },
                    { new Guid("c8a5acb4-0376-87d9-17e1-55a7bb60497f"), null, "P_DD", (byte)1, "Phòng Điều dưỡng", 13 },
                    { new Guid("c8aac6ef-d8ad-1568-1145-1855e012efbe"), null, "P_KHNV", (byte)1, "Phòng Kế hoạch Nghiệp vụ", 11 },
                    { new Guid("cec3bea9-c3e5-6d3d-e1c1-ce8a73d1870c"), null, "K_KSBT", (byte)2, "Khoa Kiểm soát bệnh tật và HIV/AIDS", 31 },
                    { new Guid("d82cb7fd-7d2b-e2c3-2e47-a8fe69c6c761"), null, "K_PK", (byte)2, "Khoa Phòng khám", 34 },
                    { new Guid("daad9986-718b-b74e-e130-a4a7b47c603d"), null, "P_DS", (byte)1, "Phòng Dân số", 15 },
                    { new Guid("e6bed160-d5cb-d526-fa2a-d6be730d9183"), null, "K_NOI", (byte)2, "Khoa Nội", 21 },
                    { new Guid("f4a6f47f-8ca6-035a-842e-4f844d97363f"), null, "TYT_KHAC", (byte)3, "Trạm Y tế (chung)", 56 },
                    { new Guid("ff999ac1-4c27-bb9b-a17e-c08ddb74718c"), null, "TYT_HHN", (byte)3, "TYT Hòa Hiệp Nam", 53 }
                });

            migrationBuilder.InsertData(
                table: "VaiTro",
                columns: new[] { "Id", "Ma", "MoTa", "Ten" },
                values: new object[,]
                {
                    { new Guid("1f981080-1288-4c68-59fb-ed608719577f"), "DUOCSI", "Quản lý đơn thuốc, ký xác nhận cấp phát", "Dược sĩ" },
                    { new Guid("36da78af-74ea-8d49-9ff0-79ec250c85bb"), "DIEUDUONG", "Hỗ trợ tạo/cập nhật hồ sơ, ký phiếu chăm sóc", "Điều dưỡng" },
                    { new Guid("468e54da-69b6-9c75-7064-a25e947e8c0c"), "ADMIN", "Quản lý người dùng, vai trò, audit", "Quản trị hệ thống" },
                    { new Guid("7e9fc296-6444-b732-407d-7e9b586bafd5"), "LANHDAO_BV", "Ký duyệt cấp BV, xem toàn bộ", "Lãnh đạo Bệnh viện" },
                    { new Guid("825aa01d-a9a0-a07c-f1a8-dee376cd78d7"), "BACSI", "Tạo hồ sơ, ký bệnh án mình tạo", "Bác sĩ" },
                    { new Guid("ae81ac86-aac8-dd25-ff2f-9a1ed7cf6d86"), "BENHNHAN", "Đăng nhập app, xem hồ sơ của mình", "Bệnh nhân" },
                    { new Guid("c547e799-29b3-5c4b-174e-b4fd5eae7bef"), "KHTH", "Kiểm tra hồ sơ, chuyển kho BV, báo cáo", "Kế hoạch nghiệp vụ" },
                    { new Guid("e53c79aa-6512-780d-4cbc-8ea1fa150965"), "TRUONGKHOA", "Ký duyệt cấp khoa, xem hồ sơ trong khoa", "Trưởng khoa" }
                });

            migrationBuilder.InsertData(
                table: "Khoa",
                columns: new[] { "Id", "KhoaChaId", "Ma", "Nhom", "Ten", "ThuTu" },
                values: new object[,]
                {
                    { new Guid("06322610-66ac-d888-a31e-46ebf704710c"), new Guid("95bd14d9-3244-d67f-ccd5-07eabe1b4790"), "K_LCK_NTRU_RHM", (byte)2, "Liên Chuyên Khoa (Ngoại trú RHM)", 35 },
                    { new Guid("97cbbdd3-b484-0f24-b6d0-0af8bc6a53c6"), new Guid("98df8e63-89da-bcfb-6a45-8554f7693fa8"), "K_YHCT_NTRU", (byte)2, "Khoa YHCT - VLTL&PHCN (Ngoại trú)", 36 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_ActorId",
                table: "AuditLog",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_DoiTuongId_HanhDong",
                table: "AuditLog",
                columns: new[] { "DoiTuongId", "HanhDong" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_ThoiGian",
                table: "AuditLog",
                column: "ThoiGian",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ChuKy_NguoiKyId",
                table: "ChuKy",
                column: "NguoiKyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuKy_SmartCa_TransactionId",
                table: "ChuKy",
                column: "SmartCa_TransactionId",
                unique: true,
                filter: "[SmartCa_TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChuKy_TaiLieuId",
                table: "ChuKy",
                column: "TaiLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuKy_TrangThai",
                table: "ChuKy",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_BacSiTaoId",
                table: "HoSoBenhAn",
                column: "BacSiTaoId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_KhoaId_TrangThai",
                table: "HoSoBenhAn",
                columns: new[] { "KhoaId", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_MaBenhNhanHIS",
                table: "HoSoBenhAn",
                column: "MaBenhNhanHIS");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_MaHoSo",
                table: "HoSoBenhAn",
                column: "MaHoSo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoBenhAn_NgayTao",
                table: "HoSoBenhAn",
                column: "NgayTao");

            migrationBuilder.CreateIndex(
                name: "IX_Khoa_KhoaChaId",
                table: "Khoa",
                column: "KhoaChaId");

            migrationBuilder.CreateIndex(
                name: "IX_Khoa_Ma",
                table: "Khoa",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_CCCD",
                table: "NguoiDung",
                column: "CCCD",
                unique: true,
                filter: "[CCCD] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_KhoaId",
                table: "NguoiDung",
                column: "KhoaId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_TenDangNhap",
                table: "NguoiDung",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_VaiTro_VaiTroId",
                table: "NguoiDung_VaiTro",
                column: "VaiTroId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_HoSoBenhAnId",
                table: "TaiLieu",
                column: "HoSoBenhAnId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_NguoiUploadId",
                table: "TaiLieu",
                column: "NguoiUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieu_TrangThaiKy",
                table: "TaiLieu",
                column: "TrangThaiKy");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_Ma",
                table: "VaiTro",
                column: "Ma",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "ChuKy");

            migrationBuilder.DropTable(
                name: "NguoiDung_VaiTro");

            migrationBuilder.DropTable(
                name: "TaiLieu");

            migrationBuilder.DropTable(
                name: "VaiTro");

            migrationBuilder.DropTable(
                name: "HoSoBenhAn");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "Khoa");
        }
    }
}
