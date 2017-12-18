using Esri.ArcGISRuntime.Geometry;
using System;
using System.Threading.Tasks;

namespace ExternalNmeaGPS
{
	public class NmeaLocationProvider : Esri.ArcGISRuntime.Location.LocationDataSource
	{
		private NmeaParser.NmeaDevice device;
		double m_Accuracy = 0;

		public NmeaLocationProvider(NmeaParser.NmeaDevice device)
		{
			this.device = device;
			device.MessageReceived += device_MessageReceived;
		}

		void device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs e)
		{
			var message = e.Message;
			if (message is NmeaParser.Nmea.Gps.Garmin.Pgrme)
			{
				m_Accuracy = ((NmeaParser.Nmea.Gps.Garmin.Pgrme)message).HorizontalError;
			}
			else if (message is NmeaParser.Nmea.Gps.Gprmc)
			{
				var rmc = (NmeaParser.Nmea.Gps.Gprmc)message;
				if(rmc.Active)
				{
                    base.UpdateLocation(new Esri.ArcGISRuntime.Location.Location(new Esri.ArcGISRuntime.Geometry.MapPoint(rmc.Longitude, rmc.Latitude, SpatialReferences.Wgs84), m_Accuracy, rmc.Speed, rmc.Course, false));
				}
			}
		}

        protected override Task OnStartAsync()
        {
            return this.device.OpenAsync();
        }

        protected override Task OnStopAsync()
        {
            m_Accuracy = double.NaN;
            return this.device.CloseAsync();
        }
    }
}
