using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
#if (NET5_0 || NET5_0_OR_GREATER) && WINDOWS
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
#endif

namespace ExternalNmeaGPS
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly Controls.SatelliteViewWindow skyViewWindow = new Controls.SatelliteViewWindow();
		private NmeaLocationDataSource? currentNmeaDevice;

		public MainWindow()
		{
			InitializeComponent();
			mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Navigation;
			mapView.LocationDisplay.InitialZoomScale = 5000;
			this.Closing += (s, e) => { mapView.LocationDisplay.IsEnabled = false; }; // Ensure datasource gets stopped on shutdown
			var device = NmeaLocationDataSource.FromStreamCreator(
			 	(s) => Task.FromResult<Stream>(new BufferedFileStream(new System.IO.StreamReader("NmeaSampleData.txt"), new BurstEmulationSettings())));
			LoadPorts();
			LoadDevice(device);

#if (NET5_0 || NET5_0_OR_GREATER) && WINDOWS
			// Monitor for added or removed serial and bluetooth devices
			var watcher = DeviceInformation.CreateWatcher(SerialDevice.GetDeviceSelector());
			watcher.Added += (s, e) => Dispatcher.InvokeAsync(LoadPorts);
			watcher.Removed += (s, e) => Dispatcher.InvokeAsync(LoadPorts);
			watcher.Start();
			watcher = DeviceInformation.CreateWatcher(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
			watcher.Added += (s, e) => Dispatcher.InvokeAsync(LoadPorts);
			watcher.Removed += (s, e) => Dispatcher.InvokeAsync(LoadPorts);
			watcher.Start();
#endif
		}

		#region Get list of available devices

		/// <summary>
		/// Class to hold information about available devices
		/// </summary>
		public class Device
		{
			public Device(string name) { Name = name; }
			public string Name { get; }

#if (NET5_0 || NET5_0_OR_GREATER) && WINDOWS
			public DeviceInformation? DeviceInfo { get; set; }
#endif
			public bool IsSerialPort { get; set; } = true;
			public override string ToString() => Name!;
		}

#if (NET5_0 || NET5_0_OR_GREATER) && WINDOWS
		// Use WinRT APIs instead
		public async void LoadPorts()
		{
			List<Device> list = new List<Device>();
			if (Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.Devices.Enumeration.DeviceInformation", "FindAllAsync")) // Ensure we are on an OS where these WinRT APIs are supported
			{
				//Get list of serial port devices 
				if (Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.Devices.SerialCommunication.SerialDevice", "GetDeviceSelector"))
				{
					// Get a list of serial devices
					var devices = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
					list.AddRange(devices.Where(t => t.IsEnabled).Select(t => new Device(t.Name) { DeviceInfo = t, IsSerialPort = !t.Id.Contains("\\BTHENUM#") }));
				}
				//Get list of bluetooth devices 
				if (Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService", "GetDeviceSelector"))
				{
					string serialDeviceType = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
					var devices = await DeviceInformation.FindAllAsync(serialDeviceType); // Select a bluetooth serial port from this list 
					list.AddRange(devices.Where(t => t.IsEnabled).Select(t => new Device(t.Name) { DeviceInfo = t, IsSerialPort = false }));
				}
			}
			PortsList.ItemsSource = list;
			if (list.Any())
				PortsList.SelectedIndex = 0;

		}
#else
		public void LoadPorts()
		{ 
			var ports = System.IO.Ports.SerialPort.GetPortNames();
			PortsList.ItemsSource = ports.Select(p => new Device(p)).OrderBy(p => p.Name);
			if(ports.Any())
				PortsList.SelectedIndex = 0;
		}
#endif
		#endregion Get list of available devices

		private void LoadDevice(NmeaLocationDataSource device)
		{
			if (currentNmeaDevice != null)
			{
				currentNmeaDevice.SatellitesChanged -= Device_SatellitesChanged;
				currentNmeaDevice.StopAsync();
			}
			currentNmeaDevice = device;
			mapView.LocationDisplay.DataSource = currentNmeaDevice;
			mapView.LocationDisplay.IsEnabled = true;
            device.SatellitesChanged += Device_SatellitesChanged;
		}

		private void Device_SatellitesChanged(object? sender, IReadOnlyList<NmeaSatelliteInfo> satellites)
		{
			// Update sky view window
			Dispatcher.InvokeAsync(() => { skyViewWindow.SatelliteInfos = satellites; });
		}

		private async void SerialPortOpen_Click(object sender, RoutedEventArgs e)
		{
			if (PortsList.SelectedItem != null)
			{
				mapView.GraphicsOverlays!.First().Graphics.Clear();
				var deviceInfo = (Device)PortsList.SelectedItem;
#if (NET5_0 || NET5_0_OR_GREATER) && WINDOWS
				NmeaLocationDataSource device;
				var btdevice = await Windows.Devices.Bluetooth.BluetoothDevice.FromIdAsync(deviceInfo.DeviceInfo!.Id);
				if (btdevice is not null)
					device = NmeaLocationDataSource.FromBluetooth((await btdevice.GetRfcommServicesForIdAsync(RfcommServiceId.SerialPort)).Services.First());
				else 
					device = NmeaLocationDataSource.FromSerialPort(deviceInfo.DeviceInfo, uint.Parse(BaudRate.Text));
#else
				//Use custom datasource
				var device = new SerialPortLocationDataSource(deviceInfo.Name, int.Parse(BaudRate.Text));
				// Or use stream creator approach:
				/*
				var port = new System.IO.Ports.SerialPort(deviceInfo.Name, int.Parse(BaudRate.Text));
				var device = NmeaLocationDataSource.FromStreamCreator((s) =>
				{
					port.Open();
					return Task.FromResult(port.BaseStream);
				},
				(s) =>
				{
					port.Close();
					return Task.CompletedTask;
				});*/
#endif
				LoadDevice(device);
			}
		}

		private void BrowseForNmeaFile_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
			var result = dialog.ShowDialog();
			if(result.HasValue && result.Value)
			{
                mapView.GraphicsOverlays!.First().Graphics.Clear();
				var device = NmeaLocationDataSource.FromStreamCreator(
					(s) => Task.FromResult<Stream>(new BufferedFileStream(new System.IO.StreamReader(dialog.FileName), new BurstEmulationSettings())));
				LoadDevice(device);
			}
		}

        #region NTRIP

        private NtripClient? client;
		private Stream? currentNtripStream;

		private async void GetNtripStreams_Click(object sender, RoutedEventArgs e)
        {
			client = new NtripClient(ntripEndpoint.Text, int.Parse(ntripPort.Text), ntripUsername.Text, ntripPassword.Password);
			try
			{
				ntripStreamList.ItemsSource = null;
				var streams = await client.GetAvailableStreamsAsync();
				ntripStreamList.ItemsSource = streams;
				if (streams.Any())
					ntripStreamList.IsDropDownOpen = true;
			}
			catch (System.Exception ex)
			{
				MessageBox.Show($"Failed to get stream list: " + ex.Message);
			}
        }


		private long bytesTransferred = 0;

        private void NtripConnect_Click(object sender, RoutedEventArgs e)
        {
			var ntripstream = ntripStreamList.SelectedItem as NtripStream;
			if(ntripstream != null && client != null)
            {
				var ds = mapView.LocationDisplay.DataSource as NmeaLocationDataSource;
				if (ds is null)
				{
					MessageBox.Show("No location datasource");
					return;
				}
				var devicestream = ds.NmeaDataStream;
				if (devicestream is null)
				{
					MessageBox.Show("Datasource not started");
					return;
				}
				if (!devicestream.CanWrite)
				{
					MessageBox.Show("Stream doesn't support writing");
					return;
				}

				var ntripsocketstream = client.OpenStream(ntripstream);
				currentNtripStream?.Dispose();
				currentNtripStream = ntripsocketstream;
				Task.Run(() =>
				{
					bytesTransferred = 0;
					byte[] buffer = new byte[1024];
					while (ntripsocketstream.CanRead && devicestream.CanWrite)
					{
						int count = ntripsocketstream.Read(buffer);
						if (count > 0)
						{
							try
							{
								devicestream.Write(buffer, 0, count);
								bytesTransferred += count;
								Dispatcher.InvokeAsync(() => { ntripStatus.Text = $"{bytesTransferred} bytes"; });
							}
							catch(ObjectDisposedException)
                            {
								System.Diagnostics.Debug.WriteLine("Device stream was disposed");
								break;
							}
						}
					}
					ntripsocketstream.Dispose();
					Dispatcher.InvokeAsync(() => { ntripStatus.Text = "Status: Disconnected"; });
				});
			}
        }

		#endregion NTRIP

		#region Menu Click Handlers

		private void MenuItemFileExit_Click(object sender, RoutedEventArgs e)
		{
			App.Current.Shutdown();
		}

		private void MenuItemShowSkyView_Click(object sender, RoutedEventArgs e)
		{
			skyViewWindow.Show();
			skyViewWindow.Activate();
			skyViewWindow.Owner = this;
		}

		private void AutoPanOff_Click(object sender, RoutedEventArgs e)
        {
			AutoPanOffItem.IsChecked = true;
			AutoPanRecenterItem.IsChecked = false;
			AutoPanNavigationItem.IsChecked = false;
			restoreAutoModeBehavior.PanMode = mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Off;

		}

        private void AutoPanRecenter_Click(object sender, RoutedEventArgs e)
        {
			AutoPanOffItem.IsChecked = false;
			AutoPanRecenterItem.IsChecked = true;
			AutoPanNavigationItem.IsChecked = false;
			restoreAutoModeBehavior.PanMode = mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Recenter;
		}

        private void AutoPanNavigation_Click(object sender, RoutedEventArgs e)
		{
			AutoPanOffItem.IsChecked = false;
			AutoPanRecenterItem.IsChecked = false;
			AutoPanNavigationItem.IsChecked = true;
			restoreAutoModeBehavior.PanMode = mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Navigation;

		}
		#endregion Menu Click Handlers
	}
}
