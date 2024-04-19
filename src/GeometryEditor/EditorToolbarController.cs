using CommunityToolkit.Mvvm.ComponentModel;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Editing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorDemo
{
    internal partial class EditorToolbarController : ObservableObject
    {
        private readonly MyGeometryEditor editor = new MyGeometryEditor();
        private readonly GeometryEditor lineInputEditor = new GeometryEditor();
        private readonly GraphicsOverlay EditorOverlay = new GraphicsOverlay();

        public EditorToolbarController()
        {
            lineInputEditor.PropertyChanged += ReshapeEditor_PropertyChanged;
            editor.PropertyChanged += Editor_PropertyChanged;
        }
        private void ReshapeEditor_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GeometryEditor.Geometry))
                LineInputAcceptCommand.NotifyCanExecuteChanged();
        }

        private void Editor_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(GeometryEditor.CanUndo): UndoCommand.NotifyCanExecuteChanged(); break;
                case nameof(GeometryEditor.CanRedo): RedoCommand.NotifyCanExecuteChanged(); break;
                case nameof(MyGeometryEditor.Geometry):
                    {
                        ReshapeCommand.NotifyCanExecuteChanged();
                        CutCommand.NotifyCanExecuteChanged();
                        ApplyCommand.NotifyCanExecuteChanged();
                        break;
                    }
                case nameof(MyGeometryEditor.Tool):
                    {
                        ClearSelection();
                        RaiseActiveINPC();
                        break;
                    }
                case nameof(MyGeometryEditor.SelectedElement):
                    {
                        DeleteSelectionCommand.NotifyCanExecuteChanged();
                        ClearSelectionCommand.NotifyCanExecuteChanged();
                        break;
                    }
            }
        }
        string lineInputMode = "";

        private void RaiseActiveINPC()
        {
            OnPropertyChanged(nameof(IsMoveActive));
            OnPropertyChanged(nameof(IsRotateActive));
            OnPropertyChanged(nameof(IsEditVerticesActive));
            OnPropertyChanged(nameof(IsReshapeActive));
            OnPropertyChanged(nameof(IsCutActive));
            OnPropertyChanged(nameof(IsLineInputActive));
            OnPropertyChanged(nameof(IsEditingActive));
        }

        public bool IsMoveActive => GeometryEditor == editor && editor.IsMoveActive;
        public bool IsRotateActive => GeometryEditor == editor && editor.IsRotateActive;
        public bool IsEditVerticesActive => GeometryEditor == editor && editor.IsEditVerticesActive;
        public bool IsReshapeActive => IsLineInputActive && lineInputMode == "Reshape";
        public bool IsCutActive => IsLineInputActive && lineInputMode == "Cut";
        public bool IsLineInputActive => GeometryEditor == lineInputEditor;
        public bool IsEditingActive => GeometryEditor is not null && GeometryEditor.IsStarted;

        [ObservableProperty]
        private GeometryEditor? _geometryEditor;

        partial void OnGeometryEditorChanged(GeometryEditor? oldValue, GeometryEditor? newValue)
        {
            if(newValue != editor)
            {
                // Add current state of geometry to temporary overlay while using a different editor
                EditorOverlay.Graphics.Add(new Graphic(editor.Geometry, editor.Symbol) { IsSelected = true });
            }
            else
            {
                EditorOverlay.Graphics.Clear();
            }
            if (newValue == editor)
                editor.SetInactive();
            RaiseActiveINPC();
            CopySnapSettings(oldValue, newValue);
        }

        public void CopySnapSettings(GeometryEditor? source, GeometryEditor? target)
        {
            if (source is not null && target is not null)
            {
                target.SnapSettings = source.SnapSettings;
            }
        }

        private GraphicsOverlayCollection? _graphicsOverlays;

        public GraphicsOverlayCollection? GraphicsOverlays
        {
            get { return _graphicsOverlays; }
            set
            {
                var oldOverlays = _graphicsOverlays;
                if (_graphicsOverlays != value)
                {
                    if (oldOverlays is not null && oldOverlays.Contains(EditorOverlay))
                        oldOverlays.Remove(EditorOverlay);
                    _graphicsOverlays = value;
                    if (value is not null) // TODO: Perhaps do this delayed / on-demand instead
                        value.Add(EditorOverlay);
                }
            }
        }

        
        [ObservableProperty]
        private GeoElement? _geoElement;

        partial void OnGeoElementChanged(GeoElement? oldValue, GeoElement? newValue)
        {
            GeometryEditor = null;
            EditorOverlay.Graphics.Clear();
            editor.Stop();
            if (newValue is not null && !CanEditGeometry(newValue))
            {
                Trace.WriteLine("Geometry doesn't not support editing");
            }
            else
            {
                SetElementVisibility(true, oldValue);

                if (newValue?.Geometry is not null)
                {
                    Symbol? symbol = null;
                    if (GeoElement is Graphic g)
                    {
                        symbol = g.Symbol ?? GetGraphicsOwner(g)?.Renderer?.GetSymbol(g);
                    }
                    else if (GeoElement is Feature f)
                    {
                        symbol = (f.FeatureTable?.Layer as FeatureLayer)?.Renderer?.GetSymbol(f, true) ??
                            (f.FeatureTable as ArcGISFeatureTable)?.LayerInfo?.DrawingInfo?.Renderer?.GetSymbol(f, true);
                    }
                    SetElementVisibility(false, newValue);
                    editor.Initialize(newValue.Geometry, symbol);
                    editor.SetInactive();
                    GeometryEditor = editor;
                }
            }
            EditVerticesCommand.NotifyCanExecuteChanged();
            RotateCommand.NotifyCanExecuteChanged();
            MoveCommand.NotifyCanExecuteChanged();
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
            ReshapeCommand.NotifyCanExecuteChanged();
            CutCommand.NotifyCanExecuteChanged();
            LineInputAcceptCommand.NotifyCanExecuteChanged();
            DiscardCommand.NotifyCanExecuteChanged();
            ApplyCommand.NotifyCanExecuteChanged();
            DeleteSelectionCommand.NotifyCanExecuteChanged();
            ClearSelectionCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private bool isSettingsPanelVisible;

        [ObservableProperty]
        private bool isSnappingEnabled;

        partial void OnIsSnappingEnabledChanged(bool value)
        {
            SnapSourceSettings.Clear();

            if (GeometryEditor is not null)
            {
                if (value)
                    GeometryEditor.SnapSettings.SyncSourceSettings();

                GeometryEditor.SnapSettings.IsEnabled = value;

                foreach (var sourceSetting in GeometryEditor.SnapSettings.SourceSettings)
                {
                    SnapSourceSettings.Add(sourceSetting);
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<SnapSourceSettings> snapSourceSettings = [];

        private GraphicsOverlay? GetGraphicsOwner(Graphic g)
        {
            if (g is not null && GraphicsOverlays is not null)
            {
                foreach (var overlay in GraphicsOverlays)
                {
                    if (overlay == EditorOverlay) continue;
                    if (overlay.Graphics.Contains(g)) return overlay;
                }
            }
            return null;
        }

        public static bool CanEditGeometry(GeoElement? geoElement)
        {
            if (geoElement?.Geometry is null)
                return false;
            if (geoElement is Graphic)
                return true;
            if (geoElement is Feature feature && feature.FeatureTable != null)
            {
                return feature.FeatureTable.CanEditGeometry();
            }
            return false;
        }

        public event EventHandler<Esri.ArcGISRuntime.Geometry.Geometry>? EditingCompleted;

        public event EventHandler? EditingCancelled;
    }
}
