using System.Threading.Tasks;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowSample.ViewModels;

namespace OfflineWorkflowsSample
{
    public interface IWindowService
    {
        Task ShowAlertAsync(string message);
        Task ShowAlertAsync(string message, string title);
        void SetBusy(bool isBusy);
        void SetBusyMessage(string message);
        void NavigateToPageForItem(PortalItemViewModel item);
        void NavigateToLoginPage();
        void LaunchItem(Item item);
    }
}