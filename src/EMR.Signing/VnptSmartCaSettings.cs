namespace EMR.Signing;

public class VnptSmartCaSettings
{
    public bool Enabled { get; set; } = false;

    /// <summary>"V1" = SmartCA thường (user confirm trên app), "TH" = SmartCA Tích hợp (TOTP + password)</summary>
    public string Mode { get; set; } = "V1";

    public string BaseUrl { get; set; } = "https://rmgateway.vnptit.vn/sca/sp769"; // UAT

    /// <summary>3rd Party app credentials do VNPT cấp khi BV ký hợp đồng</summary>
    public string SpId { get; set; } = "";
    public string SpPassword { get; set; } = "";

    public string SignType { get; set; } = "hash"; // SmartCA chỉ ký hash
    public string FileType { get; set; } = "pdf";
    public int PollIntervalSeconds { get; set; } = 3;
    public int PollTimeoutSeconds { get; set; } = 180;

    /// <summary>
    /// Map user của EMR (theo CCCD trong NguoiDung.CCCD) sang config SmartCA.
    /// Lookup: tìm user có Cccd khớp với CCCD của BS đang ký.
    /// </summary>
    public List<VnptSmartCaUserConfig> Users { get; set; } = new();

    public VnptSmartCaUserConfig? FindUser(string cccd) =>
        Users.FirstOrDefault(u => u.Cccd == cccd);
}
