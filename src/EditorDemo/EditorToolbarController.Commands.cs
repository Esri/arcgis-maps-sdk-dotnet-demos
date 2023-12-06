using CommunityToolkit.Mvvm.Input;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorDemo
{
    internal partial class EditorToolbarController
    {
        private bool CanEditVertices => CanEditGeometry(GeoElement) && (GeoElement?.Geometry is Polygon || (GeoElement?.Geometry is Polyline));

        [RelayCommand(CanExecute = nameof(CanEditVertices))]
        private void EditVertices()
        {
            GeometryEditor = editor;
            editor.EditVertices();
        }
        private bool CanMove => CanEditGeometry(GeoElement) == true;

        [RelayCommand(CanExecute = nameof(CanMove))]
        private void Move()
        {
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

        private bool CanReshape => !reshapeEditor.IsStarted && editor.Geometry?.IsEmpty == false && editor.Geometry is Polygon; //TODO

        [RelayCommand(CanExecute = nameof(CanReshape))]
        private void Reshape()
        {
            GeometryEditor = reshapeEditor;
            reshapeEditor.Start(GeometryType.Polyline);
            ReshapeCommand.NotifyCanExecuteChanged();
            ReshapeAcceptCommand.NotifyCanExecuteChanged();
            ReshapeDiscardCommand.NotifyCanExecuteChanged();
        }

        private bool CanAcceptReshape => reshapeEditor.IsStarted &&
            this.editor.Geometry is Multipart mp && !mp.IsEmpty && reshapeEditor.Geometry is Polyline line && !line.IsEmpty &&
            GeometryEngine.Reshape(mp, line) != null;

        [RelayCommand(CanExecute = nameof(CanAcceptReshape))]
        private void ReshapeAccept()
        {
            var geometry = reshapeEditor.Stop();
            EditorOverlay.Graphics.Clear();
            if (editor.Geometry is Multipart mp && geometry is Polyline line && !mp.IsEmpty && !line.IsEmpty)
            {
                var reshapedGeometry = GeometryEngine.Reshape(mp, line);
                if (reshapedGeometry != null)
                    editor.ReplaceGeometry(reshapedGeometry);
            }
            GeometryEditor = editor;
            ReshapeCommand.NotifyCanExecuteChanged();
            ReshapeAcceptCommand.NotifyCanExecuteChanged();
            ReshapeDiscardCommand.NotifyCanExecuteChanged();
        }

        private bool CanReshapeDiscard => reshapeEditor.IsStarted;

        [RelayCommand(CanExecute = nameof(CanReshapeDiscard))]
        private void ReshapeDiscard()
        {
            reshapeEditor.Stop();
            EditorOverlay.Graphics.Clear();
            GeometryEditor = editor;
            ReshapeCommand.NotifyCanExecuteChanged();
            ReshapeAcceptCommand.NotifyCanExecuteChanged();
            ReshapeDiscardCommand.NotifyCanExecuteChanged();
        }

        private bool CanApply => CanEditGeometry(GeoElement) && editor.IsStarted && GeoElement != null &&(editor.Geometry?.IsEmpty ?? true) == false;

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
    }
}
