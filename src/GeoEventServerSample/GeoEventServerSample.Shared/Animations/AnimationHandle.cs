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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoEventServerSample.Animations
{
    public class AnimationHandle : IDisposable
    {
        private TimeSpan _startTime = TimeSpan.Zero;
        private TimeSpan _duration;
        private Action<double> _onPulse;
        private Action _onStart;
        private Action _onEnd;
        private bool _hasEnded;
        private EasingFunctionBase _easing;
        private TaskCompletionSource<bool> _tcs;

        public static AnimationHandle StartAnimation(TimeSpan duration, Action<double> onPulse, Action onStart = null, Action onEnd = null, bool repeat = false, EasingFunctionBase easing = null)
        {
            var handle = new AnimationHandle(duration, onPulse, onStart, onEnd, repeat, easing);
            AnimatorHelper.QueueAnimation(handle);
            return handle;
        }

        private AnimationHandle(TimeSpan duration, Action<double> onPulse, Action onStart = null, Action onEnd = null, bool repeat = false, EasingFunctionBase easing = null)
        {
            _duration = duration;
            IsRepeat = repeat;
            _onPulse = onPulse;
            _onStart = onStart;
            _onEnd = () => { if(!_hasEnded) onEnd?.Invoke(); _hasEnded = true; };
            _easing = easing;
        }

        ~AnimationHandle()
        {
            Dispose(true);
        }

        internal void Pulse(TimeSpan time)
        {
            if (_startTime.Ticks == 0)
            {
                _startTime = time;
                _onStart?.Invoke();
                return;
            }
            var elapsed = time - _startTime;
            if (IsPaused)
            {
                _startTime += elapsed;
            }
            else
            {
                var totalDuration = AutoReverse ? _duration.Add(_duration) : _duration;
                while (elapsed > totalDuration && IsRepeat)
                {
                    _startTime = time - (elapsed - totalDuration);
                    elapsed = time - _startTime;
                }
                if (elapsed > totalDuration)
                {
                    _onEnd?.Invoke();
                    _tcs?.TrySetResult(true);
                    Dispose();
                }
                else
                {
                    double normalizedTime;
                    if (!AutoReverse || elapsed < _duration)
                        normalizedTime = elapsed.TotalMilliseconds / _duration.TotalMilliseconds;
                    else
                        normalizedTime = 1 - (elapsed - _duration).TotalMilliseconds / _duration.TotalMilliseconds;
                    if (_easing != null)
                        normalizedTime = _easing.Ease(normalizedTime);
                    _onPulse?.Invoke(normalizedTime);
                }
            }
        }
        public bool IsRepeat { get; set; }
        public bool AutoReverse { get; set; }
        public void Stop() { Dispose(); }
        public void Pause() { IsPaused = true; }
        public void Resume() { IsPaused = false; }

        public void Dispose()
        {
            Dispose(false);
        }

        public void Dispose(bool isfinalize)
        {
            IsDiposed = true;
            if (!isfinalize)
            {
                _onEnd?.Invoke();
            }
            _tcs?.TrySetResult(false);
        }

        private bool IsPaused { get; set; }

        internal bool IsDiposed { get; private set; }
        
        public Task<bool> AsTask()
        {
            if(_tcs == null)
            {
                _tcs = new TaskCompletionSource<bool>();
            }
            return _tcs.Task;
        }
    }
}
