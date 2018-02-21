using System;
using System.Collections.Generic;
#if NETFX_CORE
using Windows.UI.Xaml.Media;
#elif !__ANDROID__ && !__IOS__
using System.Windows.Media;
#endif

namespace GeoEventServerSample.Animations
{
    /// <summary>
    /// A cross-platform animation helper for running time-based animations synced to the device framerate
    /// </summary>
    public static class AnimatorHelper
    {
        private static object animationsLock = new object();
        private static List<AnimationHandle> animations = new List<AnimationHandle>();
#if __ANDROID__
        private static Android.Animation.TimeAnimator timeAnimator;
#elif __IOS__
        private static CoreAnimation.CADisplayLink _displayLink;
#endif

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
#if __ANDROID__
            if (timeAnimator == null)
            {
                timeAnimator = new Android.Animation.TimeAnimator();
                timeAnimator.Update += (s,e) => OnFrameEvent(TimeSpan.FromMilliseconds(e.Animation.CurrentPlayTime));
            }
            timeAnimator.Start();
#elif __IOS__
            if (_displayLink == null)
            {
                _displayLink = CoreAnimation.CADisplayLink.Create(() => OnFrameEvent(TimeSpan.FromSeconds(_displayLink.Timestamp)));
                _displayLink.AddToRunLoop(Foundation.NSRunLoop.Main, Foundation.NSRunLoop.NSDefaultRunLoopMode);
            }
#elif NETFX_CORE
            if (Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                CompositionTarget.Rendering += CompositionTargetRendering;
            else
            {
                var _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, StartAnimationLoop);
            }
#else
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
                CompositionTarget.Rendering += CompositionTargetRendering;
            else
                System.Windows.Application.Current.Dispatcher.Invoke(StartAnimationLoop);
#endif
        }

        private static void StopAnimationLoop()
        {
#if __ANDROID__
            timeAnimator?.Cancel();
#elif __IOS__
            _displayLink?.Invalidate();
            _displayLink = null;
#elif NETFX_CORE
            if (Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                CompositionTarget.Rendering -= CompositionTargetRendering;
            else
            {
                var _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, StartAnimationLoop);
            }
#else
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
                CompositionTarget.Rendering -= CompositionTargetRendering;
            else
                System.Windows.Application.Current.Dispatcher.Invoke(StopAnimationLoop);
#endif
        }

#if !__ANDROID__ && !__IOS__
        private static void CompositionTargetRendering(object sender, object e)
        {
            OnFrameEvent(((RenderingEventArgs)e).RenderingTime);
        }
#endif

        private static void OnFrameEvent(TimeSpan renderingTime)
        { 
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