using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Esri.ArcGISRuntime.Portal;

namespace OfflineWorkflowsSample.Models
{
    public class UserProfileModel : BindableBase
    {
        private ImageSource _profilePicture;
        public ImageSource ProfilePicture
        {
            get { return _profilePicture; }
            set { SetProperty(ref _profilePicture, value); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private string _fullName;
        public string FullName
        {
            get { return _fullName; }
            set { SetProperty(ref _fullName, value); }
        }

        public PortalUser User { get; set; }

        public UserProfileModel(PortalUser user)
        {
            Username = user.UserName;

            BitmapImage profilePicture = null;
            ProfilePicture = user.ThumbnailUri != null ? new BitmapImage(user.ThumbnailUri) : null;

            FullName = user.FullName;

            User = user;
        }
    }
}
