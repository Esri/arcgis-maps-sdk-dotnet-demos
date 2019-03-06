using Esri.ArcGISRuntime.Tasks.Offline;
using Prism.Mvvm;
using System;
using System.Drawing;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace OfflineWorkflowsSample.Models
{
    public class MapAreaModel : BindableBase
    {
        // Random number generator for getting colors
        private static Random _rng = new Random();
        private Color? _displayColor;

        public Color DisplayColor
        {
            get
            {
                if (_displayColor == null)
                {
                    SetProperty(ref _displayColor,
                        Color.FromArgb(_rng.Next(0, 255), _rng.Next(0, 255), _rng.Next(0, 255)));
                }

                return _displayColor.Value;
            }
        }

        private PreplannedMapArea _mapArea;

        public MapAreaModel(PreplannedMapArea mapArea)
        {
            _mapArea = mapArea;

            if (mapArea.PortalItem.ThumbnailUri != null)
                Thumbnail = new BitmapImage(mapArea.PortalItem.ThumbnailUri);
            else
                Thumbnail = null;
        }

        private ImageSource _thrumbnail;

        public ImageSource Thumbnail
        {
            get { return _thrumbnail; }
            set { SetProperty(ref _thrumbnail, value); }
        }

        public string Title => _mapArea.PortalItem.Title;

        public string Description => _mapArea.PortalItem.Description;

        public string Snippet => _mapArea.PortalItem.Snippet;

        public DateTimeOffset Updated => _mapArea.PortalItem.Modified;

        public PreplannedMapArea GetArea => _mapArea;
    }
}