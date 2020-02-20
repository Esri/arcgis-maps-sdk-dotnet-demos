using Esri.ArcGISRuntime.Symbology;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for SymbolEditor.xaml
    /// </summary>
    public partial class SymbolEditor : UserControl
    {
        public SymbolEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Symbol Symbol
        {
            get { return (Symbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol", typeof(Symbol), typeof(SymbolEditor), new PropertyMetadata(null));

        private void PickFromSymbolStyle_Click(object sender, RoutedEventArgs e)
        {
            var picker = new SymbolPicker();
            var window = new MetroWindow()
            {
                Width = 600,
                Height = 500,
                Content = picker,
                Title = "Symbol Style Browser"
            };

            if (window.ShowDialog() == true)
            {
                var symbol = picker.SelectedItem?.Symbol;
                if (symbol != null)
                    Symbol = symbol;
            }
        }
    }
}
