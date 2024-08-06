using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using ArcGISMapViewer.ViewModels;
using Esri.ArcGISRuntime.Portal;
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
    /// Startup / signin window
    /// </summary>
    public sealed partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            this.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            this.InitializeComponent();
            var manager = WindowManager.Get(this);
            manager.Width = 640;
            manager.Height = 400;
            manager.IsMaximizable = false;
            manager.IsMinimizable = false;
            manager.IsResizable = false;
            this.Closed += (s, e) => SigninTask?.TrySetCanceled();
            this.Closed += SignInWindow_Closed;
            WinUIEx.WindowExtensions.SetIcon(this, new Microsoft.UI.IconId() { Value = 0 });
            this.CenterOnScreen();

            InitializeApp();
        }

        private async void InitializeApp()
        {
            AppInitializer initializer = new AppInitializer();
            initializer.ProgressChanged += (s, progress) => this.progress.Value = progress;
            initializer.StatusTextChanged += (s, status) => this.status.Text = status;
            initializer.SigninRequested += Initializer_SigninRequested;
            initializer.ApplicationInitialized += Initializer_ApplicationInitialized;
            try
            {
                await initializer.Initialize();
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch (System.Exception ex)
            {
                var dialog = WindowExtensions.CreateMessageDialog(this, "Error initializing", ex.Message);
                await dialog.ShowAsync();
                this.Close();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        private void Initializer_ApplicationInitialized(object sender, EventArgs e)
        {
            var window = new MainWindow();
            window.Show();
            this.Close();
            window.SetForegroundWindow();
        }

        private TaskCompletionSource<PortalUser>? SigninTask;
        private void Initializer_SigninRequested(object sender, TaskCompletionSource<PortalUser> e)
        {
            SigninTask = e;
            LoadingSection.Visibility = Visibility.Collapsed;
            SigninSection.Visibility = Visibility.Visible;
        }

        private async Task SignIn()
        {
            var serviceUri = ApplicationViewModel.Instance.AppSettings.PortalUrl;

            try
            {
                signinstatus.Text = "Waiting for sign in... Check your browser.";
                ArcGISPortal arcgisPortal = await ArcGISPortal.CreateAsync(serviceUri, true);
                SigninTask?.TrySetResult(arcgisPortal.User!);
                signinstatus.Text = string.Empty;
            }
            catch (OperationCanceledException) {
                SigninTask?.TrySetCanceled();
            }
            catch (Exception ex)
            {
                signinstatus.Text = "Failed to sign in: " + ex.Message;
            }
            WindowExtensions.SetForegroundWindow(this);
        }

        private void SignInWindow_Closed(object sender, WindowEventArgs args)
        {
            SigninTask?.TrySetCanceled();
        }
        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            _ = SignIn();
        }
    }
}
