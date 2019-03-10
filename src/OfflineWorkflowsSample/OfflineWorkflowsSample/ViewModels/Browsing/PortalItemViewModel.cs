using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using Prism.Windows.Mvvm;
using System;
using System.Threading;
#if WINDOWS_UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace OfflineWorkflowSample.ViewModels
{
    public class PortalItemViewModel : ViewModelBase
    {
        private readonly SynchronizationContext _context;

        public Item Item { get; private set; }

        public PortalItemViewModel (Item item)
        {
            Item = item;
            _context = SynchronizationContext.Current;
        }

        public string DisplayName => Item.Title;

        public string TypeString
        {
            get
            {
                switch (Item)
                {
                    case PortalItem portalItem:
                        return portalItem.TypeName;
                    case LocalItem localITem:
                        return localITem.Type.ToString();
                }

                return "Unknown";
            }
        }

        public string Owner
        {
            get
            {
                if (Item is PortalItem portalItem)
                {
                    return portalItem.Owner;
                }

                return "";
            }
        }

        public DateTimeOffset ModifiedDate => Item.Modified;

        private RuntimeImage _runtimeImage;

        private RuntimeImage ItemImage
        {
            get
            {
                if (_runtimeImage != null)
                {
                    return _runtimeImage;
                }

                if (Item.Thumbnail != null)
                {
                    _runtimeImage = Item.Thumbnail;

                    return _runtimeImage;
                }

                if (Item.ThumbnailUri != null)
                {
                    _runtimeImage = new RuntimeImage(Item.ThumbnailUri);
                    return _runtimeImage;
                }

                return _runtimeImage;
            }
        }

        #if WINDOWS_UWP
        private bool _imageLoaded;
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get
            {
                if (_thumbnail != null && _imageLoaded)
                {
                    return _thumbnail;
                }

                if (_thumbnail == null)
                {
                    _thumbnail = new BitmapImage(new Uri("ms-appx:///Assets/MapIcon.png"));
                }

                if (ItemImage == null)
                {
                    _imageLoaded = true;
                    return _thumbnail;
                }

                if (ItemImage.LoadStatus != LoadStatus.Loaded)
                {
                    ItemImage.Loaded += ItemImageOnLoadStatusChanged;

                    ItemImage.LoadAsync();
                }
                else
                {
                    // Context trickiness to make sure things happen on the UI thread.
                    _context.Post(UpdateImage, null);
                }

                return _thumbnail;
            }
            private set
            {
                _imageLoaded = true;
                SetProperty(ref _thumbnail, value);
            }
        }

        private void ItemImageOnLoadStatusChanged(object sender, EventArgs e)
        {
            // Context trickiness to make sure things happen on the UI thread.
            _context.Post(UpdateImage, null);
        }

        private async void UpdateImage(object state)
        {
            Thumbnail = await ItemImage.ToImageSourceAsync();
        }
        #endif
    }
}
