#if __ANDROID__ || __IOS__
using System;
using System.Collections.Generic;
using System.Text;

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
                    return 1.0 - EasingImplementation(1.0 - normalizedTime);
                case EasingMode.EaseIn:
                    return EasingImplementation(normalizedTime);
                case EasingMode.EaseInOut:
                default:
                    return (normalizedTime < 0.5) ? EasingImplementation(normalizedTime * 2.0) * 0.5 : (1.0 - EasingImplementation((1.0 - normalizedTime) * 2.0)) * 0.5 + 0.5;
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
#endif