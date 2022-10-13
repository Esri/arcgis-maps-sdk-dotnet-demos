using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PortalBrowser.ViewModels;

/// <summary>
/// Base View Model to be inherited by all view models in the project
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
	protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
	{
		if (Equals(field, value))
		{
			return;
		}

		field = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public event PropertyChangedEventHandler? PropertyChanged;
}
