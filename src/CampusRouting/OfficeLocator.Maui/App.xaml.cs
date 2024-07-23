namespace OfficeLocator;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        ProvisionDataHelper.AppSettings = new Preferences();
        ProvisionDataHelper.AppDataDirectory = FileSystem.AppDataDirectory;

        MainPage = new CampusMapPage();
    }

    private class Preferences : IPreferences
    {
        public bool ContainsKey(string key) => Microsoft.Maui.Storage.Preferences.Default.ContainsKey(key);

        public T Get<T>(string key, T defaultValue) => Microsoft.Maui.Storage.Preferences.Default.Get(key, defaultValue);

        public void Remove(string key) => Microsoft.Maui.Storage.Preferences.Default.Remove(key);

        public void Set<T>(string key, T value) => Microsoft.Maui.Storage.Preferences.Default.Set(key, value);
    }
}