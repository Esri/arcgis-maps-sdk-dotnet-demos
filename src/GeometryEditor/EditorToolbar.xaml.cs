using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Editing;
using System.Windows;
using System.Windows.Controls;

namespace EditorDemo
{
    public partial class EditorToolbar : UserControl
    {
        private readonly EditorToolbarController _controller = new EditorToolbarController();

        public EditorToolbar()
        {
            InitializeComponent();
            this.DataContext = _controller;
            _controller.PropertyChanged += Controller_PropertyChanged;
            _controller.EditingCompleted += (s,e) => EditingCompleted?.Invoke(this, new EditingCompletedEventArgs(e));
            _controller.EditingCancelled += (s, e) => EditingCancelled?.Invoke(this, new EventArgs());
        }
        public class EditingCompletedEventArgs : EventArgs
        {
            public EditingCompletedEventArgs(Geometry geometry)
            {
                Geometry = geometry;
            }
            public Geometry Geometry { get; }

        }

        public event EventHandler<EditingCompletedEventArgs>? EditingCompleted;

        public event EventHandler? EditingCancelled;

        private void Controller_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditorToolbarController.GeometryEditor))
            {
                GeometryEditor = _controller.GeometryEditor;
            }
        }

        internal static readonly DependencyPropertyKey GeometryEditorPropertyKey =
        DependencyProperty.RegisterReadOnly(
          name: nameof(GeometryEditor),
          propertyType: typeof(GeometryEditor),
          ownerType: typeof(EditorToolbar),
          typeMetadata: new FrameworkPropertyMetadata());

        /// <summary>
        /// Gets the active geometry editor. Bind this back to the MapView
        /// </summary>
        public GeometryEditor? GeometryEditor
        {
            get => (GeometryEditor)GetValue(GeometryEditorPropertyKey.DependencyProperty);
            private set => SetValue(GeometryEditorPropertyKey, value);
        }

        /// <summary>
        /// The GeoElement that should be edited
        /// </summary>
        public GeoElement? GeoElement
        {
            get { return (GeoElement)GetValue(GeoElementProperty); }
            set { SetValue(GeoElementProperty, value); }
        }

        public static readonly DependencyProperty GeoElementProperty =
            DependencyProperty.Register(nameof(GeoElement), typeof(GeoElement), typeof(EditorToolbar), new PropertyMetadata(null, (s, e) => ((EditorToolbar)s).OnGeoElementPropertyChanged(e.OldValue as GeoElement, e.NewValue as GeoElement)));

        private void OnGeoElementPropertyChanged(GeoElement? oldElement, GeoElement? newElement)
        {
            _controller.GeoElement = newElement;
        }

        /// <summary>
        /// Reference to the MapView's GraphicsOverlays
        /// </summary>
        public GraphicsOverlayCollection GraphicsOverlays
        {
            get { return (GraphicsOverlayCollection)GetValue(GraphicsOverlaysProperty); }
            set { SetValue(GraphicsOverlaysProperty, value); }
        }

        public static readonly DependencyProperty GraphicsOverlaysProperty =
            DependencyProperty.Register(nameof(GraphicsOverlays), typeof(GraphicsOverlayCollection), typeof(EditorToolbar), new PropertyMetadata(null, (s, e) => ((EditorToolbar)s).OnGraphicsOverlayPropertyChanged(e.OldValue as GraphicsOverlayCollection, e.NewValue as GraphicsOverlayCollection)));

        private void OnGraphicsOverlayPropertyChanged(GraphicsOverlayCollection? oldOverlays, GraphicsOverlayCollection? newOverlays)
        {
            _controller.GraphicsOverlays = newOverlays;
        }

        /// <summary>
        /// Returns true if this geoelement supports geometry editing.
        /// </summary>
        /// <param name="geoElement"></param>
        /// <returns></returns>
        public static bool CanEditGeometry(GeoElement? geoElement) => EditorToolbarController.CanEditGeometry(geoElement);
    }
}
