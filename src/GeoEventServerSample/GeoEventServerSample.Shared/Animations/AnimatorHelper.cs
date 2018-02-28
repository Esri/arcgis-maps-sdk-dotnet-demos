// /*******************************************************************************
//  * Copyright 2012-2018 Esri
//  *
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************/

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
            Action init = () =>
            {
            if (timeAnimator == null)
            {
                    timeAnimator = new Android.Animation.TimeAnimator()
                    {
                        RepeatCount = Android.Animation.ValueAnimator.Infinite
                    };
                    timeAnimator.Time += (s, e) => OnFrameEvent(TimeSpan.FromMilliseconds(e.TotalTime));
            }
                timeAnimator.CurrentPlayTime = 0;
                timeAnimator.Start();
            };
            if (Android.OS.Looper.MainLooper.IsCurrentThread)
                init();
            else
            {
                using (var h = new Android.OS.Handler(Android.OS.Looper.MainLooper))
                {
                    h.Post(init);
                }
            }
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
                var _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => CompositionTarget.Rendering += CompositionTargetRendering);
            }
#else
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
                CompositionTarget.Rendering += CompositionTargetRendering;
            else
                System.Windows.Application.Current.Dispatcher.Invoke(()=> CompositionTarget.Rendering += CompositionTargetRendering);
#endif
        }

        private static void StopAnimationLoop()
        {
#if __ANDROID__
            if (Android.OS.Looper.MainLooper.IsCurrentThread)
                timeAnimator?.Cancel();
            else
            {
                using (var h = new Android.OS.Handler(Android.OS.Looper.MainLooper))
                {
                    h.Post(() => timeAnimator?.Cancel());
                }
            }
#elif __IOS__
            _displayLink?.Invalidate();
            _displayLink = null;
#elif NETFX_CORE
            if (Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                CompositionTarget.Rendering -= CompositionTargetRendering;
            else
            {
                var _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => CompositionTarget.Rendering -= CompositionTargetRendering);
            }
#else
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
                CompositionTarget.Rendering -= CompositionTargetRendering;
            else
                System.Windows.Application.Current.Dispatcher.Invoke(() => CompositionTarget.Rendering -= CompositionTargetRendering);
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
            int count = 0;
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
                    count++;
                }
            }
            if (count == 0)
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