#if NETCOREAPP3_1
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;

namespace ExternalNmeaGPS
{
	// Custom Serial Port Location Datasource
	// You can also use NmeaLocationDataSource.FromStreamCreator method to achieve the same thing without a custom class.

	public class SerialPortLocationDataSource : NmeaLocationDataSource
	{
		private readonly System.IO.Ports.SerialPort port;

		public SerialPortLocationDataSource(string portName, int baudRate, SpatialReference? receiverSR = null) : base(receiverSR ?? SpatialReferences.Wgs84)
		{
			port = new System.IO.Ports.SerialPort(portName, baudRate);
		}

		protected override Task OnStartAsync()
		{
			port.Open();
			NmeaDataStream = port.BaseStream; // Must be set before calling base.OnStartAsync
			return base.OnStartAsync();
		}

		protected override async Task OnStopAsync()
		{
			await base.OnStopAsync(); // Wait for read thread to stop
			NmeaDataStream = null;
			port.Close();
		}
	}
}
#endif