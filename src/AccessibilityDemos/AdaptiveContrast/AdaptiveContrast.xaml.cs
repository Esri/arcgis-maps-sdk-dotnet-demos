using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Microsoft.Win32;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Windows.UI.ViewManagement;
using Colors = System.Drawing.Color;

namespace DemoApplicationAccessibility.Contrast
{
    public partial class AdaptiveContrast : UserControl
    {
        private readonly UISettings _uiSettings = new();

        public AdaptiveContrast()
        {
            InitializeComponent();
            MyMapView.Map = new Map(); // default placeholder
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        private void OnLoaded(object s, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            _uiSettings.ColorValuesChanged += OnColorValuesChanged;
            _ = UpdateBasemapAsync();
        }

        private void OnUnloaded(object s, RoutedEventArgs e)
        {
            // Unsubscribe from events to prevent memory leaks
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            _uiSettings.ColorValuesChanged -= OnColorValuesChanged;
        }

        private async void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
                await UpdateBasemapAsync();
        }

        private async void OnColorValuesChanged(UISettings sender, object args)
        {
            await UpdateBasemapAsync();
        }

        private async Task UpdateBasemapAsync()
        {
            bool isHighContrast = SystemParameters.HighContrast;
            bool isLightTheme = IsColorLight(ConvertToMediaColor(_uiSettings.GetColorValue(UIColorType.Background)));

            await ApplyBasemapAsync(isHighContrast, isLightTheme);

            // Ensure UI thread for grid update
            await Dispatcher.InvokeAsync(() => ApplyGrid(isHighContrast, isLightTheme));
        }

        private async Task ApplyBasemapAsync(bool highContrast, bool isLightTheme)
        {
            try
            {
                string itemId = (highContrast, isLightTheme) switch
                {
                    (true, true) => "084291b0ecad4588b8c8853898d72445", // High contrast light
                    (true, false) => "3e23478909194c54992eaaee78b5f754", // High contrast dark
                    (false, true) => "979c6cc89af9449cbeb5342a439c6a76", // light gray canvas
                    (false, false) => "1970c1995b8f44749f4b9b6e81b5ba45"  // dark gray canvas
                };

                var portal = await ArcGISPortal.CreateAsync();
                var portalItem = await PortalItem.CreateAsync(portal, itemId);
                MyMapView.Map.Basemap = new Basemap(portalItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set basemap: {ex.Message}", "Basemap Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyGrid(bool highContrast, bool isLightTheme)
        {
            Colors gridColor = GetColor((highContrast, isLightTheme), ("#000000", "#FFFFFF", "#999999", "#CCCCCC"));
            Colors haloColor = GetColor((highContrast, isLightTheme), ("#FFFFFF", "#000000", "#F0F0F0", "#222222"));
            Colors labelColor = GetColor((highContrast, isLightTheme), ("#000000", "#FFFFFF", "#999999", "#CCCCCC"));

            var grid = new LatitudeLongitudeGrid
            {
                IsLabelVisible = true,
                IsVisible = true,
                LabelPosition = GridLabelPosition.Geographic
            };

            // Apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < grid.LevelCount; level++)
            {
                grid.SetLineSymbol(level, new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, gridColor, 2));
                grid.SetTextSymbol(level, new TextSymbol
                {
                    Color = labelColor,
                    OutlineColor = haloColor,
                    HaloColor = haloColor,
                    HaloWidth = 3,
                    Size = 16
                });
            }

            MyMapView.Grid = grid;
        }

        private static Colors GetColor((bool highContrast, bool isLight) mode, (string hcLight, string hcDark, string light, string dark) colors)
        {
            return mode switch
            {
                (true, true) => ColorTranslator.FromHtml(colors.hcLight),
                (true, false) => ColorTranslator.FromHtml(colors.hcDark),
                (false, true) => ColorTranslator.FromHtml(colors.light),
                (false, false) => ColorTranslator.FromHtml(colors.dark)
            };
        }

        private static Colors ConvertToMediaColor(Windows.UI.Color color) =>
            Colors.FromArgb(color.A, color.R, color.G, color.B);

        // Heuristic to detect dark mode vs light mode based on system background color
        private static bool IsColorLight(Colors clr) =>
            ((5 * clr.G) + (2 * clr.R) + clr.B) > (8 * 128);
    }
}

