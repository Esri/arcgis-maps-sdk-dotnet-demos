using Esri.ArcGISRuntime.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
#if XAMARIN
using Esri.ArcGISRuntime.Xamarin.Forms;
using Xamarin.Forms;
#elif WINDOWS_UWP
using Esri.ArcGISRuntime.UI.Controls;
using Windows.UI.Xaml;
#elif WINDOWS_WPF
using Esri.ArcGISRuntime.UI.Controls;
using System.Windows;
#endif

namespace RoutingSample
{
    public class RestoreAutoPanMode
    {
        private MapView _mapView;
        private CancellationTokenSource _delayTokenSource;
        private CancellationToken _delayToken;

        private void AttachToMapView(MapView mv)
        {
            if (_mapView != null && _mapView != mv)
                throw new InvalidOperationException("RestoreAutoPanMode can only be assigned to one MapView.");

            _mapView = mv;
            _mapView.NavigationCompleted += OnMapViewNavigationCompleted;
        }

        private void DetachFromMapView(MapView mv)
        {
            if (_mapView != null && _mapView == mv)
            {
                _mapView.NavigationCompleted -= OnMapViewNavigationCompleted;
                _mapView = null;
            }
        }

        private async void OnMapViewNavigationCompleted(object sender, EventArgs e)
        {
            // If user stopped navigating and we're not in the correct autopan mode,
            // restore autopan after the set delay.
            if (IsEnabled && !_mapView.IsNavigating)
            {
                if (_mapView.LocationDisplay != null && _mapView.LocationDisplay.AutoPanMode != PanMode)
                {
                    if (_delayTokenSource != null)
                    {
                        if (_delayToken.CanBeCanceled)
                            _delayTokenSource.Cancel();

                        _delayTokenSource.Dispose();
                    }

                    _delayTokenSource = new CancellationTokenSource();
                    _delayToken = _delayTokenSource.Token;

                    try
                    {
                        await WaitAndResetPanMode();
                    }
                    catch (TaskCanceledException)
                    { }
                }
            }
        }

        private async Task WaitAndResetPanMode()
        {
            await Task.Delay(DelayInSeconds * 1000, _delayToken);
            if (!_delayToken.IsCancellationRequested)
                ResetPanMode();
        }

        private void ResetPanMode()
        {
            if (_mapView != null && _mapView.LocationDisplay != null)
                _mapView.LocationDisplay.AutoPanMode = PanMode;
        }

        /// <summary>
        /// Gets or sets whether the property is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the delay in seconds.
        /// </summary>
        public int DelayInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the pan mode.
        /// </summary>
        public LocationDisplayAutoPanMode PanMode { get; set; }

#if XAMARIN
        public static BindableProperty RestoreAutoPanSettingsProperty =
            BindableProperty.CreateAttached(
                "RestoreAutoPanSettings",
                typeof(RestoreAutoPanMode),
                typeof(RestoreAutoPanMode),
                null,
                propertyChanged: OnRestoreAutoPanSettingsChanged
            );
#else
        public static readonly DependencyProperty RestoreAutoPanSettingsProperty =
            DependencyProperty.RegisterAttached(
                "RestoreAutoPanSettings",
                typeof(RestoreAutoPanMode),
                typeof(RestoreAutoPanMode),
                new PropertyMetadata(null, OnRestoreAutoPanSettingsChanged)
            );
#endif

#if XAMARIN
        public static RestoreAutoPanMode GetRestoreAutoPanSettings(BindableObject obj)
#else
        public static RestoreAutoPanMode GetRestoreAutoPanSettings(DependencyObject obj)
#endif
        {
            return (RestoreAutoPanMode)obj.GetValue(RestoreAutoPanSettingsProperty);
        }

#if XAMARIN
        public static void SetRestoreAutoPanSettings(BindableObject obj, RestoreAutoPanMode value)
#else
        public static void SetRestoreAutoPanSettings(DependencyObject obj, RestoreAutoPanMode value)
#endif
        {
            obj.SetValue(RestoreAutoPanSettingsProperty, value);
        }

#if XAMARIN
        private static void OnRestoreAutoPanSettingsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MapView mv)
            {
                if (oldValue is RestoreAutoPanMode restoreOld)
                {
                    restoreOld.DetachFromMapView(mv);
                }

                if (newValue is RestoreAutoPanMode restoreNew)
                {
                    restoreNew.AttachToMapView(mv);
                }
            }
            else
            {
                throw new InvalidOperationException("This property must be attached to a MapView.");
            }
        }
#else
        private static void OnRestoreAutoPanSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MapView mv)
            {
                if (e.OldValue is RestoreAutoPanMode oldValue)
                {
                    oldValue.DetachFromMapView(mv);
                }

                if (e.NewValue is RestoreAutoPanMode newValue)
                {
                    newValue.AttachToMapView(mv);
                }
            }
            else
            {
                throw new InvalidOperationException("This property must be attached to a MapView.");
            }
        }
#endif
    }
}
