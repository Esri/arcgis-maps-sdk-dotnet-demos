using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
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
    /// Interaction logic for SymbolViewer.xaml
    /// </summary>
    public partial class SymbolViewer : UserControl
    {
        static readonly MapPoint nullIsland = new MapPoint(0, 0, SpatialReferences.Wgs84);
        Graphic graphic2D = new Graphic() { Geometry = nullIsland };
        // Graphic graphic3D = new Graphic() { Geometry = nullIsland };
        public SymbolViewer()
        {
            InitializeComponent();
            var go = new GraphicsOverlay()
            {
                ScaleSymbols = true,
                RenderingMode = GraphicsRenderingMode.Static
            };
            mapView.InteractionOptions = new MapViewInteractionOptions() { IsPanEnabled = false, IsZoomEnabled = false, IsEnabled = false };
            var map = new Map(SpatialReferences.Wgs84)
            {
                InitialViewpoint = new Viewpoint(nullIsland, 10000),
                ReferenceScale = 10000,
                MinScale = 10000,
                MaxScale = 10000 / 10,
            };
            go.Graphics.Add(graphic2D);
            mapView.GraphicsOverlays.Add(go);
            mapView.Map = map;
        }

        private void RefreshSymbol()
        {
            graphic2D.Symbol = Symbol;
            // graphic3D.Symbol = Symbol;
            if (Symbol != null)
                mapView.Visibility = Visibility;
        }

        public Symbol Symbol
        {
            get { return (Symbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(nameof(Symbol), typeof(Symbol), typeof(SymbolViewer), new PropertyMetadata(null, OnSymbolPropertyChanged));

        private static void OnSymbolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SymbolViewer)d).RefreshSymbol();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mapView.SetViewpoint(new Viewpoint(nullIsland, 10000 / e.NewValue));
        }
    }
}
