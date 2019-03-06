using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;

namespace OfflineWorkflowsSample
{
    public interface IWindowService
    {
        Task ShowAlertAsync(string message);
        Task ShowAlertAsync(string message, string title);
        void SetBusy(bool isBusy);
        void SetBusyMessage(string message);
        void NavigateToPageForItem(Item item);
        void NavigateToLoginPage();
        void LaunchItem(Item item);
    }
}