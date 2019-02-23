using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KmlViewer
{
	public sealed partial class CompassHeading : UserControl
	{
		public CompassHeading()
		{
			this.InitializeComponent();
			SizeChanged += CompassHeading_SizeChanged;
		}

		private void CompassHeading_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			SetHeading(Heading);
		}

		private void SetHeading(double heading)
		{
			var offset = this.ActualWidth * .5;
			if(heading > 180)
			offset += (heading - 720) * 3;
			else
				offset += (heading - 360) * 3;
			trans.X = offset;
		}

		public double Heading
		{
			get { return (double)GetValue(HeadingProperty); }
			set { SetValue(HeadingProperty, value); }
		}

		public static readonly DependencyProperty HeadingProperty =
			DependencyProperty.Register("Heading", typeof(double), typeof(CompassHeading), new PropertyMetadata(0d, OnCompassHeadingPropertyChanged));

		private static void OnCompassHeadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CompassHeading)d).SetHeading((double)e.NewValue);
		}
	}
}
