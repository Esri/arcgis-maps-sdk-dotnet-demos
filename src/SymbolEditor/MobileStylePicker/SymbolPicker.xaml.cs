using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MobileStylePicker
{
    /// <summary>
    /// Interaction logic for SymbolPicker.xaml
    /// </summary>
    public partial class SymbolPicker : MahApps.Metro.Controls.MetroWindow
    {
        static ObservableCollection<SymbolStyleItems> symbols = new ObservableCollection<SymbolStyleItems>();
        public class SymbolStyleItems
        {
            public SymbolStyle Style { get; set; }
            public string Name { get; set; }
            public override string ToString() => Name;
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
                var pro2d = await SymbolStyle.OpenAsync("ArcGISRuntime2D_Pro25.stylx");
                symbols.Add(new SymbolStyleItems() { Style = pro2d, Name = "2D Web Styles" });
                //var pro3d = await SymbolStyle.OpenAsync("ArcGISRuntime3D_Pro25.stylx");
                //symbols.Add(new SymbolStyleItems() { Style = pro3d, Name = "3D Web Styles" });
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
            IList<SymbolStyleSearchResult> styleResults = null;
            if (e.AddedItems.Count == 1)
            {
                try
                {
                    // Search the style with the default parameters to return all symbol results.
                    SymbolStyleSearchParameters searchParams = await SymbolStyle.GetDefaultSearchParametersAsync();
                    searchParams.Categories.Clear();
                    searchParams.Categories.Add(e.AddedItems[0] as string);
                    styleResults = await SymbolStyle.SearchSymbolsAsync(searchParams);
                }
                catch { }
            }
            SymbolList.ItemsSource = styleResults;
            if (styleResults?.Count > 0)
                SymbolList.SelectedIndex = 0;
        }

        private void SymbolList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var oldValue = SelectedItem;
            SymbolStyleSearchResult result = null;
            if(e.AddedItems.Count == 1)
            {
                result = e.AddedItems[0] as SymbolStyleSearchResult;
            }
            if (result != oldValue)
            {
                SetValue(SelectedItemPropertyKey, result);
                SelectedItemChanged?.Invoke(this, result);
            }
        }

        public event EventHandler<SymbolStyleSearchResult> SelectedItemChanged;

        public SymbolStyleSearchResult SelectedItem
        {
            get { return (SymbolStyleSearchResult)GetValue(SelectedItemProperty); }
        }

        public static readonly DependencyPropertyKey SelectedItemPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(SelectedItem), typeof(SymbolStyleSearchResult), typeof(SymbolPicker), new PropertyMetadata(null));

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
