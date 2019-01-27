using System.Threading.Tasks;

namespace OfflineWorkflowsSample
{
    public interface IWindowService
    {
        Task ShowAlertAsync(string message);
        Task ShowAlertAsync(string message, string title);
        void SetBusy(bool isBusy);
        void SetBusyMessage(string message);
    }
}