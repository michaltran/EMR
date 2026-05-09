using EMR.Mobile.Pages;
using EMR.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace EMR.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HoSoListPage>();
        builder.Services.AddTransient<HoSoDetailPage>();

        Routing.RegisterRoute("detail", typeof(HoSoDetailPage));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
