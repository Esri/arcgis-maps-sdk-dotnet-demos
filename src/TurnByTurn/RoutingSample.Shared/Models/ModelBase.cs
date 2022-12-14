namespace RoutingSample
{
	/// <summary>
	/// Base class used for bindable objects
	/// </summary>
	public abstract class ModelBase : ObservableObject
	{
        /// <summary>
        /// Determines whether the application is executing in design mode.
        /// </summary>
        protected static bool IsDesignMode
        {
            get
            {
#if MAUI
                return false;
#elif NETFX_CORE
                return Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#else
                return System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
#endif
            }
        }
    }
}
