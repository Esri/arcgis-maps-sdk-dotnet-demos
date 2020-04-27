using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Auth;

namespace RoutingSample.Forms.Droid
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

            // Get the current activity.
            var activity = MainActivity.Current;

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
                finally
                {
                    activity.FinishActivity(99);
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
                        activity.FinishActivity(99);
                    }
                }

                authenticator.OnCancelled();
            };

            // Present the authenticator UI
            var intent = authenticator.GetUI(activity);
            activity.StartActivityForResult(intent, 99);

            return _taskCompletionSource.Task;
        }
    }
}