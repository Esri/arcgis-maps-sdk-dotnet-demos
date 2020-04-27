using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PortalBrowser.ViewModels
{
    /// <summary>
    /// Base View Model to be inherited by all view models in the project.
    /// </summary>
	public abstract class BaseViewModel : INotifyPropertyChanged
	{
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
