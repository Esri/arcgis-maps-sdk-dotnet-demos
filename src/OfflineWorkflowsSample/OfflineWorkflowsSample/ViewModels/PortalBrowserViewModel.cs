using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Windows.Mvvm;

namespace OfflineWorkflowSample.ViewModels
{
    public class PortalBrowserViewModel : ViewModelBase
    {
        private PortalViewModel _portalModel;
        private PortalFolderViewModel _selectedFolder;
        public IEnumerable<PortalFolderViewModel> Folders => _portalModel.Folders.Values;

        public PortalFolderViewModel SelectedFolder
        {
            get => _selectedFolder;
            set => SetProperty(ref _selectedFolder, value);
        }

        public void Initialize(PortalViewModel portalModel)
        {
            _portalModel = portalModel;
            SelectedFolder = _portalModel.FeaturedContent;
            RaisePropertyChanged(nameof(Folders));
            RaisePropertyChanged(nameof(SelectedFolder));
        }
    }
}
