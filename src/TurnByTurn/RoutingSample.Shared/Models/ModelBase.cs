using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingSample
{
	/// <summary>
	/// Base class used for bindable objects
	/// </summary>
	public abstract class ModelBase : INotifyPropertyChanged
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ModelBase"/> class.
		/// </summary>
		protected ModelBase() { }

		protected void RaisePropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		protected void RaisePropertiesChanged(IEnumerable<string> propertyNames)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				foreach (var propertyName in propertyNames)
					handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Helper property to use for populating design time data
		/// </summary>
		protected static bool IsDesignMode 
		{ 
			get 
			{
#if WINDOWS_PHONE
				return 
					System.Windows.Application.Current.RootVisual != null &&
					System.ComponentModel.DesignerProperties.GetIsInDesignMode(System.Windows.Application.Current.RootVisual);
#elif NETFX_CORE
				return Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#else
				return false;
#endif
			}
		}
	}
}
