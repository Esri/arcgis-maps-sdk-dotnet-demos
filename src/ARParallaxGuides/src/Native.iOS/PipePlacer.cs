using System;
using System.Linq;
using ARParallaxGuidelines.Shared;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ARParallaxGuidelines
{
    [Register("ViewHiddenInfrastructureAR")]
    public class PipePlacer : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _mapView;
        private UILabel _helpLabel;
        private UIBarButtonItem _addButton;
        private UIBarButtonItem _undoButton;
        private UIBarButtonItem _redoButton;
        private UIBarButtonItem _viewButton;
        private UIBarButtonItem _doneButton;
        private UIBarButtonItem _elevationSliderButton;
        private UISlider _elevationSlider;

        private InfrastructureEditorViewModel _editorVM = new InfrastructureEditorViewModel();

        // Graphics overlays for showing pipes.
        private GraphicsOverlay _pipesOverlay = new GraphicsOverlay();

        private async void Initialize()
        {
            // Create and add the map.
            _mapView.Map = new Map(Basemap.CreateImagery());

            // Configure location display.
            _mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            await _mapView.LocationDisplay.DataSource.StartAsync();
            _mapView.LocationDisplay.IsEnabled = true;

            // Add a graphics overlay for the drawn pipes.
            _mapView.GraphicsOverlays.Add(_pipesOverlay);
            _pipesOverlay.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2));

            await _editorVM.InitializeAsync();

            // Set the SketchEditor for the map.
            _mapView.SketchEditor = _editorVM.SketchEditor;

            // Enable the add button.
            _addButton.Enabled = true;
        }

        private void DoneButton_Clicked(object sender, EventArgs e) => _editorVM.ExecuteCompleteCommand();

        private void ViewButton_Clicked(object sender, EventArgs e)
        {
            // Transition to the view the pipes in augmented reality.
            NavigationController.PopViewController(true);
            NavigationController.PushViewController(new PipeViewerAR() { _pipeGraphics = _pipesOverlay.Graphics.Select(x => new Graphic(x.Geometry, x.Attributes)) }, true);
        }

        private void RedoButton_Clicked(object sender, EventArgs e) => _editorVM.ExecuteRedoCommand();

        private void UndoButton_Clicked(object sender, EventArgs e) => _editorVM.ExecuteUndoCommand();

        private async void AddSketch(object sender, EventArgs e)
        {
            var geometry = await _editorVM.ExecuteAddSketch();

            if (geometry != null)
            {
                var graphic = new Graphic(geometry);
                graphic.Attributes[nameof(_editorVM.ElevationOffset)] = _editorVM.ElevationOffset;
                _pipesOverlay.Graphics.Add(graphic);
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _mapView = new MapView();
            _mapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _helpLabel = new UILabel();
            _helpLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _helpLabel.TextAlignment = UITextAlignment.Center;
            _helpLabel.TextColor = UIColor.White;
            _helpLabel.BackgroundColor = UIColor.FromWhiteAlpha(0f, 0.6f);
            _helpLabel.Text = "Preparing services...";

            UIToolbar elevToolbar = new UIToolbar();
            elevToolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            UIBarButtonItem elevLabel = new UIBarButtonItem() { CustomView = new UILabel() { Text = "Elevation:" } };

            _elevationSlider = new UISlider() { MinValue = -10, MaxValue = 10, Value = 0 };
            _elevationSliderButton = new UIBarButtonItem() { CustomView = _elevationSlider };

            elevToolbar.Items = new[]
            {
                 elevLabel,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _elevationSliderButton
            };

            UIToolbar buttonToolbar = new UIToolbar();
            buttonToolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, AddSketch) { Enabled = false };
            _undoButton = new UIBarButtonItem(UIBarButtonSystemItem.Undo, UndoButton_Clicked) { Enabled = false };
            _redoButton = new UIBarButtonItem(UIBarButtonSystemItem.Redo, RedoButton_Clicked) { Enabled = false };
            _doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, DoneButton_Clicked) { Enabled = false };
            _viewButton = new UIBarButtonItem(UIBarButtonSystemItem.Camera, ViewButton_Clicked) { Enabled = false };

            buttonToolbar.Items = new[]
            {
                 _addButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _undoButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _redoButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _doneButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _viewButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            // Add the views.
            View.AddSubviews(_mapView, _helpLabel, elevToolbar, buttonToolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _mapView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _mapView.BottomAnchor.ConstraintEqualTo(elevToolbar.TopAnchor),
                _mapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.HeightAnchor.ConstraintEqualTo(40),
                elevToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                elevToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                elevToolbar.BottomAnchor.ConstraintEqualTo(buttonToolbar.TopAnchor),
                buttonToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                buttonToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                buttonToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        private void _elevationSlider_ValueChanged(object sender, EventArgs e)
        {
            _editorVM.ElevationOffset = _elevationSlider.Value;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (_elevationSlider != null)
                _elevationSlider.ValueChanged += _elevationSlider_ValueChanged;
            if (_editorVM != null)
                _editorVM.PropertyChanged += _editorVM_PropertyChanged;
        }

        private void _editorVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_editorVM.AddButtonEnabled):
                    _addButton.Enabled = _editorVM.AddButtonEnabled;
                    break;
                case nameof(_editorVM.DoneButtonEnabled):
                    _doneButton.Enabled = _editorVM.DoneButtonEnabled;
                    break;
                case nameof(_editorVM.RedoButtonEnabled):
                    _redoButton.Enabled = _editorVM.RedoButtonEnabled;
                    break;
                case nameof(_editorVM.UndoButtonEnabled):
                    _undoButton.Enabled = _editorVM.UndoButtonEnabled;
                    break;
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (_elevationSlider != null)
                _elevationSlider.ValueChanged -= _elevationSlider_ValueChanged;
            if (_editorVM != null)
                _editorVM.PropertyChanged -= _editorVM_PropertyChanged;
        }
    }
}