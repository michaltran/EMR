namespace EMR.Signing;

public class VnptSmartCaSettings
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = "https://rmgateway.vnptit.vn/sca/sp769"; // UAT
    public string SpId { get; set; } = "";
    public string SpPassword { get; set; } = "";
    public string SignType { get; set; } = "hash"; // theo TT 13/2025: API SmartCA chỉ ký hash
    public string FileType { get; set; } = "pdf";
    public int PollIntervalSeconds { get; set; } = 3;
    public int PollTimeoutSeconds { get; set; } = 180;
}
