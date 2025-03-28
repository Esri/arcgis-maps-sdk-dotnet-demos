using Microsoft.UI.Windowing;

namespace ArcGISMapViewer;

/// <summary>
/// Handles OAuth redirection to the system browser and re-activation for ArcGIS Authentication Handler.
/// </summary>
/// <remarks>
/// <para>To use with ArcGIS Runtime's OAuth callback, assign the static instance to the handler:
/// <code lang="csharp">AuthenticationManager.Current.OAuthAuthorizeHandler = OAuthAuthorizeHandler.Instance;</code>
/// </para>
/// </remarks>
public class OAuthAuthorizeHandler : Esri.ArcGISRuntime.Security.IOAuthAuthorizeHandler
{
    public static OAuthAuthorizeHandler Instance { get; } = new OAuthAuthorizeHandler();

    private Dictionary<string, TaskCompletionSource<Uri>> tasks = new Dictionary<string, TaskCompletionSource<Uri>>();

    private OAuthAuthorizeHandler()
    {
    }

    async Task<IDictionary<string, string>> Esri.ArcGISRuntime.Security.IOAuthAuthorizeHandler.AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
    {
        if (ApplicationViewModel.Instance.PortalUser is not null)
        {
            // We already signed in
            throw new UnauthorizedAccessException();
        }
        var result = await Microsoft.Security.Authentication.OAuth.OAuth2Manager.RequestAuthWithParamsAsync(
            new Microsoft.UI.WindowId(0), authorizeUri,
            Microsoft.Security.Authentication.OAuth.AuthRequestParams.CreateForAuthorizationCodeRequest("", callbackUri));
        return new Dictionary<string, string>() { { "code", result.Response.Code } };
    }
}
