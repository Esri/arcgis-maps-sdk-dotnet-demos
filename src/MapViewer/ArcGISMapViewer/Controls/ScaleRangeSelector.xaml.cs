using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISMapViewer.Controls
{
    public sealed partial class ScaleRangeSelector : UserControl
    {
        public ScaleRangeSelector()
        {
            this.InitializeComponent();
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
            UpdateDropDown(newValue, MinCombo, 0);
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
            UpdateDropDown(newValue, MaxCombo, MaxCombo.Items.Count - 1);
        }

        private void UpdateDropDown(double newValue, ComboBox combo, int defaultIndex)
        {
            if (newValue <= 0)
            {
                combo.SelectedIndex = defaultIndex;
                return;
            }
            int index = 0;
            foreach (var value in combo.Items)
            {
                if (value is ComboBoxItem item && double.TryParse(item.Tag as string, out double scale))
                {
                    if (scale < newValue)
                    {
                        combo.SelectedIndex = index;
                        return;
                    }
                }
                index++;
            }
            combo.SelectedIndex = combo.Items.Count - 1;
        }

        private void UpdateRanges()
        {
            if (ignore) return;
            ignore = true;
            var min = RangeSelector.Maximum - Math.Log(MinScale, 2) + RangeSelector.Minimum;
            var max = RangeSelector.Maximum - Math.Log(MaxScale, 2) + RangeSelector.Minimum;
            RangeSelector.RangeStart = double.IsNormal(min) ? Math.Max(RangeSelector.Minimum, min) : RangeSelector.Minimum;
            RangeSelector.RangeEnd = double.IsNormal(max) ? Math.Min(RangeSelector.Maximum, max) : RangeSelector.Maximum;
            ignore = false;
        }
        bool ignore;
        private void RangeSelector_ValueChanged(object sender, CommunityToolkit.WinUI.Controls.RangeChangedEventArgs e)
        {
            if (ignore) return;
            ignore = true;
            var min = Math.Pow(2, RangeSelector.Maximum - RangeSelector.RangeStart + RangeSelector.Minimum);
            var max = Math.Pow(2, RangeSelector.Maximum - RangeSelector.RangeEnd + RangeSelector.Minimum);
            MinScale = min;
            MaxScale = max;
            ignore = false;
        }
    }
}
