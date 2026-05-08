namespace EMR.Domain.Enums;

public enum TrangThaiHoSo : byte
{
    Draft = 0,
    ChoKy = 1,
    DaKyBacSi = 2,
    DaKyTruongKhoa = 3,
    DaKyLanhDao = 4,
    HoanTat = 9,
    Huy = 99
}

public enum KhoLuuTru : byte
{
    Khoa = 0,
    BenhVien = 1
}

public enum TrangThaiKyTaiLieu : byte
{
    ChuaKy = 0,
    DangKy = 1,
    DaKy = 2,
    Loi = 9
}

public enum TrangThaiChuKy : byte
{
    ChoXacNhan = 0,
    DaKy = 1,
    ThatBai = 2,
    Huy = 3,
    QuaHan = 4
}

public enum LoaiCa : byte
{
    SelfSigned = 0,
    VnptSmartCa = 1
}

public enum NhomKhoa : byte
{
    BGD = 0,
    Phong = 1,
    Khoa = 2,
    TYT = 3
}
