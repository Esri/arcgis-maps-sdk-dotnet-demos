using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample.Infrastructure;
using Prism.Windows.Mvvm;

namespace OfflineWorkflowSample
{
    public class PortalViewModel : ViewModelBase
    {
        public string Title { get; private set; }
        public IEnumerable<PortalItem> Items { get; set; }
        public IList<PortalViewModel> Groups { get; private set; } = new List<PortalViewModel>();
        private IEnumerable<PortalItem> _featured;

        public IEnumerable<PortalItem> Featured
        {
            get => _featured;
            set => SetProperty(ref _featured, value);
        }

        public PortalViewModel MyContent { get; private set; }
        public ArcGISPortal Portal { get; set; }

        public bool HasFeaturedItems => Featured.Any();

        private PortalViewModel(){}

        private PortalViewModel(PortalFolder folder, IEnumerable<PortalItem> items)
        {
            this.Portal = folder.Portal;
            this.Title = folder.Title;
            this.Items = items;
        }

        private PortalViewModel(PortalGroup group, IEnumerable<PortalItem> items)
        {
            this.Portal = group.Portal;
            this.Title = group.Title;
            this.Items = items;
        }

        public static async Task<PortalViewModel> GetRootVM(ArcGISPortal portal, bool hasMyContent, bool hasMyGroups)
        {
            PortalViewModel resultModel = new PortalViewModel();
            resultModel.Title = "Root";
            resultModel.Portal = portal;
            if (hasMyContent)
            {
                // Get the 'my content' group
                var result = await portal.User.GetContentAsync();
                PortalViewModel myContentModel = new PortalViewModel();
                myContentModel.Portal = portal;
                myContentModel.Title = "My Content";
                myContentModel.Items = result.Items.Where(item => item.Type == PortalItemType.WebMap && item.TypeKeywords.Contains("Offline"));
                myContentModel.Groups = new ObservableCollection<PortalViewModel>();
                foreach (var folder in result.Folders)
                {
                    var items = await portal.User.GetContentAsync(folder.FolderId);
                    items = items.Where(item => item.Type == PortalItemType.WebMap);
                    if (!items.Any())
                    {
                        continue;
                    }

                    myContentModel.Groups.Add(new PortalViewModel(folder, items));
                }

                resultModel.MyContent = myContentModel;
            }

            if (hasMyGroups)
            {
                // Get the groups
                foreach (var item in portal.User.Groups)
                {
                    PortalQueryParameters parameters = PortalQueryParameters.CreateForItemsOfTypeInGroup(PortalItemType.WebMap, item.GroupId);
                    var itemResults = await portal.FindItemsAsync(parameters);
                    var filteredResults = itemResults.Results.Where(resultItem => resultItem.TypeKeywords.Contains("Offline")).ToList();
                    if (!filteredResults.Any())
                    {
                        continue;
                    }

                    PortalViewModel groupModel = new PortalViewModel(item, filteredResults);
                    resultModel.Groups.Add(groupModel);
                }
            }

            // Populate featured
            resultModel.Featured = (await portal.GetFeaturedItemsAsync()).Where(item => item.Type == PortalItemType.WebMap);
            return resultModel;
        }
    }
}