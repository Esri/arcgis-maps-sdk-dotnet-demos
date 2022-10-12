using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using MahApps.Metro;
using System.Windows;

namespace SymbolEditorApp
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    public class MapViewModel : INotifyPropertyChanged
    {
        private static MapViewModel _Current;

        public static MapViewModel Current => _Current ?? (_Current = new MapViewModel());

        private Basemap _darkModeBaseMap = new Basemap(BasemapStyle.ArcGISNavigationNight);
        private Basemap _lightModeBaseMap = new Basemap(BasemapStyle.ArcGISNavigation);

        private MapViewModel()
        {
            SetDarkMode(UserSettings.Default.IsDarkModeEnabled);
            SetTheme(UserSettings.Default.ThemeName);
            _map.OperationalLayers.Add(new FeatureLayer(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0")));
        }
    
        private Map _map = new Map();

        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Map Map
        {
            get => _map;
            set { _map = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetDarkMode(bool isDarkEnabled)
        {
            ThemeManager.ChangeThemeBaseColor(Application.Current, isDarkEnabled ? ThemeManager.BaseColorDark : ThemeManager.BaseColorLight);
            _map.Basemap = isDarkEnabled ? _darkModeBaseMap : _lightModeBaseMap;
            UserSettings.Default.IsDarkModeEnabled = isDarkEnabled;
            UserSettings.Default.Save();
        }

        public void SetTheme(string name)
        {
            ThemeManager.ChangeThemeColorScheme(Application.Current, name);
            UserSettings.Default.ThemeName = name;
            UserSettings.Default.Save();
        }
    }
}
