using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace LocalNetworkSample.Controls
{
	public class TwoStateControl : ButtonBase
	{
		public TwoStateControl()
		{
			DefaultStyleKey = typeof(TwoStateControl);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			ChangeVisualState(false);
		}

		public bool IsSecondStateEnabled
		{
			get { return (bool)GetValue(IsSecondStateEnabledProperty); }
			set { SetValue(IsSecondStateEnabledProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="IsSecondStateEnabled"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsSecondStateEnabledProperty =
			DependencyProperty.Register("IsSecondStateEnabled", typeof(bool), typeof(TwoStateControl), new PropertyMetadata(false, OnIsSecondStateEnabledPropertyChanged));

		private static void OnIsSecondStateEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TwoStateControl obj = (TwoStateControl)d;
			obj.ChangeVisualState(true);
		}

		private void ChangeVisualState(bool useTransitions)
		{
			if (IsSecondStateEnabled)
				GoToState(useTransitions, "Two");
			else
				GoToState(useTransitions, "One");
		}

		private bool GoToState(bool useTransitions, string stateName)
		{
			return VisualStateManager.GoToState(this, stateName, useTransitions);
		}
	}
}
