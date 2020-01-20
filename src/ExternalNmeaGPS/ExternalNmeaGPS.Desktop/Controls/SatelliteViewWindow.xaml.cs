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
using System.Windows.Shapes;

namespace ExternalNmeaGPS.Controls
{
	/// <summary>
	/// Interaction logic for SatelliteViewWindow.xaml
	/// </summary>
	public partial class SatelliteViewWindow : Window
	{
		public SatelliteViewWindow()
		{
			InitializeComponent();
		}
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			this.Hide();
			base.OnClosing(e);
		}

		public NmeaParser.Messages.Gsv GsvMessage
		{
			get { return satView.GsvMessage; }
			set { satView.GsvMessage = value; }
		}
		
	}
}
