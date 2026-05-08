using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMR.Signing;

/// <summary>
/// Client gọi VNPT SmartCA API theo tài liệu v4.0 (xem docs/smartca-api.md).
/// Endpoints:
///   POST /v1/credentials/get_certificate
///   POST /v1/signatures/sign
///   POST /v1/signatures/sign/{tranCode}/status
/// Webhook (nhận từ CA): POST {our endpoint} chứa signed_files[]
/// </summary>
public class VnptSmartCaClient(HttpClient http, IOptions<VnptSmartCaSettings> opt, ILogger<VnptSmartCaClient> logger)
{
    private readonly VnptSmartCaSettings _s = opt.Value;
    private static readonly JsonSerializerOptions Json = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<GetCertResponse?> GetCertificateAsync(string userCccd, string transactionId, string? serial = null, CancellationToken ct = default)
    {
        var req = new
        {
            sp_id = _s.SpId,
            sp_password = _s.SpPassword,
            user_id = userCccd,
            serial_number = serial ?? "",
            transaction_id = transactionId
        };
        var url = $"{_s.BaseUrl}/v1/credentials/get_certificate";
        logger.LogInformation("SmartCA GetCert: user={Cccd} tran={Tran}", userCccd, transactionId);
        var resp = await http.PostAsJsonAsync(url, req, Json, ct);
        var body = await resp.Content.ReadFromJsonAsync<GetCertResponse>(Json, ct);
        return body;
    }

    public async Task<SignResponse?> RequestSignAsync(string userCccd, string transactionId, string transactionDesc, string serialNumber, string docId, byte[] hash, CancellationToken ct = default)
    {
        var req = new
        {
            sp_id = _s.SpId,
            sp_password = _s.SpPassword,
            user_id = userCccd,
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
        var url = $"{_s.BaseUrl}/v1/signatures/sign";
        var resp = await http.PostAsJsonAsync(url, req, Json, ct);
        var body = await resp.Content.ReadFromJsonAsync<SignResponse>(Json, ct);
        return body;
    }

    public async Task<SignStatusResponse?> GetSignStatusAsync(string tranCode, CancellationToken ct = default)
    {
        var url = $"{_s.BaseUrl}/v1/signatures/sign/{tranCode}/status";
        var resp = await http.PostAsync(url, null, ct);
        var body = await resp.Content.ReadFromJsonAsync<SignStatusResponse>(Json, ct);
        return body;
    }

    // ============= DTOs =============

    public class GetCertResponse
    {
        public int Status_Code { get; set; }
        public string? Message { get; set; }
        public CertData? Data { get; set; }
    }
    public class CertData { public List<UserCert>? User_Certificates { get; set; } }
    public class UserCert
    {
        public string? Service_Type { get; set; }
        public string? Service_Name { get; set; }
        public string? Cert_Id { get; set; }
        public string? Cert_Status { get; set; }
        public string? Serial_Number { get; set; }
        public string? Cert_Subject { get; set; }
        public DateTime? Cert_Valid_From { get; set; }
        public DateTime? Cert_Valid_To { get; set; }
        public string? Cert_Data { get; set; }
        public string? Transaction_Id { get; set; }
    }

    public class SignResponse
    {
        public int Status_Code { get; set; }
        public string? Message { get; set; }
        public SignData? Data { get; set; }
    }
    public class SignData { public string? Transaction_Id { get; set; } public string? Tran_Code { get; set; } }

    public class SignStatusResponse
    {
        public int Status_Code { get; set; }
        public string? Message { get; set; }
        public SignStatusData? Data { get; set; }
    }
    public class SignStatusData { public string? Transaction_Id { get; set; } public List<SignedFile>? Signatures { get; set; } }
    public class SignedFile { public string? Doc_Id { get; set; } public string? Signature_Value { get; set; } public string? Timestamp_Signature { get; set; } }
}
