using System.Diagnostics;
using System.Drawing;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Editing;
using WinRT;

namespace EditorDemo
{
    public class MyGeometryEditor : GeometryEditor
    {
        #region Tool Configurations
        private VertexTool _moveTool = new VertexTool()
        {
            Configuration = new InteractionConfiguration()
            {
                //AllowMovingSelectedElement = true,
                AllowMidVertexSelection = false,
                AllowDeletingSelectedElement = false,
                AllowPartCreation = false,
                AllowGeometrySelection = true,
                AllowPartSelection = true,
                AllowScalingSelectedElement = false,
                AllowVertexCreation = false,
                AllowVertexSelection = false,
                AllowRotatingSelectedElement = false,
                RequireSelectionBeforeMove = false
            },
            Style = new GeometryEditorStyle
            {
                MidVertexSymbol = null,
                VertexSymbol = null,
                SelectedVertexSymbol = null,
                SelectedMidVertexSymbol = null,
                FeedbackVertexSymbol = null,
                VertexTextSymbol = null
            }
        };
        private VertexTool _rotateTool = new VertexTool()
        {
            Configuration = new InteractionConfiguration()
            {
                AllowMovingSelectedElement = false,
                AllowMidVertexSelection = false,
                AllowDeletingSelectedElement = false,
                AllowPartCreation = false,
                AllowGeometrySelection = true,
                AllowPartSelection = true,
                AllowScalingSelectedElement = false,
                AllowVertexCreation = false,
                AllowVertexSelection = false,
                AllowRotatingSelectedElement = true,
                RequireSelectionBeforeMove = false
            },
            Style = new GeometryEditorStyle
            {
                MidVertexSymbol = null,
                VertexSymbol = null,
                SelectedVertexSymbol = null,
                SelectedMidVertexSymbol = null,
                FeedbackVertexSymbol = null
            }
        };
        private VertexTool _vertexTool = new VertexTool()
        {
            Configuration = new InteractionConfiguration()
            {
                AllowMovingSelectedElement = true,
                AllowMidVertexSelection = true,
                AllowDeletingSelectedElement = true,
                AllowPartCreation = false,
                AllowGeometrySelection = false,
                AllowPartSelection = false,
                AllowScalingSelectedElement = false,
                AllowVertexCreation = true,
                AllowVertexSelection = true,
                AllowRotatingSelectedElement = false,
                RequireSelectionBeforeMove = false
            },
            Style = new GeometryEditorStyle
            {
                VertexTextSymbol = null,
                VertexSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, System.Drawing.Color.Black, 9),
                SelectedVertexSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, System.Drawing.Color.Black, 9),
                MidVertexSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Gray, 8),
                SelectedMidVertexSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Gray, 8),
                FeedbackVertexSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Black, 10),
            }
        };
        private VertexTool _inactiveTool = new VertexTool()
        {
            Configuration = new InteractionConfiguration()
            {
                AllowMovingSelectedElement = false,
                AllowMidVertexSelection = false,
                AllowDeletingSelectedElement = true,
                AllowPartCreation = false,
                AllowGeometrySelection = false,
                AllowPartSelection = true,
                AllowScalingSelectedElement = false,
                AllowVertexCreation = false,
                AllowVertexSelection = false,
                AllowRotatingSelectedElement = false,
                RequireSelectionBeforeMove = false
            },
            Style = new GeometryEditorStyle()
            {
                MidVertexSymbol = null,
                VertexSymbol = null,
                SelectedVertexSymbol = null,
                SelectedMidVertexSymbol = null,
                FeedbackVertexSymbol = null, 
                BoundingBoxSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Cyan, 2),
                VertexTextSymbol = null
            }
        };
        #endregion

        public bool IsMoveActive => IsStarted && Tool == _moveTool;
        public bool IsRotateActive => IsStarted && Tool == _rotateTool;
        public bool IsEditVerticesActive => IsStarted && Tool == _vertexTool;

        public MyGeometryEditor()
        {
            PropertyChanged += OnPropertyChanged;
        }

        private Geometry? _geometry;

        public Symbol? Symbol { get; private set; }

        public void SetInactive()
        {
            Tool = _inactiveTool;
        }

        public void Initialize(Geometry geometry, Symbol? symbol)
        {
            _geometry = geometry;
            Symbol = symbol;
            if (symbol is not null)
            {
                if (geometry is Polygon)
                {
                    _vertexTool.Style.FillSymbol = _rotateTool.Style.FillSymbol = _moveTool.Style.FillSymbol = _inactiveTool.Style.FillSymbol = symbol;
                    _inactiveTool.Style.SelectedVertexSymbol = _inactiveTool.Style.VertexSymbol = _moveTool.Style.SelectedVertexSymbol = _moveTool.Style.VertexSymbol = null;
                }
                else if (geometry is Polyline)
                {
                    _vertexTool.Style.LineSymbol = _rotateTool.Style.LineSymbol = _moveTool.Style.LineSymbol = symbol;
                    _inactiveTool.Style.LineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Cyan, 2);
                    _inactiveTool.Style.SelectedVertexSymbol = _inactiveTool.Style.VertexSymbol = _moveTool.Style.SelectedVertexSymbol = _moveTool.Style.VertexSymbol = null;
                }
                else if (geometry is MapPoint || geometry is Multipoint)
                {
                    _inactiveTool.Style.VertexSymbol = _inactiveTool.Style.SelectedVertexSymbol = _moveTool.Style.SelectedVertexSymbol = _moveTool.Style.VertexSymbol = symbol;
                }
            }
            Start(geometry);
            Tool = _inactiveTool;
        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsStarted):
                    break;
                case nameof(SelectedElement):
                    break;
                case nameof(Tool):
                    break;
                case nameof(Geometry):
                    break;
            }
        }

        private void EnsureStarted()
        {
            Debug.Assert(_geometry != null, "Geometry not set");
            if (!IsStarted)
                Start(_geometry);
        }
        public void Move()
        {
            Tool = _moveTool;
            EnsureStarted();
            SelectGeometry();
        }
        public void EditVertices()
        {
            Tool = _vertexTool;
            EnsureStarted();
            ClearSelection();
        }
        public void Rotate()
        {
            Tool = _rotateTool;
            EnsureStarted();
            SelectGeometry();
        }
    }
}
