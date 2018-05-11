using Esri.ArcGISRuntime.UI.Controls;
using System;
using Windows.UI.Xaml;


namespace OfflineWorkflowsSample.Infrastructure.ViewServices
{
    public abstract partial class GeoViewServiceBase<TGeoView>
        where TGeoView : GeoView
    {
        private WeakReference<TGeoView> _geoView;

        protected TGeoView View
        {
            get
            {
                if (_geoView != null && _geoView.TryGetTarget(out TGeoView geoView))
                    return geoView;
                return null;
            }
        }

        public static GeoViewServiceBase<TGeoView> GetViewService(DependencyObject obj)
        {
            return (GeoViewServiceBase<TGeoView>)obj.GetValue(ViewServiceProperty);
        }

        public static void SetViewService(DependencyObject obj, GeoViewServiceBase<TGeoView> value)
        {
            obj.SetValue(ViewServiceProperty, value);
        }

        public static readonly DependencyProperty ViewServiceProperty =
            DependencyProperty.RegisterAttached("ViewService", 
                typeof(GeoViewServiceBase<TGeoView>), 
                typeof(GeoViewServiceBase<TGeoView>), 
                new PropertyMetadata(null, OnGeoViewPropertyChanged));

        private static void OnGeoViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TGeoView && e.OldValue is GeoViewServiceBase<TGeoView>)
            {
                var viewService = (e.OldValue as GeoViewServiceBase<TGeoView>);
                var oldGeoView = d as GeoView;
                // Unregister all events from used service to release it for collection
                viewService.UnregisterGeoViewEvents(oldGeoView as TGeoView);
                viewService._geoView = null;
            }
            if (d is TGeoView && e.NewValue is GeoViewServiceBase<TGeoView>)
            {
                var viewService = (e.NewValue as GeoViewServiceBase<TGeoView>);
                var currentView = d as GeoView;

                // Set reference as a weak reference
                viewService._geoView = new WeakReference<TGeoView>(currentView as TGeoView);

                // Register events, use can be used to register events when the geoview is changed
                viewService.RegisterGeoViewEvents((TGeoView)currentView);
            }
        }

        /// <summary>
        /// Override this to remove type specific events from the <see cref="GeoView"/>
        /// </summary>
        /// <remarks>Remember to use weakevents!</remarks>
        /// <param name="geoView">Geoview to that has attached events.</param>
        protected virtual void UnregisterGeoViewEvents(TGeoView geoView)
        {
            // Template method.
        }

        /// <summary>
        /// Override this to add type specific events to the <see cref="GeoView"/>
        /// </summary>
        /// <remarks>Remember to use weakevents!</remarks>
        /// <param name="newGeoView">New geoview to attach the events.</param>
        protected virtual void RegisterGeoViewEvents(TGeoView newGeoView)
        {
            // Template method.
        }
    }
}