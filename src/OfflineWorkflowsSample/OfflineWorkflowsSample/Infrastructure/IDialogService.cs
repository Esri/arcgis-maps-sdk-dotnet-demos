using System.Threading.Tasks;

namespace OfflineWorkflowsSample
{
    public interface IDialogService
    {
        Task ShowMessageAsync(string message);
    }
}