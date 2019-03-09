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
    public class PortalItemViewModel<T> : ViewModelBase where T : Item
    {
        private SynchronizationContext _context;
        private T _item;

        public T Item
        {
            get => _item;
        }

        public PortalItemViewModel (T item)
        {
            _item = item;
            _context = SynchronizationContext.Current;
        }

        public string DisplayName => _item.Title;

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

                ItemImage.LoadStatusChanged += ItemImageOnLoadStatusChanged;

                ItemImage.LoadAsync();

                return _thumbnail;
            }
            set
            {
                _imageLoaded = true;
                SetProperty(ref _thumbnail, value);
            }
        }

        private void ItemImageOnLoadStatusChanged(object sender, LoadStatusEventArgs e)
        {
            if (e.Status == LoadStatus.Loaded)
            {
                // Context trickiness to make sure things happen on the UI thread.
                _context.Post(UpdateImage, null);
            }
        }

        private async void UpdateImage(object state)
        {
            Thumbnail = await ItemImage.ToImageSourceAsync();
        }
        #endif
    }
}
