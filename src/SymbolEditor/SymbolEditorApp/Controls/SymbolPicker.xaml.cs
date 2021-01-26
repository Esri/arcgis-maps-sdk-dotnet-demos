using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SymbolEditorApp.Controls
{
    /// <summary>
    /// Interaction logic for SymbolPicker.xaml
    /// </summary>
    public partial class SymbolPicker : UserControl
    {
        static ObservableCollection<SymbolStyleItems> symbols = new ObservableCollection<SymbolStyleItems>();
        SymbolStyleSearchParameters searchParams = new SymbolStyleSearchParameters();

        public class SymbolStyleItems
        {
            public SymbolStyle Style { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
        }

        public class StyleSearchResult : INotifyPropertyChanged
        {
            private Task<Symbol> _symbol;
            private async System.Threading.Tasks.Task Initialize()
            {
                _symbol = SymbolStyleSearchResult.GetSymbolAsync();
                if (_symbol.IsCompletedSuccessfully)
                    return;
                var symbol = await _symbol;
                OnPropertyChanged(nameof(StyleSymbol));
            }


            public SymbolStyleSearchResult SymbolStyleSearchResult { get; set; }

            public Symbol StyleSymbol
            {
                get
                {
                    if (_symbol == null)
                        _ = Initialize();
                    if (_symbol.IsCompletedSuccessfully)
                        return _symbol.Result;

                    return null;
                }

            }

            /// <summary>
            /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
            /// </summary>
            /// <param name="propertyName">The name of the property that has changed</param>
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public SymbolPicker()
        {
            InitializeComponent();
            LoadSymbols();
        }

        public async void LoadSymbols()
        {
            SymbolStylePicker.ItemsSource = symbols;
            if (symbols.Count == 0)
            {
                // SymbolStyle from a web style. Esri2DPointSymbolsStyle is an esri registered web style available out of the box on ArcGIS.com. 
                // Null portal will fetch the 2D symbols style from ArcGIS.com. 
                // If you want to fetch the esri registered web style from your custom portal then you should pass your Portal instance as a parameter.
                var esri_webstyle = await SymbolStyle.OpenAsync(styleName: "Esri2DPointSymbolsStyle", portal: null);
                symbols.Add(new SymbolStyleItems() { Style = esri_webstyle, Name = "Esri 2D Symbol Style - Web" });

                // SymbolStyle from a web style with custom symbols published on a portal.                
                var custom_webstyle = await SymbolStyle.OpenAsync(new Uri("https://www.arcgis.com/home/item.html?id=ee2aafab25f941c89a5e814e14a44d5d"));
                symbols.Add(new SymbolStyleItems() { Style = custom_webstyle, Name = "Custom Symbol Style - Web" });

                // SymbolStyle from a local stylx file on disk.
                var local_style_file = await SymbolStyle.OpenAsync(styleLocation: @"Resources\ArcGISRuntime2D_Pro25.stylx");
                symbols.Add(new SymbolStyleItems() { Style = local_style_file, Name = "Esri 2D Symbol Style - Local" });
            }
            SymbolStylePicker.SelectedIndex = 0;
        }

        private async void LoadSymbolStyle()
        {
            IEnumerable<string> categories = null;
            if (SymbolStyle != null)
            {
                try
                {
                    SymbolStyleSearchParameters searchParams = await SymbolStyle.GetDefaultSearchParametersAsync();
                    categories = searchParams.Categories;
                }
                catch { }
            }
            CategoryPicker.ItemsSource = categories;
            CategoryPicker.SelectedIndex = 0;
        }

        private async void categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count == 1)
                {
                    ObservableCollection<StyleSearchResult> styleSearchResults = new ObservableCollection<StyleSearchResult>();
                    searchParams.Categories.Clear();
                    searchParams.Categories.Add(e.AddedItems[0] as string);
                    IList<SymbolStyleSearchResult> symbolStyleSearchResults;
                    symbolStyleSearchResults = await SymbolStyle.SearchSymbolsAsync(searchParams);
                    foreach (var searchResult in symbolStyleSearchResults)
                    {
                        var styleSearchResult = new StyleSearchResult();
                        styleSearchResult.SymbolStyleSearchResult = searchResult;
                        styleSearchResults.Add(styleSearchResult);
                    }
                    if (styleSearchResults?.Count > 0)
                    {
                        SymbolList.SelectedIndex = 0;
                    }
                    SymbolList.ItemsSource = styleSearchResults;
                }
            }
            catch { }

        }

        private void SymbolList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var oldValue = SelectedItem;
            StyleSearchResult result = null;
            if (e.AddedItems.Count == 1)
            {
                result = e.AddedItems[0] as StyleSearchResult;
            }
            if (result != oldValue)
            {
                SetValue(SelectedItemPropertyKey, result);
                SelectedItemChanged?.Invoke(this, result);
            }
        }

        public event EventHandler<StyleSearchResult> SelectedItemChanged;

        public StyleSearchResult SelectedItem
        {
            get { return (StyleSearchResult)GetValue(SelectedItemProperty); }
        }

        public static readonly DependencyPropertyKey SelectedItemPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(SelectedItem), typeof(StyleSearchResult), typeof(SymbolPicker), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty = SelectedItemPropertyKey.DependencyProperty;

        public SymbolStyle SymbolStyle
        {
            get { return (SymbolStyle)GetValue(SymbolStyleProperty); }
            set { SetValue(SymbolStyleProperty, value); }
        }

        public static readonly DependencyProperty SymbolStyleProperty =
            DependencyProperty.Register("SymbolStyle", typeof(SymbolStyle), typeof(SymbolPicker), new PropertyMetadata(null, OnSymbolStylePropertyChanged));

        private static void OnSymbolStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SymbolPicker)d).LoadSymbolStyle();
        }

        private async void LoadFromFile_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog() { DefaultExt = "*.stylx" };
            if (ofd.ShowDialog() == true)
            {
                var file = ofd.FileName;
                try
                {
                    var name = new System.IO.FileInfo(file).Name;
                    var style = await SymbolStyle.OpenAsync(file);
                    symbols.Add(new SymbolStyleItems() { Style = style, Name = name });
                    SymbolStylePicker.SelectedItem = symbols.Last();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Couldn't load style: " + ex.Message);
                }
            }
        }

        private void SymbolStylePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            SymbolStyle = (SymbolStylePicker.SelectedItem as SymbolStyleItems).Style;
        }
    }
}
