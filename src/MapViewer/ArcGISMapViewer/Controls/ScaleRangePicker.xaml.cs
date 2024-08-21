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
    public sealed partial class ScaleRangePicker : UserControl
    {
        public ScaleRangePicker()
        {
            this.InitializeComponent();
        }

        public double MinScale
        {
            get { return (double)GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register("MinScale", typeof(double), typeof(ScaleRangePicker), new PropertyMetadata(-1d, (s, e) => ((ScaleRangePicker)s).OnMinScalePropertyChanged((double)e.NewValue)));

        private void OnMinScalePropertyChanged(double newValue)
        {
            UpdateDropDown(newValue, MinCombo, 0);
        }

        public double MaxScale
        {
            get { return (double)GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register("MaxScale", typeof(double), typeof(ScaleRangePicker), new PropertyMetadata(-1d, (s, e) => ((ScaleRangePicker)s).OnMaxScalePropertyChanged((double)e.NewValue)));

        private void OnMaxScalePropertyChanged(double newValue)
        {
            UpdateDropDown(newValue, MaxCombo, MaxCombo.Items.Count - 1);
        }

        private void UpdateDropDown(double newValue, ComboBox combo, int defaultIndex)
        {
            if (ignore) return;
            if (newValue <= 0)
            {
                combo.SelectedIndex = defaultIndex;
                return;
            }
            ignore = true;
            int index = 0;
            foreach (var value in combo.Items)
            {
                if (value is ComboBoxItem item && double.TryParse(item.Tag as string, out double scale))
                {
                    if (scale < newValue)
                    {
                        combo.SelectedIndex = index;

                        ignore = false;
                        return;
                    }
                }
                index++;
            }
            combo.SelectedIndex = combo.Items.Count - 1;
            ignore = false;
        }
        bool ignore;
        private void MinCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ignore) return;
            ignore = true;
            if (double.TryParse((MinCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString(), out double scale))
            {
                MinScale = scale;
            }
            ignore = false;
        }

        private void MaxCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignore) return;
            ignore = true;
            if (double.TryParse((MaxCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString(), out double scale))
            {
                MaxScale = scale;
            }
            ignore = false;
        }
    }
}
