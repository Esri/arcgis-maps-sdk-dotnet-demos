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
    /// <summary>
    /// Interaction logic for SymbolMarkerSymbolEditor.xaml
    /// </summary>
    public partial class MultilayerSymbolEditor : UserControl
    {
        public MultilayerSymbolEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public MultilayerSymbol Symbol
        {
            get { return (MultilayerSymbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol", typeof(MultilayerSymbol), typeof(MultilayerSymbolEditor), new PropertyMetadata(null));
    }
}
