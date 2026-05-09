using EMR.Mobile.Pages;
using EMR.Mobile.Services;

namespace EMR.Mobile;

public partial class App : Application
{
    public App(IServiceProvider sp)
    {
        InitializeComponent();

        if (ApiClient.IsAuthenticated)
        {
            MainPage = new AppShell();
        }
        else
        {
            MainPage = new NavigationPage(sp.GetRequiredService<LoginPage>());
        }
    }
}
