using ControlzEx.Theming;
using MahApps.Metro;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SymbolEditorApp
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            UpdateVersion();
            DarkModeButton.IsChecked = UserSettings.Default.IsDarkModeEnabled;
            LightModeButton.IsChecked = !UserSettings.Default.IsDarkModeEnabled;
            AccentSelector.ItemsSource = new string[]
            {
                "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna"
            };
            AccentSelector.SelectedItem = ThemeManager.Current.ColorSchemes.Where(c => c == UserSettings.Default.ThemeName).FirstOrDefault();
        }

        private void UpdateVersion()
        {
            string version = null;            
            var id = MsixHelpers.GetPackageId();
            if (id != null)
                version = id.Version.ToString();
            else
            {
                version = typeof(SettingsWindow).Assembly.GetName().Version?.ToString();
            }
            version += ". Powered by ArcGIS Runtime v" + typeof(Esri.ArcGISRuntime.ArcGISRuntimeEnvironment).Assembly.GetName().Version?.ToString();
            versionInfo.Text = version;
        }

        private void AccentSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedAccent = AccentSelector.SelectedItem as string;
            if (selectedAccent != null)
            {
                MapViewModel.Current.SetTheme(selectedAccent);
            }
        }

        private void SetDarkMode(object sender, RoutedEventArgs e) => MapViewModel.Current.SetDarkMode(true);

        private void SetLightMode(object sender, RoutedEventArgs e) => MapViewModel.Current.SetDarkMode(false);

    }
}
