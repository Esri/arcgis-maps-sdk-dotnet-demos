using Esri.ArcGISRuntime.Symbology;
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

namespace SymbolEditorApp.Controls.SymbolEditors
{
    public partial class SimpleMarkerSymbolEditor : UserControl
    {
        public SimpleMarkerSymbolEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public SimpleMarkerSymbol Symbol
        {
            get { return (SimpleMarkerSymbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol", typeof(SimpleMarkerSymbol), typeof(SimpleMarkerSymbolEditor), new PropertyMetadata(null));
    }
}
