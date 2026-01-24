using System;
using System.Drawing;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Esri.ArcGISRuntime.Tasks.Offline;
using Prism.Mvvm;

namespace OfflineWorkflowsSample.Models
{
    public class MapAreaModel : BindableBase
    {
        // Random number generator for getting colors
        private static readonly Random Rng = new Random();
        private Color? _displayColor;

        private ImageSource _thumbnail;

        public MapAreaModel(PreplannedMapArea mapArea)
        {
            MapArea = mapArea;

            Thumbnail = mapArea.PortalItem.ThumbnailUri != null ? new BitmapImage(mapArea.PortalItem.ThumbnailUri) : null;
        }

        public Color DisplayColor
        {
            get
            {
                if (_displayColor == null)
                    _displayColor = Color.FromArgb(Rng.Next(0, 255), Rng.Next(0, 255), Rng.Next(0, 255));
                return _displayColor.Value;
            }
        }

        public ImageSource Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public string Title => MapArea.PortalItem.Title;

        public string Description => MapArea.PortalItem.Description;

        public string Snippet => MapArea.PortalItem.Snippet;

        public DateTimeOffset Updated => MapArea.PortalItem.Modified;

        public PreplannedMapArea MapArea { get; }
    }
}