using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMR.Infrastructure.Migrations.HisDemo
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BenhNhan",
                columns: table => new
                {
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<byte>(type: "tinyint", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenhNhan", x => x.Ma);
                });

            migrationBuilder.CreateTable(
                name: "LanKham",
                columns: table => new
                {
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaBenhNhan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayVaoVien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayRaVien = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KhoaDieuTri = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChanDoanRaVien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanKham", x => x.Ma);
                    table.ForeignKey(
                        name: "FK_LanKham_BenhNhan_MaBenhNhan",
                        column: x => x.MaBenhNhan,
                        principalTable: "BenhNhan",
                        principalColumn: "Ma",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LanKham_MaBenhNhan",
                table: "LanKham",
                column: "MaBenhNhan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LanKham");

            migrationBuilder.DropTable(
                name: "BenhNhan");
        }
    }
}
