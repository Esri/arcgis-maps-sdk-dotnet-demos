using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Auth;
using Xamarin.Forms;

namespace RoutingSample.Forms.iOS
{
    public class OAuthAuthorizeHandler : IOAuthAuthorizeHandler
    {
        private readonly string _clientId;
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        public OAuthAuthorizeHandler(string clientId)
        {
            _clientId = clientId;
        }

        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // Create a new task completion source.
            if (_taskCompletionSource != null)
                _taskCompletionSource.TrySetCanceled();

            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Get the current ViewController.
            UIViewController viewController = null;
            Device.BeginInvokeOnMainThread(() =>
            {
                viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            });

            // Create a new OAuth2 authenticator.
            var authenticator = new OAuth2Authenticator(
                clientId: _clientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri)
            {
                ShowErrors = false,
                AllowCancel = true
            };

            authenticator.Completed += (sender, e) =>
            {
                try
                {
                    viewController.DismissViewController(true, null);

                    if (e.IsAuthenticated)
                    {
                        _taskCompletionSource.SetResult(e.Account.Properties);
                    }
                    else
                    {
                        throw new Exception("Failed to authenticate.");
                    }
                }
                catch (Exception ex)
                {
                    _taskCompletionSource.TrySetException(ex);
                    authenticator.OnCancelled();
                }
            };

            authenticator.Error += (sender, e) =>
            {
                if (e.Exception != null)
                {
                    _taskCompletionSource.TrySetException(e.Exception);
                }
                else
                {
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                    }
                }

                authenticator.OnCancelled();
            };

            // Present the authenticator UI
            Device.BeginInvokeOnMainThread(() =>
            {
                viewController.PresentViewController(authenticator.GetUI(), true, null);
            });

            return _taskCompletionSource.Task;
        }
    }
}