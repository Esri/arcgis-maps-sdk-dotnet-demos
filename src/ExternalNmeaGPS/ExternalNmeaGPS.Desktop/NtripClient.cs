using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ExternalNmeaGPS
{
    public sealed class NtripClient
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string? _auth;

        public NtripClient(string host, int port = 2101, string? username = null, string? password = null)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            if (port < 1)
                throw new ArgumentOutOfRangeException(nameof(port));
            _port = port;
            if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
                _auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
        }

        private Socket OpenSocket(string path)
        {
            var sckt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sckt.Blocking = true;
            sckt.ReceiveTimeout = 5000;
            sckt.Connect(_host, _port);
            string msg = $"GET /{path} HTTP/1.1\r\n";
            msg += "User-Agent: NTRIP MyNtripClientSample\r\n";
            if (_auth != null)
            {
                msg += "Authorization: Basic " + _auth + "\r\n";
            }
            msg += "Accept: */*\r\nConnection: close\r\n";
            msg += "\r\n";

            byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
            sckt.Send(data);
            return sckt;
        }

        public Task<IEnumerable<NtripStream>> GetAvailableStreamsAsync()
        {
            return Task.Run(() =>
            {
                string data = "";
                byte[] buffer = new byte[1024];
                using (var sck = OpenSocket(""))
                {
                    int count;
                    while ((count = sck.Receive(buffer)) > 0)
                    {
                        data += System.Text.Encoding.UTF8.GetString(buffer, 0, count);
                    }
                }
                var lines = data.Split('\n');
                List<NtripStream> streams = new List<NtripStream>();
                foreach (var item in lines)
                {
                    var d = item.Split(';');
                    if (d.Length == 0) continue;
                    if (d[0] == "ENDSOURCETABLE")
                        break;
                    else if (d[0] == "STR")
                    {
                        streams.Add(new NtripStream(d));
                    }
                }
                return streams.AsEnumerable();
            });
        }

        public Stream OpenStream(NtripStream stream) => OpenStream(stream?.Mountpoint ?? throw new ArgumentNullException(nameof(stream)));

        public Stream OpenStream(string mountPoint)
        {
            if (mountPoint == null)
                throw new ArgumentNullException(nameof(mountPoint));
            if (string.IsNullOrWhiteSpace(mountPoint))
                throw new ArgumentException(nameof(mountPoint));

            return new NtripDataStream(() => OpenSocket(mountPoint));
        }

        private class NtripDataStream : System.IO.Stream
        {
            private Func<Socket> m_openSocketAction;
            private Socket m_socket;

            public NtripDataStream(Func<Socket> openSocketAction)
            {
                m_openSocketAction = openSocketAction;
                m_socket = openSocketAction();
            }

            public override bool CanRead => m_socket.Connected;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => -1;

            long position = 0;
            public override long Position { get => position; set => throw new NotSupportedException(); }

            public override void Flush() => throw new NotSupportedException();

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (isDiposed)
                    throw new ObjectDisposedException("NTRIP Stream");
                if (!m_socket.Connected)
                {
                    // reconnect
                    System.Diagnostics.Debug.WriteLine("NTRIP Stream Disconnected. Reconnecting...");
                    m_socket.Dispose();
                    m_socket = m_openSocketAction();
                }
                int read = m_socket.Receive(buffer, offset, count, SocketFlags.None);
                position += read;
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                isDiposed = true;
                m_socket.Dispose();
                base.Dispose(disposing);
            }
            private bool isDiposed;

            public override int ReadTimeout { get => m_socket.ReceiveTimeout; set => m_socket.ReceiveTimeout = value; }
        }
    }

    public class NtripStream 
    {
        internal NtripStream(string[] d)
        {
            Mountpoint = d[1];
            Identifier = d[2];
            Format = d[3];
            FormatDetails = d[4];
            if (!int.TryParse(d[5], out int carrier))
                carrier = -1;
            switch (carrier)
            {
                case 1: Carrier = "L1"; break;
                case 2: Carrier = "L2"; break;
                default: Carrier = "None"; break;
            }
            Network = d[7];
            CountryCode = d[8];
            Latitude = double.Parse(d[9], CultureInfo.InvariantCulture);
            Longitude = double.Parse(d[10], CultureInfo.InvariantCulture);
            SupportsNmea = d[11] == "1";
        }

        public string Mountpoint { get; }
        public string Identifier { get; }
        public string Format { get; }
        public string FormatDetails { get; }
        public string Carrier { get; }
        public string Network { get; }
        public string CountryCode { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public bool SupportsNmea { get; }
        public override string ToString() => $"{Mountpoint} @ {Latitude},{Longitude}";
    }
}
