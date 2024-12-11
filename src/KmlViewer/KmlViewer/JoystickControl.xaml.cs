using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace KmlViewer
{
    public sealed partial class JoystickControl : UserControl
    {
		DispatcherTimer timer;
        public JoystickControl()
        {
            this.InitializeComponent();
			timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(15) };
			timer.Tick += timer_Tick;
        }

		void timer_Tick(object sender, object e)
		{
			if (ValueTick != null)
				ValueTick(this, translationFactor);
		}
		private double translation;
		private double translationFactor;
		private void Border_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			var ty = e.Cumulative.Translation.Y / 4;
			double maxTy = border.ActualHeight * .5 - (border.BorderThickness.Top - border.BorderThickness.Bottom) * .5 - thumb.ActualHeight * .5;
			if(ty<0)
			{
				translation = Math.Max(-maxTy, ty);
			}
			else
			{
				translation = Math.Min(maxTy, ty);
			}
			translationTransform.Y = translation;
			translationFactor = translation / maxTy;
			if (!timer.IsEnabled) {
				timer.Start();
				timer_Tick(null, null);
			}
		}

		private void Border_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			translation = 0;
			translationFactor = 0;
			translationTransform.Y = 0;
			timer.Stop();
		}

		public event EventHandler<double> ValueTick;
    }
}
