using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExternalNmeaGPS.Controls
{
	/// <summary>
	/// Interaction logic for SatelliteView.xaml
	/// </summary>
	public partial class SatelliteSnr : UserControl
	{
		public SatelliteSnr()
		{
			InitializeComponent();
		}

		public NmeaParser.Nmea.Gsv GsvMessage
		{
			get { return (NmeaParser.Nmea.Gsv)GetValue(GsvMessageProperty); }
			set { SetValue(GsvMessageProperty, value); }
		}

		public static readonly DependencyProperty GsvMessageProperty =
			DependencyProperty.Register(nameof(GsvMessage), typeof(NmeaParser.Nmea.Gsv), typeof(SatelliteSnr), new PropertyMetadata(null, OnGsvMessagePropertyChanged));

		private static void OnGsvMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var sats = e.NewValue as NmeaParser.Nmea.Gsv;
			if (sats == null)
				((SatelliteSnr)d).satellites.ItemsSource = null;
			else
				((SatelliteSnr)d).satellites.ItemsSource = sats.SVs;
		}		
	}
}
