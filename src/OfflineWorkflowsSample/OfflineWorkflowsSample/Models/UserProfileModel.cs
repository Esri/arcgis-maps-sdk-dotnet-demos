using Windows.UI.Xaml.Media;
using Esri.ArcGISRuntime.Portal;
using Prism.Mvvm;
using Esri.ArcGISRuntime.UI;
using System.Threading.Tasks;

namespace OfflineWorkflowsSample.Models
{
    public class UserProfileModel : BindableBase
    {
        private ImageSource _profilePicture;

        public UserProfileModel(PortalUser user)
        {
            Portal = user.Portal;

            FullName = user.FullName;

            User = user;

            _ = LoadProfilePicture(user);
        }

        private async Task LoadProfilePicture(PortalUser user)
        {
            ProfilePicture = await user.Thumbnail.ToImageSourceAsync();
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