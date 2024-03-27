using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
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
    internal partial class EditorToolbarController
    {
        private bool CanClearSelection => CanEditGeometry(GeoElement) && GeometryEditor == editor && editor.SelectedElement != null;

        [RelayCommand(CanExecute = nameof(CanClearSelection))]
        private void ClearSelection()
        {
            GeometryEditor = editor;
            editor.ClearSelection();
        }

        private bool CanDeleteSelection => CanEditGeometry(GeoElement) && GeometryEditor == editor && editor.SelectedElement?.CanDelete == true &&
            (editor.SelectedElement is not Esri.ArcGISRuntime.UI.Editing.GeometryEditorGeometry || editor.SelectedElement is not Esri.ArcGISRuntime.UI.Editing.GeometryEditorMidVertex);

        [RelayCommand(CanExecute = nameof(CanDeleteSelection))]
        private void DeleteSelection()
        {
            GeometryEditor = editor;
            editor.DeleteSelectedElement();
        }

        private bool CanEditVertices => CanEditGeometry(GeoElement) && (GeoElement?.Geometry is Polygon || (GeoElement?.Geometry is Polyline));

        [RelayCommand(CanExecute = nameof(CanEditVertices))]
        private void EditVertices()
        {
            if (GeometryEditor == editor && editor.IsEditVerticesActive)
            {
                editor.SetInactive();
                return;
            }
            GeometryEditor = editor;
            editor.EditVertices();
        }
        private bool CanMove => CanEditGeometry(GeoElement) == true;

        [RelayCommand(CanExecute = nameof(CanMove))]
        private void Move()
        {
            if (GeometryEditor == editor && editor.IsMoveActive)
            {
                editor.SetInactive();
                return;
            }
            GeometryEditor = editor;
            editor.Move();
        }

        private bool CanUndo() => editor.CanUndo;
        [RelayCommand(CanExecute = nameof(CanUndo))]
        private void Undo() => editor.Undo();

        private bool CanRedo() => editor.CanRedo;
        [RelayCommand(CanExecute = nameof(CanRedo))]
        private void Redo() => editor.Redo();

        [RelayCommand(CanExecute = nameof(CanEditVertices))]
        private void Rotate()
        {
            if (GeometryEditor == editor && editor.IsRotateActive)
            {
                editor.SetInactive();
                return;
            }
            GeometryEditor = editor;
            editor.Rotate();
        }

        private static void SetElementVisibility(bool visible, GeoElement? element)
        {
            if (element is Graphic g)
                g.IsVisible = visible;
            else if (element is Feature f && f.FeatureTable?.Layer is FeatureLayer l)
            {
                l.SetFeatureVisible(f, visible);
            }
        }

        private bool CanUseLineInput => !lineInputEditor.IsStarted && editor.Geometry?.IsEmpty == false && editor.Geometry is Polygon; //TODO

        [RelayCommand(CanExecute = nameof(CanUseLineInput))]
        private void Reshape()
        {
            GeometryEditor = lineInputEditor;
            lineInputMode = nameof(Reshape);
            lineInputEditor.Start(GeometryType.Polyline);
            ReshapeCommand.NotifyCanExecuteChanged();
            LineInputAcceptCommand.NotifyCanExecuteChanged();
            LineInputDiscardCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanUseLineInput))]
        private void Cut()
        {
            GeometryEditor = lineInputEditor;
            lineInputMode = nameof(Cut);
            lineInputEditor.Start(GeometryType.Polyline);
            CutCommand.NotifyCanExecuteChanged();
            LineInputAcceptCommand.NotifyCanExecuteChanged();
            LineInputDiscardCommand.NotifyCanExecuteChanged();
        }

        private bool CanAcceptLineInput => lineInputEditor.IsStarted &&
            this.editor.Geometry is Multipart mp && !mp.IsEmpty && lineInputEditor.Geometry is Polyline line && !line.IsEmpty &&
            ((lineInputMode == nameof(Reshape) && GeometryEngine.Reshape(mp, line) != null) ||
             (lineInputMode == nameof(Cut) && GeometryEngine.Cut(mp, line).Length > 0));

        [RelayCommand(CanExecute = nameof(CanAcceptLineInput))]
        private void LineInputAccept()
        {
            var geometry = lineInputEditor.Stop();
            EditorOverlay.Graphics.Clear();
            if (editor.Geometry is Multipart mp && geometry is Polyline line && !mp.IsEmpty && !line.IsEmpty)
            {
                if (lineInputMode == nameof(Reshape))
                {
                    var reshapedGeometry = GeometryEngine.Reshape(mp, line);
                    if (reshapedGeometry != null)
                        editor.ReplaceGeometry(reshapedGeometry);
                }
                if (lineInputMode == nameof(Cut))
                {
                    var cutGeometry = GeometryEngine.Cut(mp, line).Where(g => g.GeometryType == editor.Geometry.GeometryType).ToArray();
                    if (cutGeometry != null && cutGeometry.Length > 0)
                    {
                        if (cutGeometry.Length == 1)
                        {
                            editor.ReplaceGeometry(cutGeometry[0]);
                        }
                        else
                        {
                            if (editor.Geometry is Polygon p)
                            {
                                var b = new PolygonBuilder(p.SpatialReference);
                                foreach (var piece in cutGeometry.OfType<Polygon>())
                                {
                                    b.AddParts(piece.Parts);
                                }
                                editor.ReplaceGeometry(b.ToGeometry());
                            }
                            else if (editor.Geometry is Polyline l)
                            {
                                var b = new PolylineBuilder(l.SpatialReference);
                                foreach (var piece in cutGeometry.OfType<Polyline>())
                                {
                                    b.AddParts(piece.Parts);
                                }
                                editor.ReplaceGeometry(b.ToGeometry());
                            }
                            else
                                editor.ReplaceGeometry(cutGeometry.First());
                        }
                    }
                }
            }
            GeometryEditor = editor;
            ReshapeCommand.NotifyCanExecuteChanged();
            CutCommand.NotifyCanExecuteChanged();
            LineInputAcceptCommand.NotifyCanExecuteChanged();
            LineInputDiscardCommand.NotifyCanExecuteChanged();
            LineInputEditorToggleSnappingCommand.NotifyCanExecuteChanged();
        }

        private bool CanReshapeDiscard => lineInputEditor.IsStarted;

        [RelayCommand(CanExecute = nameof(CanReshapeDiscard))]
        private void LineInputDiscard()
        {
            lineInputEditor.Stop();
            EditorOverlay.Graphics.Clear();
            GeometryEditor = editor;
            ReshapeCommand.NotifyCanExecuteChanged();
            CutCommand.NotifyCanExecuteChanged();
            LineInputAcceptCommand.NotifyCanExecuteChanged();
            LineInputDiscardCommand.NotifyCanExecuteChanged();
            LineInputEditorToggleSnappingCommand.NotifyCanExecuteChanged();
        }

        private bool CanApply => CanEditGeometry(GeoElement) && editor.IsStarted && GeoElement != null && (editor.Geometry?.IsEmpty ?? true) == false;

        [RelayCommand(CanExecute = nameof(CanApply))]
        private void Apply()
        {
            Debug.Assert(GeoElement != null);
            var geometry = editor.Stop();
            if (geometry != null)
                EditingCompleted?.Invoke(this, geometry);
        }

        private bool CanDiscard => GeoElement != null;

        [RelayCommand(CanExecute = nameof(CanDiscard))]
        private void Discard() => EditingCancelled?.Invoke(this, EventArgs.Empty);

        [RelayCommand(CanExecute = nameof(CanMove))]
        private void EditorToggleSnapping()
        {
            if (IsEditorSnappingEnabled)
            {
                editor.SnapSettings.IsEnabled = false;
                EditorSnapSourceSettings.Clear();
                return;
            }

            if (GeoElement is Feature feature && feature.FeatureTable?.Layer is FeatureLayer layer)
            {
                editor.SnapSettings.SyncSourceSettings();
                editor.SnapSettings.IsEnabled = true;

                foreach (var sourceSetting in editor.SnapSettings.SourceSettings)
                {
                    EditorSnapSourceSettings.Add(sourceSetting);
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanDiscard))]
        private void LineInputEditorToggleSnapping()
        {
            if (IsLineInputEditorSnappingEnabled)
            {
                lineInputEditor.SnapSettings.IsEnabled = false;
                LineInputEditorSnapSourceSettings.Clear();
                return;
            }
            if (GeoElement is Feature feature && feature.FeatureTable?.Layer is FeatureLayer layer)
            {
                lineInputEditor.SnapSettings.SyncSourceSettings();
                lineInputEditor.SnapSettings.IsEnabled = true;

                foreach (var sourceSetting in lineInputEditor.SnapSettings.SourceSettings)
                {
                    LineInputEditorSnapSourceSettings.Add(sourceSetting);
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<SnapSourceSettings> editorSnapSourceSettings = [];

        [ObservableProperty]
        private ObservableCollection<SnapSourceSettings> lineInputEditorSnapSourceSettings = [];
    }
}
