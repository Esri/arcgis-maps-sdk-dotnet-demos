namespace RoutingSample.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        /// <summary>
		/// Determines whether the application is executing in design mode.
		/// </summary>
		protected static bool IsDesignMode
        {
            get
            {
#if XAMARIN
                return Xamarin.Forms.DesignMode.IsDesignModeEnabled;
#elif WINDOWS_UWP
                return Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#elif WINDOWS_WPF
                return System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
#endif
            }
        }
    }
}
