using MSTestX;
using Microsoft.Extensions.Logging;

namespace OfficeLocator.Tests.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            ProvisionDataHelper.AppSettings = new Preferences();
            ProvisionDataHelper.AppDataDirectory = FileSystem.AppDataDirectory;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseTestApp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private class Preferences : IPreferences
        {
            public bool ContainsKey(string key) => Microsoft.Maui.Storage.Preferences.Default.ContainsKey(key);

            public T Get<T>(string key, T defaultValue) => Microsoft.Maui.Storage.Preferences.Default.Get(key, defaultValue);

            public void Remove(string key) => Microsoft.Maui.Storage.Preferences.Default.Remove(key);

            public void Set<T>(string key, T? value) => Microsoft.Maui.Storage.Preferences.Default.Set(key, value);
        }
    }
}
