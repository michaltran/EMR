using EMR.Mobile.Services;

namespace EMR.Mobile.Pages;

public partial class HoSoListPage : ContentPage
{
    private readonly ApiClient _api;
    private List<HoSoListItem> _all = new();

    public HoSoListPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Reload();
    }

    private async Task Reload()
    {
        try
        {
            Refresher.IsRefreshing = true;
            _all = await _api.GetHoSosAsync() ?? new();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi tải dữ liệu", ex.Message, "OK");
        }
        finally { Refresher.IsRefreshing = false; }
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        var k = (SearchBox.Text ?? "").Trim().ToLowerInvariant();
        HoSoList.ItemsSource = string.IsNullOrEmpty(k)
            ? _all
            : _all.Where(h =>
                h.MaHoSo.ToLower().Contains(k) ||
                h.HoTenBenhNhan.ToLower().Contains(k) ||
                h.MaBenhNhanHIS.ToLower().Contains(k)).ToList();
    }

    private async void OnRefresh(object? sender, EventArgs e) => await Reload();

    private async void OnItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not HoSoListItem item) return;
        HoSoList.SelectedItem = null;
        await Shell.Current.GoToAsync($"detail?id={item.Id}");
    }
}
