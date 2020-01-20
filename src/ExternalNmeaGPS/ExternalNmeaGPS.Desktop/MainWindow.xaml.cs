using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
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
		private readonly Controls.SatelliteViewWindow skyViewWindow = new Controls.SatelliteViewWindow();
		private readonly Controls.NmeaRawMessagesWindow messagesWindow = new Controls.NmeaRawMessagesWindow();
		private NmeaParser.NmeaDevice? currentNmeaDevice;
		private string? currentNmeaFile;

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
			if (mapView.LocationDisplay.DataSource != null)
				mapView.LocationDisplay.DataSource.LocationChanged -= LocationProvider_LocationChanged;
			if (currentNmeaDevice != null)
			{
				currentNmeaDevice.MessageReceived -= device_MessageReceived;
			}

			currentNmeaDevice = device;
			currentNmeaDevice.MessageReceived += device_MessageReceived;
			mapView.LocationDisplay.DataSource = new NmeaLocationProvider(currentNmeaDevice);
			mapView.LocationDisplay.IsEnabled = true;
			mapView.LocationDisplay.DataSource.LocationChanged += LocationProvider_LocationChanged;
		}

		private void LocationProvider_LocationChanged(object? sender, Esri.ArcGISRuntime.Location.Location e)
		{
			Dispatcher.BeginInvoke((Action)delegate()
			{
				//Zoom in on first location fix
				mapView.LocationDisplay.DataSource.LocationChanged -= LocationProvider_LocationChanged;
				mapView.SetViewpointCenterAsync(e.Position, 5000);
				mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Navigation;
			});
		}
		
		private void device_MessageReceived(object? sender, NmeaParser.NmeaMessageReceivedEventArgs args)
		{
			Dispatcher.BeginInvoke((Action) delegate()
			{
				messagesWindow.AddMessage(args.Message);

				if(args.Message is NmeaParser.Messages.Gsv gsv)
				{
					skyViewWindow.GsvMessage= gsv;
				}
			});
		}

		private void MenuItemShowSkyView_Click(object sender, RoutedEventArgs e)
		{
			skyViewWindow.Show();
			skyViewWindow.Activate();
			skyViewWindow.Owner = this;
		}

		private void MenuItemShowMessages_Click(object sender, RoutedEventArgs e)
		{
			messagesWindow.Show();
			messagesWindow.Activate();
			messagesWindow.Owner = this;
		}

		private void SerialPortOpen_Click(object sender, RoutedEventArgs e)
		{
			if(PortsList.SelectedItem != null)
			{
				mapView.GraphicsOverlays.First().Graphics.Clear();
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
                mapView.GraphicsOverlays.First().Graphics.Clear();
				var device = new NmeaParser.NmeaFileDevice(currentNmeaFile, 50);
				LoadDevice(device);
				SelectedFileName.Text = new System.IO.FileInfo(currentNmeaFile).Name;
			}
		}

		private void LoadAsLayer_Click(object sender, RoutedEventArgs e)
		{
			if (currentNmeaFile != null)
				LoadEntireNmeaTrack(currentNmeaFile);
		}

		private void LoadEntireNmeaTrack(string filename)
		{
			var layer = mapView.GraphicsOverlays.First();
			layer.Graphics.Clear();
			if (currentNmeaFile == null)
				return;
			List<MapPoint> vertices = new List<MapPoint>();
			using (var sr = System.IO.File.OpenText(filename))
			{
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					if (line != null && line.StartsWith("$"))
					{
						try
						{
							var msg = NmeaParser.Messages.NmeaMessage.Parse(line);
							if (msg is NmeaParser.Messages.Rmc rmc)
							{
								if (!double.IsNaN(rmc.Longitude))
									vertices.Add(new MapPoint(rmc.Longitude, rmc.Latitude));
							}
						}
						catch { }
					}
				}
			}
			var pline = new Esri.ArcGISRuntime.Geometry.Polyline(vertices, SpatialReferences.Wgs84);
			var linesymbol = new SimpleLineSymbol() { Width = 4, Color = System.Drawing.Color.CornflowerBlue };
			var symbol = new CompositeSymbol();
			symbol.Symbols.Add(linesymbol);
			symbol.Symbols.Add(new SimpleMarkerSymbol() { Size = 5, Color = System.Drawing.Color.Black });
			layer.Graphics.Add(new Graphic(pline, symbol));
		}

        private void MenuItemFileExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
	}
}
