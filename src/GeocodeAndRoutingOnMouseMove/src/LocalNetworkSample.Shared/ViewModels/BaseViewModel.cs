using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LocalNetworkSample
{
	/// <summary>
	/// Base View Model object that raises property notification
	/// </summary>
	public abstract class BaseViewModel : INotifyPropertyChanged
	{

		/// <summary>
		/// Raises the property changed event
		/// </summary>
		/// <param name="propertyName">Name of the property that changed.</param>
		protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets a value indicating whether the app is currently running in design mode.
		/// This is useful for creating design-time data.
		/// </summary>
		/// <value>
		/// <c>true</c> if the app is design mode; otherwise, <c>false</c>.
		/// </value>
		protected bool IsDesignMode
		{
			get
			{
#if NETFX_CORE
				return Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#else
				return DesignerProperties.GetIsInDesignMode(System.Windows.Application.Current.MainWindow);
#endif
			}
		}

		protected
#if NETFX_CORE
 Windows.UI.Core.CoreDispatcher
#else
		System.Windows.Threading.Dispatcher
#endif
 Dispatcher
		{
			get
			{
#if NETFX_CORE
				return Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
#else	
				return System.Windows.Application.Current.MainWindow.Dispatcher;
#endif
			}
		}

		protected void Dispatch(Action action)
		{
#if NETFX_CORE
			var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => action());
#else
			Dispatcher.BeginInvoke(action);
#endif
		}
	}
}
