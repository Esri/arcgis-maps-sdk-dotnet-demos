using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ARParallaxGuidelines.Shared
{
    public class InfrastructureEditorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        public SketchEditor SketchEditor { get; } = new SketchEditor();

        private bool _doneEnabled = false;
        private bool _undoEnabled = false;
        private bool _redoEnabled = false;
        private bool _addButtonEnabled = true;
        private bool _viewReady = false;

        private double _elevationOffset = 0;

        public bool ViewReady
        {
            get => _viewReady;
            set
            {
                if (_viewReady != value)
                {
                    _viewReady = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewReady)));
                }
            }
        }

        public double ElevationOffset
        {
            get => _elevationOffset;
            set
            {
                if (_elevationOffset != value)
                {
                    _elevationOffset = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ElevationOffset)));
                }
            }
        }

        public bool DoneButtonEnabled
        {
            get => _doneEnabled;
            set
            {
                if (_doneEnabled != value)
                {
                    _doneEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DoneButtonEnabled)));
                }
            }
        }

        public bool UndoButtonEnabled
        {
            get => _undoEnabled;
            set
            {
                if (_undoEnabled != value)
                {
                    _undoEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UndoButtonEnabled)));
                }
            }
        }

        public bool RedoButtonEnabled
        {
            get => _redoEnabled;
            set
            {
                if (_redoEnabled != value)
                {
                    _redoEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RedoButtonEnabled)));
                }
            }
        }

        public bool AddButtonEnabled
        {
            get => _addButtonEnabled;
            set
            {
                if (_addButtonEnabled != value)
                {
                    _addButtonEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AddButtonEnabled)));
                }
            }
        }

        public async Task InitializeAsync()
        {
            // Create an elevation source and Surface.
            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            await _elevationSource.LoadAsync();
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            await _elevationSurface.LoadAsync();
        }

        public ICommand CompleteCommand => SketchEditor.CompleteCommand;
        public void ExecuteCompleteCommand()
        {
            if (CompleteCommand.CanExecute(null))
            {
                // Complete the sketch.
                CompleteCommand.Execute(null);
                DoneButtonEnabled = false;
                UndoButtonEnabled = false;
                RedoButtonEnabled = false;
            }
        }

        public ICommand RedoCommand => SketchEditor.RedoCommand;
        public void ExecuteRedoCommand()
        {
            if (RedoCommand.CanExecute(null))
            {
                RedoCommand.Execute(null);
            }
        }

        public ICommand UndoCommand => SketchEditor.UndoCommand;
        public void ExecuteUndoCommand()
        {
            if (UndoCommand.CanExecute(null))
            {
                UndoCommand.Execute(null);
            }
        }

        public async Task<Geometry> ExecuteAddSketch()
        {
            DoneButtonEnabled = UndoButtonEnabled = RedoButtonEnabled = true;
            AddButtonEnabled = false;

            Geometry geometry = await SketchEditor.StartAsync(SketchCreationMode.Polyline);

            AddButtonEnabled = true;

            if (!(geometry is Polyline))
            {
                return null;
            }

            try
            {
                var densifiedPolyline = (Polyline)GeometryEngine.Densify(geometry, 2);
                PolylineBuilder newPolylineBuilder = new PolylineBuilder(densifiedPolyline.SpatialReference);

                foreach (var part in densifiedPolyline.Parts)
                {
                    foreach (var point in part.Points)
                    {
                        double pointElevation = await _elevationSurface.GetElevationAsync(point);
                        MapPoint newMapPoint = new MapPoint(point.X, point.Y, pointElevation + ElevationOffset);
                        newPolylineBuilder.AddPoint(newMapPoint);
                    }
                }

                // Enable the view button once a pipe has been added to the graphics overlay.
                ViewReady = true;

                return newPolylineBuilder.ToGeometry();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
