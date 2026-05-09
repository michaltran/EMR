using EMR.Mobile.Services;

namespace EMR.Mobile.Pages;

[QueryProperty(nameof(HoSoId), "id")]
public partial class HoSoDetailPage : ContentPage
{
    private readonly ApiClient _api;
    public string? HoSoId { get; set; }
    private HoSoDetail? _detail;

    public HoSoDetailPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!Guid.TryParse(HoSoId, out var gid)) return;
        try
        {
            Loading.IsRunning = true;
            _detail = await _api.GetHoSoDetailAsync(gid);
            if (_detail is null)
            {
                await DisplayAlert("Không tìm thấy", "Hồ sơ không tồn tại hoặc bạn không có quyền xem.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }
            MaHoSoLbl.Text = _detail.MaHoSo;
            BenhNhanLbl.Text = $"{_detail.HoTenBenhNhan} (mã BN: {_detail.MaBenhNhanHIS})";
            KhoaLbl.Text = $"{_detail.KhoaTen} • Tạo {_detail.NgayTao.ToLocalTime():dd/MM/yyyy HH:mm}";
            TrangThaiLbl.Text = _detail.TrangThai;
            InfoCard.IsVisible = true;
            TaiLieuList.ItemsSource = _detail.TaiLieus;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", ex.Message, "OK");
        }
        finally { Loading.IsRunning = false; }
    }

    private async void OnXemPdf(object? sender, EventArgs e)
    {
        if (sender is not Button b || b.CommandParameter is not Guid tlId || _detail is null) return;
        var url = _api.PdfUrl(_detail.Id, tlId);
        try
        {
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi mở PDF", ex.Message, "OK");
        }
    }
}
