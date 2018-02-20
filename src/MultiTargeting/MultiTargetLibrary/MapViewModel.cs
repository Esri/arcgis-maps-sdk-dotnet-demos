using System;
using System.ComponentModel;
using Esri.ArcGISRuntime.Mapping;

namespace MultiTargetLibrary
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public MapViewModel()
        {
            Map = new Map(Basemap.CreateLightGrayCanvasVector());
        }

        public Map Map { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
