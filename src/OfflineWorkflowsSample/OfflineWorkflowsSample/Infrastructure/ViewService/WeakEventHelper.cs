using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace OfflineWorkflowsSample.Infrastructure
{
    public static class WeakEventHelper
    {
        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterNotifyPropertyChanged(INotifyPropertyChanged sender, PropertyChangedEventHandler handler)
        {
            var listener = new WeakEventListener<INotifyPropertyChanged, object, PropertyChangedEventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.PropertyChanged -= weakEventListener.OnEvent
            };
            sender.PropertyChanged += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterNotifyCollectionChanged(INotifyCollectionChanged sender, NotifyCollectionChangedEventHandler handler)
        {
            var listener = new WeakEventListener<INotifyCollectionChanged, object, NotifyCollectionChangedEventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.CollectionChanged -= weakEventListener.OnEvent
            };
            sender.CollectionChanged += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterLoadable(ILoadable sender, EventHandler handler)
        {
            var listener = new WeakEventListener<ILoadable, object, EventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.Loaded -= weakEventListener.OnEvent
            };
            sender.Loaded += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterLayerViewStateChanged(GeoView sender, EventHandler<LayerViewStateChangedEventArgs> handler)
        {
            var listener = new WeakEventListener<GeoView, object, LayerViewStateChangedEventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.LayerViewStateChanged -= weakEventListener.OnEvent
            };
            sender.LayerViewStateChanged += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterDrawStatusChanged(GeoView sender, EventHandler<DrawStatusChangedEventArgs> handler)
        {
            var listener = new WeakEventListener<GeoView, object, DrawStatusChangedEventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.DrawStatusChanged -= weakEventListener.OnEvent
            };
            sender.DrawStatusChanged += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterSpatialReferenceChanged(GeoView sender, EventHandler<EventArgs> handler)
        {
            var listener = new WeakEventListener<GeoView, object, EventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.SpatialReferenceChanged -= weakEventListener.OnEvent
            };
            sender.SpatialReferenceChanged += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterNavigationCompleted(GeoView sender, EventHandler<EventArgs> handler)
        {
            var listener = new WeakEventListener<GeoView, object, EventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.NavigationCompleted -= weakEventListener.OnEvent
            };
            sender.NavigationCompleted += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterViewpointChanged(GeoView sender, EventHandler<EventArgs> handler)
        {
            var listener = new WeakEventListener<GeoView, object, EventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.ViewpointChanged -= weakEventListener.OnEvent
            };
            sender.ViewpointChanged += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterGeoViewTapped(GeoView sender, EventHandler<GeoViewInputEventArgs> handler)
        {
            var listener = new WeakEventListener<GeoView, object, GeoViewInputEventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.GeoViewTapped -= weakEventListener.OnEvent
            };
            sender.GeoViewTapped += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterGeoViewDoubleTapped(GeoView sender, EventHandler<GeoViewInputEventArgs> handler)
        {
            var listener = new WeakEventListener<GeoView, object, GeoViewInputEventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.GeoViewDoubleTapped -= weakEventListener.OnEvent
            };
            sender.GeoViewDoubleTapped += listener.OnEvent;
        }

        /// <summary>
        /// Register given handler to given event as a weak reference.
        /// </summary>
        /// <param name="sender">Object to register.</param>
        /// <param name="handler">Handler to register.</param>
        public static void RegisterGeoViewHolding(GeoView sender, EventHandler<GeoViewInputEventArgs> handler)
        {
            var listener = new WeakEventListener<GeoView, object, GeoViewInputEventArgs>(sender)
            {
                OnEventAction = (instance, source, eventArgs) => { handler.Invoke(source, eventArgs); },
                OnDetachAction = (instance, weakEventListener) => instance.GeoViewHolding -= weakEventListener.OnEvent
            };
            sender.GeoViewHolding += listener.OnEvent;
        }
    }
}
