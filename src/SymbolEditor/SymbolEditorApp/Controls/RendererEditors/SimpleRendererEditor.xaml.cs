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

namespace SymbolEditorApp.Controls.RendererEditors
{
    public partial class SimpleRendererEditor : UserControl
    {
        public SimpleRendererEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public SimpleRenderer Renderer
        {
            get { return (SimpleRenderer)GetValue(RendererProperty); }
            set { SetValue(RendererProperty, value); }
        }

        public static readonly DependencyProperty RendererProperty =
            DependencyProperty.Register("Renderer", typeof(SimpleRenderer), typeof(SimpleRendererEditor), new PropertyMetadata(null));

        private void SymbolButton_Click(object sender, RoutedEventArgs e)
        {
            if (Renderer.Symbol == null)
                return;
            var editor = new SymbolEditor();
            editor.Symbol = Renderer.Symbol?.Clone() ?? new SimpleMarkerSymbol();
            var result = MetroDialog.ShowDialog("Symbol Editor", editor, this);
            if (result == true)
            {
                Renderer.Symbol = editor.Symbol;
            }
        }
    }
}
