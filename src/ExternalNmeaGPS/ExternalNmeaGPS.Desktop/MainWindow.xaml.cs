using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
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
		private string currentNmeaFile;
		public MainWindow()
		{
			InitializeComponent();
			currentNmeaFile = "NmeaSampleData.txt";
			NmeaParser.NmeaDevice device = new NmeaParser.NmeaFileDevice(currentNmeaFile);
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
					if(args.IsMultipart && args.MessageParts != null)
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
				mapView.Map.Layers.OfType<GraphicsLayer>().First().Graphics.Clear();
				var port = new System.IO.Ports.SerialPort((string)PortsList.SelectedItem, int.Parse(BaudRate.Text));
				var device = new NmeaParser.SerialPortDevice(port);
				currentNmeaFile = null;
				LoadDevice(device);
			}
		}

		private void BrowseForNmeaFile_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
			var result = dialog.ShowDialog();
			if(result.HasValue && result.Value)
			{
				currentNmeaFile = dialog.FileName;
				mapView.Map.Layers.OfType<GraphicsLayer>().First().Graphics.Clear();
				var device = new NmeaParser.NmeaFileDevice(currentNmeaFile, 50);
				LoadDevice(device);
				SelectedFileName.Text = new System.IO.FileInfo(currentNmeaFile).Name;
			}
		}

		private void LoadAsLayer_Click(object sender, RoutedEventArgs e)
		{
			LoadEntireNmeaTrack(currentNmeaFile);
		}

		private void LoadEntireNmeaTrack(string filename)
		{
			var layer = mapView.Map.Layers.OfType<GraphicsLayer>().First();
			layer.Graphics.Clear();
			if (currentNmeaFile == null)
				return;
			List<MapPoint> vertices = new List<MapPoint>();
			using (var sr = System.IO.File.OpenText(filename))
			{
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					if (line.StartsWith("$"))
					{
						try
						{
							var msg = NmeaParser.Nmea.NmeaMessage.Parse(line);
							if (msg is NmeaParser.Nmea.Gps.Gprmc)
							{
								var rmc = (NmeaParser.Nmea.Gps.Gprmc)msg;
								if (!double.IsNaN(rmc.Longitude))
									vertices.Add(new MapPoint(rmc.Longitude, rmc.Latitude));
							}
						}
						catch { }
					}
				}
			}
			var pline = new Esri.ArcGISRuntime.Geometry.Polyline(vertices, SpatialReferences.Wgs84);
			var linesymbol = new SimpleLineSymbol() { Width = 4, Color = Colors.CornflowerBlue };
			var symbol = new CompositeSymbol();
			symbol.Symbols.Add(linesymbol);
			symbol.Symbols.Add(new SimpleMarkerSymbol() { Size = 5, Color = Colors.Black });

			Esri.ArcGISRuntime.Layers.Graphic g = new Esri.ArcGISRuntime.Layers.Graphic(pline, symbol);
			layer.Graphics.Add(g);
		}
	}
}
