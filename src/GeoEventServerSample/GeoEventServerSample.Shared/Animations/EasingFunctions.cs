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

namespace GeoEventServerSample.Animations
{
    public enum EasingMode
    {
        EaseIn = 0,
        EaseOut = 1,
        EaseInOut = 2
    }

    public abstract class EasingFunctionBase
    {
        protected EasingFunctionBase() { }

        public EasingMode EasingMode { get; set; }

        public double Ease(double normalizedTime)
        {
            switch (EasingMode)
            {
                case EasingMode.EaseOut:
                return 1 - EasingImplementation(1 - normalizedTime);
                case EasingMode.EaseIn:
                return EasingImplementation(normalizedTime);
                case EasingMode.EaseInOut:
                default:
                    return (normalizedTime < 0.5) ? EasingImplementation(normalizedTime * 2) * 0.5 : (1 - EasingImplementation((1 - normalizedTime) * 2)) * 0.5 + 0.5;
            }

        }

        protected abstract double EasingImplementation(double normalizedTime);
    }

    public class QuadraticEase : EasingFunctionBase
    {
        protected override double EasingImplementation(double normalizedTime)
        {
            return normalizedTime * normalizedTime;
        }
    }
}