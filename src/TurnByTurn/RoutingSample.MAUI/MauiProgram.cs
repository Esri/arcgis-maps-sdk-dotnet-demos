using Esri.ArcGISRuntime.Maui;

namespace RoutingSample.MAUI
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
#warning Supply an API key in MauiProgram.cs, then delete this line
                // Create an API key at https://developers.arcgis.com/api-keys/, configure it with routing and geocoding services, then use it to replace YOUR_API_KEY below.
                .UseArcGISRuntime(apiKey: "YOUR_API_KEY");

            return builder.Build();
        }
    }
}