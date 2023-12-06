using Esri.ArcGISRuntime.Location;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for SatelliteSnr.xaml
    /// </summary>
    public partial class SatelliteSnr : UserControl
    {
        public SatelliteSnr()
        {
            InitializeComponent();
        }
        
        public IReadOnlyList<Esri.ArcGISRuntime.Location.NmeaSatelliteInfo> SatelliteInfos
        {
            get => (IReadOnlyList<Esri.ArcGISRuntime.Location.NmeaSatelliteInfo>)GetValue(SatelliteInfosProperty);
            set => SetValue(SatelliteInfosProperty, value);
        }

        public static readonly DependencyProperty SatelliteInfosProperty =
            DependencyProperty.Register(nameof(SatelliteInfos), typeof(IReadOnlyList<Esri.ArcGISRuntime.Location.NmeaSatelliteInfo>), typeof(SatelliteSnr), new PropertyMetadata(null, OnSatelliteInfosPropertyChanged));

        private static void OnSatelliteInfosPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sats = e.NewValue as IReadOnlyList<Esri.ArcGISRuntime.Location.NmeaSatelliteInfo>;
            if (sats == null)
                ((SatelliteSnr)d).satellites.ItemsSource = null;
            else
                ((SatelliteSnr)d).satellites.ItemsSource = sats;
        }
    }
    
    public class SnrBar : Border
    {
        public SnrBar()
        {
            VerticalAlignment = VerticalAlignment.Bottom;
        }
        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(constraint.Width, constraint.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (SatelliteInfo is null)
                return new Size(0, 0);
            var snr = SatelliteInfo.Snr;
            var height = finalSize.Height / 100.0 * snr;
            return new Size(finalSize.Width, height);
        }

        public NmeaSatelliteInfo SatelliteInfo
        {
            get => (NmeaSatelliteInfo)GetValue(SatelliteInfoProperty);
            set => SetValue(SatelliteInfoProperty, value);
        }

        public static readonly DependencyProperty SatelliteInfoProperty =
            DependencyProperty.Register(nameof(SatelliteInfo), typeof(NmeaSatelliteInfo), typeof(SnrBar), new PropertyMetadata(null, (s, e) => ((SnrBar)s).OnSatelliteInfoPropertyChanged()));

        int snr = 0;
        private void OnSatelliteInfoPropertyChanged()
        {
            var newSnr = SatelliteInfo?.Snr ?? 0;
            if (newSnr != snr)
                InvalidateArrange();
            snr = newSnr;
        }
    }

    public class SatelliteIdToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return SignalIdToName(value as NmeaSatelliteInfo);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        internal static string SignalIdToName(NmeaSatelliteInfo? sat)
        {
            if (sat is null) return string.Empty;
            return $"{sat.System} {sat.Id}";
        }
    }
    public class SatelliteVechicleColorConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NmeaSatelliteInfo sat) {
                var color = GetColor(sat);
                if (targetType == typeof(Brush))
                    return new SolidColorBrush(color);
                return color;
            }
            return null;
        }
        public static Color GetColor(NmeaSatelliteInfo? value)
        { 
            if (value is NmeaSatelliteInfo sat)
            {
                byte alpha = (byte)(sat.Snr <= 0 || !sat.InUse ? 80 : 255);
                switch (sat.System)
                {
                    case NmeaGnssSystem.Gps: return Color.FromArgb(alpha, 255, 0, 0);
                    case NmeaGnssSystem.Galileo: return Color.FromArgb(alpha, 0, 255, 0);
                    case NmeaGnssSystem.Glonass: return Color.FromArgb(alpha, 0, 0, 255);
                    case NmeaGnssSystem.Bds: return Color.FromArgb(alpha, 0, 255, 255);
                    case NmeaGnssSystem.Qzss : return Color.FromArgb(alpha, 0, 0, 0);
                    default: return Colors.CornflowerBlue;
                }
            }
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
