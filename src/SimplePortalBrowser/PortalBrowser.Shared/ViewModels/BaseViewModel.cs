using System.ComponentModel;

namespace PortalBrowser.ViewModels
{
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
