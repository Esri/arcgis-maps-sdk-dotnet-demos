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

namespace ExternalNmeaGPS
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ExternalNmeaGPS.Controls.SatelliteViewWindow skyViewWindow = new Controls.SatelliteViewWindow();
		private ExternalNmeaGPS.Controls.NmeaRawMessagesWindow messagesWindow = new Controls.NmeaRawMessagesWindow();
		private NmeaParser.NmeaDevice currentNmeaDevice;
		public MainWindow()
		{
			InitializeComponent();
			NmeaParser.NmeaDevice device = new NmeaParser.NmeaFileDevice("NmeaSampleData.txt", 50);
			LoadDevice(device);
			var ports = System.IO.Ports.SerialPort.GetPortNames();
		 	PortsList.ItemsSource = ports;
			if(ports.Any())
				PortsList.SelectedIndex = 0;
		}

		private void LoadDevice(NmeaParser.NmeaDevice device)
		{
			if (mapView.LocationDisplay.LocationProvider != null)
				mapView.LocationDisplay.LocationProvider.LocationChanged -= LocationProvider_LocationChanged;
			if (currentNmeaDevice != null)
			{
				currentNmeaDevice.MessageReceived -= device_MessageReceived;
			}

			currentNmeaDevice = device;
			currentNmeaDevice.MessageReceived += device_MessageReceived;
			mapView.LocationDisplay.LocationProvider = new NmeaLocationProvider(currentNmeaDevice);
			mapView.LocationDisplay.IsEnabled = true;
			mapView.LocationDisplay.LocationProvider.LocationChanged += LocationProvider_LocationChanged;
		}

		private void LocationProvider_LocationChanged(object sender, Esri.ArcGISRuntime.Location.LocationInfo e)
		{
			Dispatcher.BeginInvoke((Action)delegate()
			{
				//Zoom in on first location fix
				mapView.LocationDisplay.LocationProvider.LocationChanged -= LocationProvider_LocationChanged;
				mapView.SetView(e.Location, 5000);
				mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.Location.AutoPanMode.Navigation;
			});
		}
		
		private void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs args)
		{
			Dispatcher.BeginInvoke((Action) delegate()
			{
				messagesWindow.AddMessage(args.Message);

				if(args.Message is NmeaParser.Nmea.Gps.Gpgsv)
				{
					var gpgsv = (NmeaParser.Nmea.Gps.Gpgsv)args.Message;
					if(args.IsMultiPart && args.MessageParts != null)
						skyViewWindow.GpgsvMessages = args.MessageParts.OfType<NmeaParser.Nmea.Gps.Gpgsv>();
				}
			});
		}

		private void MenuItemShowSkyView_Click(object sender, RoutedEventArgs e)
		{
			skyViewWindow.Show();
			skyViewWindow.Activate();
		}

		private void MenuItemShowMessages_Click(object sender, RoutedEventArgs e)
		{
			messagesWindow.Show();
			messagesWindow.Activate();
		}

		private void SerialPortOpen_Click(object sender, RoutedEventArgs e)
		{
			if(PortsList.SelectedItem != null)
			{
				var port = new System.IO.Ports.SerialPort((string)PortsList.SelectedItem, int.Parse(BaudRate.Text));
				var device = new NmeaParser.SerialPortDevice(port);
				LoadDevice(device);
			}
		}

		private void BrowseForNmeaFile_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
			var result = dialog.ShowDialog();
			if(result.HasValue && result.Value)
			{
				var file = dialog.FileName;
				var device = new NmeaParser.NmeaFileDevice(file, 50);
				LoadDevice(device);
				SelectedFileName.Text = new System.IO.FileInfo(file).Name;
			}
		}
	}
}
