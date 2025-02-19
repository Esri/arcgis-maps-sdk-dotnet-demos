using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Maui;
using Microsoft.Extensions.Logging;

namespace BackgroundLocationTracking
{
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
                })
                .UseArcGISRuntime(config => config.UseApiKey("<API_KEY>"));
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
