using System;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace ArcGISMapViewer.Controls
{
    public sealed partial class ScaleRangeSelector : CommunityToolkit.WinUI.Controls.RangeSelector
    {
        public ScaleRangeSelector()
        {
            this.DefaultStyleKey = typeof(ScaleRangeSelector);
            Minimum = 7;
            Maximum = 30;
            StepFrequency = 0.1;
        }

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
        }
        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var ctrl = GetTemplateChild("TooltipContent") as ContentControl;
            if (GetTemplateChild("MinThumb") is Thumb _minThumb)
                _minThumb.DragDelta += MinThumb_DragDelta;
            if (GetTemplateChild("MaxThumb") is Thumb _maxThumb)
                _maxThumb.DragDelta += MaxThumb_DragDelta;
        }

        private void MaxThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            UpdateScales();
            if (GetTemplateChild("TooltipContent") is ContentControl ctrl)
            {
                if (MaxScale == 0)
                    ctrl.Content = "Unbounded";
                else 
                    ctrl.Content = $"1:{Math.Round(MaxScale,0)}";
            }
        }

        private void MinThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            UpdateScales();
            if (GetTemplateChild("TooltipContent") is ContentControl ctrl)
            {
                if (MinScale == 0)
                    ctrl.Content = "Unbounded";
                else
                    ctrl.Content = $"1:{Math.Round(MinScale, 0)}";
            }
        }

        public double CurrentScale
        {
            get { return (double)GetValue(CurrentScaleProperty); }
            set { SetValue(CurrentScaleProperty, value); }
        }

        public static readonly DependencyProperty CurrentScaleProperty =
            DependencyProperty.Register("CurrentScale", typeof(double), typeof(ScaleRangeSelector), new PropertyMetadata(0d, (s, e) => ((ScaleRangeSelector)s).OnCurrentScalePropertyChanged((double)e.NewValue)));

        private void OnCurrentScalePropertyChanged(double newValue)
        {
            // TODO
        }

        public double MinScale
        {
            get { return (double)GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register("MinScale", typeof(double), typeof(ScaleRangeSelector), new PropertyMetadata(-1d, (s, e) => ((ScaleRangeSelector)s).OnMinScalePropertyChanged((double)e.NewValue)));

        private void OnMinScalePropertyChanged(double newValue)
        {
            UpdateRanges();
        }

        public double MaxScale
        {
            get { return (double)GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register("MaxScale", typeof(double), typeof(ScaleRangeSelector), new PropertyMetadata(-1d, (s, e) => ((ScaleRangeSelector)s).OnMaxScalePropertyChanged((double)e.NewValue)));

        private void OnMaxScalePropertyChanged(double newValue)
        {
            UpdateRanges();
        }


        private void UpdateRanges()
        {
            if (ignore) return;
            ignore = true;
            var min = Maximum - Math.Log(MinScale, 2) + Minimum;
            var max = Maximum - Math.Log(MaxScale, 2) + Minimum;
            RangeStart = double.IsNormal(min) ? Math.Max(Minimum, min) : Minimum;
            RangeEnd = double.IsNormal(max) ? Math.Min(Maximum, max) : Maximum;
            ignore = false;
        }
        bool ignore;
        protected override void OnValueChanged(RangeChangedEventArgs e)
        {
            base.OnValueChanged(e);
            UpdateScales();
        }
        private void UpdateScales()
        { 
            if (ignore) return;
            ignore = true;
            var min = Minimum == RangeStart ? 0 : Math.Pow(2, Maximum - RangeStart + Minimum);
            var max = Maximum == RangeEnd ? 0 : Math.Pow(2, Maximum - RangeEnd + Minimum);
            MinScale = min;
            MaxScale = max;
            ignore = false;
        }
    }
}
