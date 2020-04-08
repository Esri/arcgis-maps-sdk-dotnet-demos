using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for CollectionPropertyGrid.xaml
    /// </summary>
    public partial class SymbolLayersList : UserControl
    {
        SymbolLayerConverter c;
        public SymbolLayersList()
        {
            InitializeComponent();
            c = (SymbolLayerConverter)Resources["symbolConverter"];
        }


        public MultilayerSymbol Symbol
        {
            get { return (MultilayerSymbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol", typeof(MultilayerSymbol), typeof(SymbolLayersList), new PropertyMetadata(null, (d,e)=>((SymbolLayersList)d).OnSymbolPropertyChanged(e)));

        private void OnSymbolPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            c.Symbol = Symbol;
            Items.ItemsSource = null;
            if (e.NewValue is MultilayerSymbol)
            {
                Items.ItemsSource = Symbol.SymbolLayers;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var value = (sender as Button).DataContext;
            MetroDialog.ShowDialog(value.GetType().Name, new AutoPropertyGrid() { Value = value, MaxWidth = 400, MinWidth = 300 }, this, showCancel: false);
            // Trigger refresh
            var items = Items.ItemsSource;
            Items.ItemsSource = null;
            Items.ItemsSource = items;
        }
    }

    public class SymbolLayerConverter : IValueConverter
    {
        public MultilayerSymbol Symbol { get; set; }
        public SymbolLayerConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var idx = Symbol.SymbolLayers.IndexOf(value as SymbolLayer);
            var s = (Esri.ArcGISRuntime.Symbology.MultilayerSymbol)Symbol.Clone();
            for (int i = 0; i < idx; i++)
                s.SymbolLayers.RemoveAt(0);
            for (int i = 1; i < s.SymbolLayers.Count; i++)
                s.SymbolLayers.RemoveAt(1);
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
