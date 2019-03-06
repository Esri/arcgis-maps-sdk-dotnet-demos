using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Esri.ArcGISRuntime.Portal;
using Prism.Mvvm;

namespace OfflineWorkflowsSample.Models
{
    public class UserProfileModel : BindableBase
    {
        private ImageSource _profilePicture;

        public UserProfileModel(PortalUser user)
        {
            Portal = user.Portal;

            ProfilePicture = user.ThumbnailUri != null ? new BitmapImage(user.ThumbnailUri) : null;

            FullName = user.FullName;

            User = user;
        }

        public ImageSource ProfilePicture
        {
            get => _profilePicture;
            set => SetProperty(ref _profilePicture, value);
        }

        public ArcGISPortal Portal { get; }

        public string FullName { get; }

        public PortalUser User { get; }
    }
}