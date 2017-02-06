using System.ComponentModel;

namespace PortalBrowser.ViewModels
{
    /// <summary>
    /// Base View Model to be inherited by all view models in the project
    /// </summary>
	public abstract class BaseViewModel : INotifyPropertyChanged
	{
		
		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
