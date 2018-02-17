using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif

namespace GeoEventServerSample.Animation
{
    public static class AnimatorHelper
    {
        private static object animationsLock = new object();
        private static List<AnimationHandle> animations = new List<AnimationHandle>();

        internal static void QueueAnimation(AnimationHandle handle)
        {
            int count = 0;
            lock (animationsLock)
            {
                animations.Add(handle);
                count = animations.Count;
            }

            if (count == 1)
                StartAnimationLoop();
        }
        private static void StartAnimationLoop()
        {
#if XAMARIN
            throw new NotImplementedException("TODO");
#else
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                CompositionTarget.Rendering += OnFrameEvent;
            }
            else
                System.Windows.Application.Current.Dispatcher.Invoke(StartAnimationLoop);
#endif
        }

        private static void StopAnimationLoop()
        {
#if XAMARIN
            throw new NotImplementedException("TODO");
#else
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
                CompositionTarget.Rendering -= OnFrameEvent;
            else
                System.Windows.Application.Current.Dispatcher.Invoke(StopAnimationLoop);
#endif
        }

        private static void OnFrameEvent(object sender, object e)
        {
#if XAMARIN
            TimeSpan renderingTime; //TODO
#else
            var args = e as RenderingEventArgs;
            TimeSpan renderingTime = args.RenderingTime;
#endif
            AnimationHandle[] a = null;
            lock (animationsLock)
                a = animations.ToArray();
            foreach (var handle in a)
            {
                if (handle.IsDiposed)
                {
                    lock(animationsLock)
                        animations.Remove(handle);
                }
                else
                {
                    handle.Pulse(renderingTime);
                }
            }
            if (a.Length == 0)
                StopAnimationLoop();
        }

        public static void ClearAllAnimations()
        {
            int count = 0;
            lock (animations)
            {
                count = animations.Count;
                if (count > 0)
                {
                    foreach (var a in animations)
                        a.Dispose();

                    animations.Clear();
                }
            }
            if (count > 0)
                StopAnimationLoop();
        }
    }
}
