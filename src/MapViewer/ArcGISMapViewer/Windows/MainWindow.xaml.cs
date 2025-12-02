using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ArcGISMapViewer.Windows
{
    /// <summary>
    /// A map window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(MainWindowTitleBar);
            WindowManager.Get(this).PersistenceId = nameof(MainWindow);
            WindowManager.Get(this).MinWidth = 900;
            WindowManager.Get(this).MinHeight = 640;
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                this.AppWindow.TitleBar.InactiveBackgroundColor =
                    this.AppWindow.TitleBar.BackgroundColor =
                    this.AppWindow.TitleBar.ButtonBackgroundColor =
                    this.AppWindow.TitleBar.ButtonInactiveBackgroundColor = global::Windows.UI.Color.FromArgb(0, 19, 13, 78);
                //this.AppWindow.TitleBar.ForegroundColor =
                //this.AppWindow.TitleBar.ButtonForegroundColor =
                //    Microsoft.UI.Colors.White;
                //this.AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            }
            MainWindowTitleBar.RegisterPropertyChangedCallback(Microsoft.UI.Xaml.Controls.TitleBar.TitleProperty, (s, e) => UpdateWindowTitle() );
            MainWindowTitleBar.RegisterPropertyChangedCallback(Microsoft.UI.Xaml.Controls.TitleBar.SubtitleProperty, (s, e) => UpdateWindowTitle());
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            // This ensures the Title on the taskbar matches the window title, and narrator will read the correct title
            this.Title = MainWindowTitleBar.Title + " - " + MainWindowTitleBar.Subtitle;
        }

        public ApplicationViewModel AppVM => ApplicationViewModel.Instance;

        private async void SignOut_Click(object sender, RoutedEventArgs e)
        {
            await ApplicationViewModel.Instance.SignOut();
            var signinWindow = new Windows.StartupWindow();
            signinWindow.Activate();
            this.Close();
        }
    }
}
