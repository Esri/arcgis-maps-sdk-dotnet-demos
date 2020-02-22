using Esri.ArcGISRuntime.Symbology;
using SymbolEditorApp.Controls.RendererEditors;
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
    /// Interaction logic for SymbologyEditor.xaml
    /// </summary>
    public partial class SymbologyEditor : UserControl
    {
        public SymbologyEditor()
        {
            InitializeComponent();
        }

        private void RendererType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RendererType.SelectedIndex == 0)
            {
                if (!(Renderer is SimpleRenderer))
                {
                    Renderer = new SimpleRenderer(new SimpleMarkerSymbol());
                    return;
                }
            }
            else if (RendererType.SelectedIndex == 1)
            {
                if (!(Renderer is UniqueValueRenderer))
                {
                    Renderer = new UniqueValueRenderer();
                    return;
                }
            }
            else if (RendererType.SelectedIndex == 2)
            {
                if (!(Renderer is ClassBreaksRenderer))
                {
                    Renderer = new ClassBreaksRenderer();
                    return;
                }
            }
        }

        private void OnRendererChanged()
        {
            if(Renderer == null)
            {
                Renderer = new SimpleRenderer(new SimpleMarkerSymbol());
                return;
            }
            if (Renderer is SimpleRenderer sr)
            {
                RendererType.SelectedIndex = 0;
                RendererEditorView.Content = new SimpleRendererEditor() { Renderer = sr };
            }
            else if (Renderer is UniqueValueRenderer uvr)
            {
                RendererType.SelectedIndex = 1;
                RendererEditorView.Content = new UniqueValueRendererEditor() { Renderer = uvr };
            }
            else if (Renderer is ClassBreaksRenderer)
            {
                RendererType.SelectedIndex = 2;
            }
            else if (Renderer is DictionaryRenderer)
            {
                // TODO
                MessageBox.Show("Dictionary Renderer is not yet supported");
                System.Diagnostics.Debugger.Break();
            }
        }


        public Renderer Renderer
        {
            get { return (Renderer)GetValue(RendererProperty); }
            set { SetValue(RendererProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Renderer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RendererProperty =
            DependencyProperty.Register("Renderer", typeof(Renderer), typeof(SymbologyEditor), new PropertyMetadata(null, OnRendererPropertyChanged));

        private static void OnRendererPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SymbologyEditor)d).OnRendererChanged();
        }
    }
}
