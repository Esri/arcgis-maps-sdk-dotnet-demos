using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Diagnostics;
using System.IO;
using UIKit;
using System.Linq;

namespace OfficeLocator.iOS
{
    /// <summary>
    /// The main view controller for office location and routing
    /// </summary>
	public partial class MainViewController : UIViewController
	{
		MapView _mapView;
		MapViewModel _mapViewModel;
		UITextField _fromTextField;
		UITableView _fromAutoCompleteTableView;
		UITextField _toTextField;
		UITableView _toAutoCompleteTableView;
		UIView _routeDetailsView;		
		UILabel _walkTimeTitle;
		UILabel _walkTime;
		UILabel _walkTimeAltTitle;
		UILabel _walkTimeAlt;
		UIView _busyView;
		UIActivityIndicatorView _busyIndicator;
		UILabel _busyLabel;

		public MainViewController() : base()
		{
		}

		public async override void ViewDidLoad()
		{
			base.ViewDidLoad();
			View.BackgroundColor = UIColor.White;

			try
			{
				var xOffset = 8;
				var yOffset = 28;
				var controlHeight = 30;
				var spacing = 6;
				var viewWidth = View.Bounds.Width;
				var viewHeight = View.Bounds.Height;

				// --- First Row - from location ---

				// Add marker image
				var markerImage = UIImage.FromFile("./MarkerA.png");
				var imageView = new UIImageView(markerImage)
				{
					Frame = new CGRect(xOffset, yOffset, controlHeight, controlHeight),
					ContentMode = UIViewContentMode.ScaleToFill
				};
				View.AddSubview(imageView);

				// Add from text field
				var textFieldOffset = xOffset + imageView.Frame.Width + spacing;
				var textFieldWidth = viewWidth - (textFieldOffset * 2);
				_fromTextField = new UITextField()
				{
					Frame = new CGRect(textFieldOffset, yOffset, textFieldWidth, controlHeight),
					Placeholder = "from office or conference room",
					BorderStyle = UITextBorderStyle.RoundedRect
				};
				_fromTextField.EditingChanged += FromTextField_EditingChanged;
				View.AddSubview(_fromTextField);

				// Add QR image
				var qrOffset = _fromTextField.Frame.Right + spacing;
				var qrImage = UIImage.FromFile("./QRScan_Black.png");
				imageView = new UIImageView(qrImage)
				{
					Frame = new CGRect(qrOffset, yOffset, controlHeight, controlHeight),
					ContentMode = UIViewContentMode.ScaleToFill
				};
				View.AddSubview(imageView);


				//// --- Second row - to location ---

				yOffset += controlHeight + spacing;

				// Add marker image
				markerImage = UIImage.FromFile("./MarkerB.png");
				imageView = new UIImageView(markerImage)
				{
					Frame = new CGRect(xOffset, yOffset, controlHeight, controlHeight),
					ContentMode = UIViewContentMode.ScaleToFill
				};
				View.AddSubview(imageView);

				// Add to text field
				_toTextField = new UITextField()
				{
					Frame = new CGRect(textFieldOffset, yOffset, textFieldWidth, controlHeight),
					Placeholder = "to office or conference room",
					BorderStyle = UITextBorderStyle.RoundedRect
				};
				_toTextField.EditingChanged += ToTextField_EditingChanged;
				View.AddSubview(_toTextField);

				// Add calendar image
				var calendarOffset = _toTextField.Frame.Right + spacing;
				var calendarImage = UIImage.FromBundle("Calendar.png");
				imageView = new UIImageView(calendarImage)
				{
					Frame = new CGRect(calendarOffset, yOffset, controlHeight, controlHeight),
					ContentMode = UIViewContentMode.ScaleToFill
				};
				View.AddSubview(imageView);



				// --- Third row - map view ---

				yOffset += controlHeight + spacing;

				// Initialize view-model
				_mapViewModel = new MapViewModel();
				await _mapViewModel.LoadAsync();
				_mapViewModel.RequestViewpoint += MapViewModel_RequestViewpoint;
				_mapViewModel.PropertyChanged += MapViewModel_PropertyChanged;

				// Initialize map view
				_mapView = new MapView()
				{
					Map = _mapViewModel.Map,
					GraphicsOverlays = _mapViewModel.Overlays,
					Frame = new CGRect(0, yOffset, viewWidth, viewHeight - yOffset)
				};
				_mapView.DrawStatusChanged += MapView_DrawStatusChanged;
				_mapView.NavigationCompleted += MapView_NavigationCompleted;
				View.AddSubview(_mapView);

				// --- Auto-complete drop-downs ---

				// Add table-view for auto-complete entries for "from" field
				_fromAutoCompleteTableView = new UITableView()
				{
					Frame = new CGRect(textFieldOffset, _fromTextField.Frame.Bottom, textFieldWidth, controlHeight),
					Hidden = true
				};
				View.AddSubview(_fromAutoCompleteTableView);

				// Add table-view for auto-complete entries for "to" field
				_toAutoCompleteTableView = new UITableView()
				{
					Frame = new CGRect(textFieldOffset, _toTextField.Frame.Bottom, textFieldWidth, controlHeight),
					Hidden = true
				};
				View.AddSubview(_toAutoCompleteTableView);


				// --- Walk-time UI ---

				controlHeight = 24;
				var routeDetailsHeight = (controlHeight * 2) + spacing + (xOffset * 2);
				_routeDetailsView = new UIView()
				{
					BackgroundColor = UIColor.White,
					Frame = new CGRect(0, viewHeight - routeDetailsHeight, viewWidth, routeDetailsHeight),
					Hidden = true
				};

				var titleWidth = 200;
				var fontSize = 22;
				var cfBlue = System.Drawing.Color.CornflowerBlue;
				var cfBlueUIColor = UIColor.FromRGB(cfBlue.R, cfBlue.G, cfBlue.B);
				_walkTimeTitle = new UILabel()
				{
					TextColor = cfBlueUIColor,
					Font = UIFont.BoldSystemFontOfSize(fontSize),
					Text = "Walk Time",
					Frame = new CGRect(xOffset, xOffset, titleWidth, controlHeight)
				};
				_routeDetailsView.AddSubview(_walkTimeTitle);

				var timeLabelOffset = _walkTimeTitle.Frame.Right;
				var timeLabelWidth = viewWidth - xOffset - timeLabelOffset;
				_walkTime = new UILabel()
				{
					TextColor = cfBlueUIColor,
					Font = UIFont.BoldSystemFontOfSize(fontSize),
					Text = "00:00:00",
					TextAlignment = UITextAlignment.Right,
					Frame = new CGRect(timeLabelOffset, xOffset, timeLabelWidth, controlHeight)
				};
				_routeDetailsView.AddSubview(_walkTime);

				var altFontSize = 18;
				var altYOffset = _walkTime.Frame.Bottom + spacing;
				_walkTimeAltTitle = new UILabel()
				{
					TextColor = UIColor.Gray,
					Font = UIFont.BoldSystemFontOfSize(altFontSize),
					Text = "Alternate Route",
					Frame = new CGRect(xOffset, altYOffset, titleWidth, controlHeight)
				};
				_routeDetailsView.AddSubview(_walkTimeAltTitle);

				_walkTimeAlt = new UILabel()
				{
					TextColor = UIColor.Gray,
					Font = UIFont.BoldSystemFontOfSize(altFontSize),
					Text = "00:00:00",
					TextAlignment = UITextAlignment.Right,
					Frame = new CGRect(timeLabelOffset, altYOffset, timeLabelWidth, controlHeight)
				};
				_routeDetailsView.AddSubview(_walkTimeAlt);

				View.AddSubview(_routeDetailsView);

				// --- Busy indicator view ---

				_busyView = new UIView()
				{
					BackgroundColor = UIColor.White,
					Frame = _routeDetailsView.Frame,
					Hidden = true
				};

				var busyIndicatorSize = 44;
				var busyIndicatorTop = (_busyView.Frame.Height - busyIndicatorSize) / 2;
				_busyIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
				{
					Color = UIColor.Gray,
					Frame = new CGRect(xOffset, busyIndicatorTop, busyIndicatorSize, busyIndicatorSize)
				};
				_busyIndicator.StartAnimating();
				_busyView.AddSubview(_busyIndicator);

				_busyLabel = new UILabel()
				{
					TextColor = cfBlueUIColor,
					Font = UIFont.BoldSystemFontOfSize(fontSize),
					Text = "Calculating...",
					Frame = new CGRect(busyIndicatorSize + xOffset, busyIndicatorTop, titleWidth, busyIndicatorSize)
				};
				_busyView.AddSubview(_busyLabel);

				View.AddSubview(_busyView);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"{ex.Message}\n{ex.StackTrace}");
			}
		}

		private void MapView_NavigationCompleted(object sender, EventArgs e)
		{
			if (_mapView.DrawStatus == DrawStatus.Completed && !_mapViewModel.IsBusy)
				_busyView.Hidden = true;
		}

		private void MapView_DrawStatusChanged(object sender, DrawStatus e)
		{
			if (e == DrawStatus.Completed && !_mapViewModel.IsBusy && !_mapView.IsNavigating)
				_busyView.Hidden = true;
		}

		private void MapViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(MapViewModel.HasRoute))
			{
				if (_mapViewModel.HasRoute)
				{
					_walkTime.Text = _mapViewModel.WalkTime;
					_walkTimeAlt.Text = _mapViewModel.WalkTimeAlt;
					_routeDetailsView.Hidden = false;
				}
				else
				{
					_routeDetailsView.Hidden = true;
				}
			}
			else if (e.PropertyName == nameof(MapViewModel.IsBusy))
			{
				if (_mapViewModel.IsBusy)
					_busyView.Hidden = false;
			}
		}

		private void MapViewModel_RequestViewpoint(object sender, Viewpoint viewpoint)
		{
			var envelope = viewpoint.TargetGeometry.Extent;
			if (envelope.Height > 0)
			{
				envelope = new Esri.ArcGISRuntime.Geometry.Envelope(envelope.XMin, envelope.YMin - envelope.Height * (64 / _mapView.Bounds.Height), 
					envelope.XMax, envelope.YMax, envelope.SpatialReference);
			}
			_mapView.SetViewpointGeometryAsync(envelope, 100);
		}

		private async void FromTextField_EditingChanged(object sender, EventArgs e)
		{
			var searchText = _fromTextField.Text;
			if (!string.IsNullOrEmpty(searchText))
			{
				var suggestions = await GeocodeHelper.SuggestAsync(searchText);
				var suggestionTableSource = new TableSource<string>(suggestions, (s) => s);
				suggestionTableSource.TableRowSelected += FromSuggestionTableSource_TableRowSelected;
				_fromAutoCompleteTableView.Source = suggestionTableSource;
				_fromAutoCompleteTableView.ReloadData();
				var oldFrame = _fromAutoCompleteTableView.Frame;
				_fromAutoCompleteTableView.Frame = new CGRect(oldFrame.Left, oldFrame.Top, oldFrame.Width, suggestions.Count() * 30f);
				_fromAutoCompleteTableView.Hidden = false;
			}
			else
			{
				_fromAutoCompleteTableView.Hidden = true;
			}
		}

		private async void ToTextField_EditingChanged(object sender, EventArgs e)
		{
			try
			{
				var searchText = _toTextField.Text;
				if (!string.IsNullOrEmpty(searchText))
				{
					var suggestions = await GeocodeHelper.SuggestAsync(searchText);
					var suggestionTableSource = new TableSource<string>(suggestions, (s) => s);
					suggestionTableSource.TableRowSelected += ToSuggestionTableSource_TableRowSelected;
					_toAutoCompleteTableView.Source = suggestionTableSource;
					_toAutoCompleteTableView.ReloadData();
					var oldFrame = _toAutoCompleteTableView.Frame;
					_toAutoCompleteTableView.Frame = new CGRect(oldFrame.Left, oldFrame.Top, oldFrame.Width, suggestions.Count() * 30f);
					_toAutoCompleteTableView.Hidden = false;
				}
				else
				{
					_toAutoCompleteTableView.Hidden = true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"{ex.Message}\n{ex.StackTrace}");
			}
		}

		private void FromSuggestionTableSource_TableRowSelected(object sender, TableRowSelectedEventArgs<string> e)
		{
			_fromAutoCompleteTableView.Hidden = true;
			_fromAutoCompleteTableView.DeselectRow(e.SelectedItemIndexPath, false);

			var fromLocation = e.SelectedItem;
			_fromTextField.Text = fromLocation;
			_fromTextField.ResignFirstResponder();
			_mapViewModel.GeocodeFromLocation(fromLocation);
		}

		private void ToSuggestionTableSource_TableRowSelected(object sender, TableRowSelectedEventArgs<string> e)
		{
			_toAutoCompleteTableView.Hidden = true;
			_toAutoCompleteTableView.DeselectRow(e.SelectedItemIndexPath, false);

			var toLocation = e.SelectedItem;
			_toTextField.Text = toLocation;
			_toTextField.ResignFirstResponder();
			_mapViewModel.GeocodeToLocation(toLocation);
		}

	}
}