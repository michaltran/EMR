using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EMR.Mobile.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(30);
        ApplyToken();
    }

    public static string ApiBaseUrl
    {
        get => Preferences.Default.Get(nameof(ApiBaseUrl), DefaultBaseUrl);
        set => Preferences.Default.Set(nameof(ApiBaseUrl), value);
    }

    public static string Token
    {
        get => Preferences.Default.Get(nameof(Token), "");
        set => Preferences.Default.Set(nameof(Token), value);
    }

    public static string HoTen
    {
        get => Preferences.Default.Get(nameof(HoTen), "");
        set => Preferences.Default.Set(nameof(HoTen), value);
    }

    public static bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    /// <summary>Default base URL: 10.0.2.2 cho Android emulator host loopback. Đổi thành IP LAN khi test trên máy thật.</summary>
    public static string DefaultBaseUrl =>
#if ANDROID
        "http://10.0.2.2:5099";
#else
        "http://localhost:5099";
#endif

    public void ApplyToken()
    {
        if (!string.IsNullOrEmpty(Token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        else
            _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<(bool ok, string? error, LoginResponse? data)> LoginAsync(string username, string password)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync($"{ApiBaseUrl}/api/auth/login", new { tenDangNhap = username, matKhau = password });
            if (!resp.IsSuccessStatusCode) return (false, $"Sai tài khoản hoặc server không phản hồi ({(int)resp.StatusCode})", null);
            var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            if (data is null) return (false, "Response rỗng", null);
            Token = data.Token;
            HoTen = data.HoTen;
            ApplyToken();
            return (true, null, data);
        }
        catch (Exception ex) { return (false, ex.Message, null); }
    }

    public void Logout()
    {
        Token = "";
        HoTen = "";
        ApplyToken();
    }

    public async Task<List<HoSoListItem>?> GetHoSosAsync()
    {
        var resp = await _http.GetAsync($"{ApiBaseUrl}/api/hoso/");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<List<HoSoListItem>>();
    }

    public async Task<HoSoDetail?> GetHoSoDetailAsync(Guid id)
    {
        var resp = await _http.GetAsync($"{ApiBaseUrl}/api/hoso/{id}");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<HoSoDetail>();
    }

    public string PdfUrl(Guid hoSoId, Guid taiLieuId) =>
        $"{ApiBaseUrl}/api/hoso/{hoSoId}/tailieu/{taiLieuId}/file";
}

public record LoginResponse(string Token, DateTime ExpiresAt, string HoTen, string[] VaiTro, Guid? KhoaId);
public record HoSoListItem(Guid Id, string MaHoSo, string MaBenhNhanHIS, string HoTenBenhNhan, string KhoaTen, string TrangThai, int SoTaiLieu, DateTime NgayTao);
public record HoSoDetail(
    Guid Id, string MaHoSo, string MaBenhNhanHIS, string? MaLanKhamHIS, string HoTenBenhNhan,
    DateTime? NgaySinh, byte? GioiTinh, string KhoaTen, string TrangThai, string KhoLuuTru,
    DateTime NgayTao, List<TaiLieuItem> TaiLieus);
public record TaiLieuItem(Guid Id, string LoaiTaiLieu, string TenFile, long KichThuoc, string TrangThaiKy, List<ChuKyItem> ChuKys);
public record ChuKyItem(Guid Id, string VaiTroKy, string LoaiCa, string TrangThai, string? CertSubject, DateTime NgayYeuCau, DateTime? NgayHoanTat);
