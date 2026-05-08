using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMR.Signing;

/// <summary>
/// Client gọi VNPT SmartCA Tích hợp (TH) — v2 endpoints (mục 4 tài liệu v4.0).
/// Flow:
///   1. POST /v1/credentials/get_certificate (giống v1, chỉ để lấy cert info nếu chưa có)
///   2. POST /v2/signatures/sign — gửi hash + password + OTP → nhận `sad`
///   3. POST /v2/signatures/confirm — gửi `sad` → nhận `signature_value` ngay (không cần user confirm trên app)
/// </summary>
public class VnptSmartCaThClient(HttpClient http, IOptions<VnptSmartCaSettings> opt, ILogger<VnptSmartCaThClient> logger)
{
    private readonly VnptSmartCaSettings _s = opt.Value;
    private static readonly JsonSerializerOptions Json = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<VnptSmartCaClient.GetCertResponse?> GetCertificateAsync(string userCccd, string transactionId, string serialNumber, CancellationToken ct = default)
    {
        var req = new
        {
            sp_id = _s.SpId,
            sp_password = _s.SpPassword,
            user_id = userCccd,
            serial_number = serialNumber ?? "",
            transaction_id = transactionId
        };
        var url = $"{_s.BaseUrl}/v1/credentials/get_certificate";
        var resp = await http.PostAsJsonAsync(url, req, Json, ct);
        return await resp.Content.ReadFromJsonAsync<VnptSmartCaClient.GetCertResponse>(Json, ct);
    }

    public async Task<ThSignResponse?> SignAsync(string userCccd, string password, string otp, string transactionId, string transactionDesc, string serialNumber, string docId, byte[] hash, CancellationToken ct = default)
    {
        var req = new
        {
            sp_id = _s.SpId,
            sp_password = _s.SpPassword,
            user_id = userCccd,
            password,
            otp,
            transaction_id = transactionId,
            transaction_desc = transactionDesc,
            serial_number = serialNumber,
            time_stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "Z",
            sign_files = new[]
            {
                new
                {
                    doc_id = docId,
                    file_type = _s.FileType,
                    sign_type = _s.SignType,
                    data_to_be_signed = Convert.ToHexString(hash).ToLowerInvariant()
                }
            }
        };
        var url = $"{_s.BaseUrl}/v2/signatures/sign";
        logger.LogInformation("SmartCA TH sign: cccd={Cccd} tran={Tran}", userCccd, transactionId);
        var resp = await http.PostAsJsonAsync(url, req, Json, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        logger.LogDebug("SmartCA TH sign response: {Body}", raw);
        return JsonSerializer.Deserialize<ThSignResponse>(raw, Json);
    }

    public async Task<ThConfirmResponse?> ConfirmAsync(string userCccd, string password, string sad, string transactionId, CancellationToken ct = default)
    {
        var req = new
        {
            sp_id = _s.SpId,
            sp_password = _s.SpPassword,
            user_id = userCccd,
            password,
            sad,
            transaction_id = transactionId
        };
        var url = $"{_s.BaseUrl}/v2/signatures/confirm";
        var resp = await http.PostAsJsonAsync(url, req, Json, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        logger.LogDebug("SmartCA TH confirm response: {Body}", raw);
        return JsonSerializer.Deserialize<ThConfirmResponse>(raw, Json);
    }

    // ===== DTOs =====
    public class ThSignResponse
    {
        public int Status_Code { get; set; }
        public string? Message { get; set; }
        public ThSignData? Data { get; set; }
    }
    public class ThSignData
    {
        public string? Transaction_Id { get; set; }
        public string? Tran_Code { get; set; }
        public string? Sad { get; set; }
        public int? Expired_In { get; set; }
    }

    public class ThConfirmResponse
    {
        public int Status_Code { get; set; }
        public string? Message { get; set; }
        public ThConfirmData? Data { get; set; }
    }
    public class ThConfirmData
    {
        public string? Transaction_Id { get; set; }
        public int? Expired_In { get; set; }
        public List<VnptSmartCaClient.SignedFile>? Signatures { get; set; }
    }
}
