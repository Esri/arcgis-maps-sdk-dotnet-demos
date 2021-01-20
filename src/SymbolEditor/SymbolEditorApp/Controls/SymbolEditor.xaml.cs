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

namespace SymbolEditorApp.Controls
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
            DependencyProperty.Register("Symbol", typeof(Symbol), typeof(SymbolEditor), new PropertyMetadata(null, OnSymbolPropertyChanged));

        private static void OnSymbolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
          //  ((SymbolEditor)d).UpdateSymbolEditor();
        }

        //private void UpdateSymbolEditor()
        //{
        //    if (Symbol is SimpleMarkerSymbol sms)
        //    {
        //        SymbolEditorContent.Content = new SymbolEditors.SimpleMarkerSymbolEditor() { Symbol = sms };
        //    }
        //    else if (Symbol is Esri.ArcGISRuntime.Symbology.MultilayerSymbol mls)
        //    {
        //        SymbolEditorContent.Content = new AutoPropertyGrid() { Value = mls };
        //        //SymbolEditorContent.Content = new SymbolEditors.MultilayerSymbolEditor() { Symbol = mls };
        //    }
        //    else
        //    {
        //        SymbolEditorContent.Content = new AutoPropertyGrid() { Value = Symbol };
        //        //SymbolEditorContent.Content = new TextBlock() { Text = "Symbol type not yet supported " };
        //    }
        //}

        private void PickFromSymbolStyle_Click(object sender, RoutedEventArgs e)
        {
            var picker = new SymbolPicker();

            if (MetroDialog.ShowDialog("Symbol Picker", picker, this) == true)
            {
                var symbol = picker.SelectedItem?.StyleSymbol;
                if (symbol != null)
                    Symbol = symbol;
            }
        }

        private void NewSimpleMarker_Click(object sender, RoutedEventArgs e) => Symbol = new SimpleMarkerSymbol();

        private void NewSimpleLine_Click(object sender, RoutedEventArgs e) => Symbol = new SimpleLineSymbol();

        private void NewSimpleFill_Click(object sender, RoutedEventArgs e) => Symbol = new SimpleFillSymbol();

        private void NewSymbol_Click(object sender, RoutedEventArgs e)
        {
            var menu = (sender as Button)?.ContextMenu;
            if (menu != null)
            {
                menu.IsEnabled = true;
                menu.PlacementTarget = sender as UIElement;
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menu.IsOpen = true;
            }
        }
    }
}
