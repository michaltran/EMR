using EMR.Mobile.Services;

namespace EMR.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ApiClient _api;

    public LoginPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        ServerEntry.Text = ApiClient.ApiBaseUrl;
    }

    private async void OnLogin(object? sender, EventArgs e)
    {
        ErrLabel.IsVisible = false;
        LoginBtn.IsEnabled = false;
        LoginBtn.Text = "Đang đăng nhập...";
        try
        {
            if (!string.IsNullOrWhiteSpace(ServerEntry.Text))
                ApiClient.ApiBaseUrl = ServerEntry.Text.TrimEnd('/');

            var (ok, err, _) = await _api.LoginAsync(UserEntry.Text ?? "", PassEntry.Text ?? "");
            if (ok)
            {
                Application.Current!.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//hoso");
            }
            else
            {
                ErrLabel.Text = err;
                ErrLabel.IsVisible = true;
            }
        }
        finally
        {
            LoginBtn.IsEnabled = true;
            LoginBtn.Text = "Đăng nhập";
        }
    }
}
