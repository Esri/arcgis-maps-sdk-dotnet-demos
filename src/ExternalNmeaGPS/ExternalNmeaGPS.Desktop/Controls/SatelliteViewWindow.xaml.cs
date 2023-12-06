using System.Collections.Generic;
using System.Windows;

namespace ExternalNmeaGPS.Controls
{
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

        public IReadOnlyList<Esri.ArcGISRuntime.Location.NmeaSatelliteInfo> SatelliteInfos
        {
            get { return satView.SatelliteInfos; }
            set { satView.SatelliteInfos = value; snrView.SatelliteInfos = value; }
        }
    }
}