﻿using System;
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
            WindowManager.Get(this).PersistenceId = nameof(MainWindow);
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
        }

        public ApplicationViewModel AppVM => ApplicationViewModel.Instance;

        private async void SignOut_Click(object sender, RoutedEventArgs e)
        {
            await ApplicationViewModel.Instance.SignOut();
            var signinWindow = new Windows.StartupWindow();
            signinWindow.Activate();
            this.Close();
        }

        //private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
        //{
        //    if (MapPage.Visibility == Visibility.Visible)
        //    {
        //        MapPage.Visibility = Visibility.Collapsed;
        //        HomePage.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        MapPage.Visibility = Visibility.Visible;
        //        HomePage.Visibility = Visibility.Collapsed;
        //        AppVM.WindowSubTitle = AppVM.Map?.Item?.Title ?? String.Empty;
        //    }
        //}
    }
}