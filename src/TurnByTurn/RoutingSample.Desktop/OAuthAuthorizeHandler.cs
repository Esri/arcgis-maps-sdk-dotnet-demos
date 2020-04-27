using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace RoutingSample
{
    // More about the IOAuthAuthorizeHandler can be found at the following sample:
    // https://github.com/Esri/arcgis-runtime-samples-dotnet/tree/master/src/WPF/ArcGISRuntime.WPF.Viewer/Samples/Security/OAuth
    public class OAuthAuthorizeHandler : IOAuthAuthorizeHandler
    {
        // A window to contain the OAuth UI.
        private Window _authWindow;

        // A TaskCompletionSource to track the completion of the authorization.
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // URL for the authorization callback result (the redirect URI configured for the application).
        private string _callbackUrl;

        // URL that handles the OAuth request.
        private string _authorizeUrl;

        // A function to handle authorization requests. It takes the URIs for the secured service, the authorization endpoint, and the redirect URI.
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource.Task has not completed, authorization is in progress.
            if (_taskCompletionSource != null || _authWindow != null)
            {
                // Allow only one authorization process at a time.
                throw new InvalidOperationException("Authorization is in progress.");
            }

            // Store the authorization and redirect URLs.
            _authorizeUrl = authorizeUri.AbsoluteUri;
            _callbackUrl = callbackUri.AbsoluteUri;

            // Create a task completion source to track completion.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Call a function to show the login controls, make sure it runs on the UI thread.
            Dispatcher dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                AuthorizeOnUIThread(_authorizeUrl);
            }
            else
            {
                Action authorizeOnUIAction = () => AuthorizeOnUIThread(_authorizeUrl);
                dispatcher.BeginInvoke(authorizeOnUIAction);
            }

            // Return the task associated with the TaskCompletionSource.
            return _taskCompletionSource.Task;
        }

        // A function to challenge for OAuth credentials on the UI thread.
        private void AuthorizeOnUIThread(string authorizeUri)
        {
            // Create a WebBrowser control to display the authorize page.
            WebBrowser authBrowser = new WebBrowser();

            // Handle the navigating event for the browser to check for a response sent to the redirect URL.
            authBrowser.Navigating += WebBrowserOnNavigating;

            // Display the web browser in a new window.
            _authWindow = new Window
            {
                Content = authBrowser,
                Height = 420,
                Width = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // Set the app's window as the owner of the browser window (if main window closes, so will the browser).
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                _authWindow.Owner = Application.Current.MainWindow;
            }

            // Handle the window closed event then navigate to the authorize url.
            _authWindow.Closed += OnWindowClosed;
            authBrowser.Navigate(authorizeUri);

            // Display the Window.
            if (_authWindow != null)
            {
                _authWindow.ShowDialog();
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            // If the browser window closes, return the focus to the main window.
            if (_authWindow != null && _authWindow.Owner != null)
            {
                _authWindow.Owner.Focus();
            }

            // If the task wasn't completed, the user must have closed the window without logging in.
            if (_taskCompletionSource != null && !_taskCompletionSource.Task.IsCompleted)
            {
                // Set the task completion to indicate a canceled operation.
                _taskCompletionSource.TrySetCanceled();
            }

            _taskCompletionSource = null;
            _authWindow = null;
        }

        // Handle browser navigation (page content changing).
        private void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // Check for a response to the callback url.
            WebBrowser webBrowser = sender as WebBrowser;
            Uri uri = e.Uri;

            // If no browser, uri, or an empty url return.
            if (webBrowser == null || uri == null || _taskCompletionSource == null || String.IsNullOrEmpty(uri.AbsoluteUri))
            {
                return;
            }

            // Check if the new content is from the callback url.
            bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl);

            if (isRedirected)
            {
                // Cancel the event to prevent it from being handled elsewhere.
                e.Cancel = true;

                // Get a local copy of the task completion source.
                TaskCompletionSource<IDictionary<string, string>> tcs = _taskCompletionSource;
                _taskCompletionSource = null;

                // Close the window.
                if (_authWindow != null)
                {
                    _authWindow.Close();
                }

                // Call a helper function to decode the response parameters (which includes the authorization key).
                IDictionary<string, string> authResponse = DecodeParameters(uri);

                // Set the result for the task completion source.
                tcs.SetResult(authResponse);
            }
        }

        // A helper function that decodes values from a querystring into a dictionary of keys and values.
        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string.
            string answer = "";

            // Get the values from the URI fragment or query string.
            if (!String.IsNullOrEmpty(uri.Fragment))
            {
                answer = uri.Fragment.Substring(1);
            }
            else
            {
                if (!String.IsNullOrEmpty(uri.Query))
                {
                    answer = uri.Query.Substring(1);
                }
            }

            // Parse parameters into key / value pairs.
            Dictionary<string, string> keyValueDictionary = new Dictionary<string, string>();
            string[] keysAndValues = answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string kvString in keysAndValues)
            {
                string[] pair = kvString.Split('=');
                string key = pair[0];
                string value = string.Empty;
                if (key.Length > 1)
                {
                    value = Uri.UnescapeDataString(pair[1]);
                }

                keyValueDictionary.Add(key, value);
            }

            // Return the dictionary of string keys/values.
            return keyValueDictionary;
        }
    }
}
