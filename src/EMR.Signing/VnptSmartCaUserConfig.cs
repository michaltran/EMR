namespace EMR.Signing;

/// <summary>
/// Thông tin xác thực 1 user trong gói SmartCA TH (Tích hợp).
/// Lưu ý: KHÔNG commit vào git — đặt trong appsettings.local.json (đã có .gitignore).
/// </summary>
public class VnptSmartCaUserConfig
{
    /// <summary>CCCD/CMND của BS (= user_id trong API VNPT)</summary>
    public string Cccd { get; set; } = "";
    /// <summary>Mật khẩu đăng nhập SmartCA của BS</summary>
    public string Password { get; set; } = "";
    /// <summary>Serial số chứng thư số (lấy từ /v1/credentials/get_certificate)</summary>
    public string SerialNumber { get; set; } = "";
    /// <summary>TOTP key VNPT cấp (Base64-encoded hex). Để trống nếu user dùng v1 (confirm trên app)</summary>
    public string? TotpKey { get; set; }
}
