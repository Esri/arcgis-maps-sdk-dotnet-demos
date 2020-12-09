using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
		private string? currentNmeaFile;

		public class Device
        {
			public string? Name { get; set; }
#if (NET5_0 || NET5_0_OR_GREATER) && WINDOWS
			public DeviceInformation? DeviceInfo { get; set; }
#endif
			public override string ToString() => Name!;
        }
		public MainWindow()
		{
			InitializeComponent();
			currentNmeaFile = "NmeaSampleData.txt";
			var device = NmeaLocationDataSource.FromStreamCreator(
				(s) => Task.FromResult<Stream>(new BufferedFileStream(new System.IO.StreamReader(currentNmeaFile), new BurstEmulationSettings())));
			LoadDevice(device);
			LoadPorts();
		}
#if (NET5_0 || NET5_0_OR_GREATER) && WINDOWS
		// Use WinRT APIs instead
		public async void LoadPorts()
		{
			var ports = System.IO.Ports.SerialPort.GetPortNames();
			PortsList.ItemsSource = ports;
			if (ports.Any())
				PortsList.SelectedIndex = 0;
			//Get list of bluetooth devices 
			string serialDeviceType = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
			var devices = await DeviceInformation.FindAllAsync(serialDeviceType); // Select a bluetooth serial port from this list 
			List<Device> list = new List<Device>(devices.Select(t => new Device() { DeviceInfo = t, Name = t.Name }));
			// Get a list of serial devices
			devices = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
			list.AddRange(devices.Select(t => new Device() { DeviceInfo = t, Name = t.Name }));
			PortsList.ItemsSource = list;
		}
#else
		public void LoadPorts()
		{ 
			var ports = System.IO.Ports.SerialPort.GetPortNames();
		 	PortsList.ItemsSource = ports.Select(p=> new Device() { Name = p });
			if(ports.Any())
				PortsList.SelectedIndex = 0;
		}
#endif

		private void LoadDevice(NmeaLocationDataSource device)
		{
			if (mapView.LocationDisplay.DataSource != null)
				mapView.LocationDisplay.DataSource.LocationChanged -= Device_LocationChanged;

			currentNmeaDevice = device;
			mapView.LocationDisplay.DataSource = currentNmeaDevice;
			mapView.LocationDisplay.IsEnabled = true;
			mapView.LocationDisplay.DataSource.LocationChanged += Device_LocationChanged;
            device.SatellitesChanged += Device_SatellitesChanged;
		}

        private void Device_LocationChanged(object? sender, Esri.ArcGISRuntime.Location.Location e)
		{
			Dispatcher.BeginInvoke((Action)delegate()
			{
				//Zoom in on first location fix
				((LocationDataSource)sender!).LocationChanged -= Device_LocationChanged;
				mapView.SetViewpointCenterAsync(e.Position, 5000);
				mapView.LocationDisplay.AutoPanMode = Esri.ArcGISRuntime.UI.LocationDisplayAutoPanMode.Navigation;
			});
		}

		private void Device_SatellitesChanged(object? sender, IReadOnlyList<NmeaSatelliteInfo> satellites)
		{
			Dispatcher.BeginInvoke((Action)delegate ()
			{
				skyViewWindow.SatelliteInfos = satellites;
			});
		}

		private void MenuItemShowSkyView_Click(object sender, RoutedEventArgs e)
		{
			skyViewWindow.Show();
			skyViewWindow.Activate();
			skyViewWindow.Owner = this;
		}

		private async void SerialPortOpen_Click(object sender, RoutedEventArgs e)
		{
			if(PortsList.SelectedItem != null)
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
				});
#endif
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
                mapView.GraphicsOverlays!.First().Graphics.Clear();
				var device = NmeaLocationDataSource.FromStreamCreator(
					(s) => Task.FromResult<Stream>(new BufferedFileStream(new System.IO.StreamReader(currentNmeaFile), new BurstEmulationSettings())));
				LoadDevice(device);
				SelectedFileName.Text = new System.IO.FileInfo(currentNmeaFile).Name;
			}
		}

        private void MenuItemFileExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
	}
}
