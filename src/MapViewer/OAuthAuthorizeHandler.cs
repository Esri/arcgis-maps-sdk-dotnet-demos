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
        var result = await WinUIEx.WebAuthenticator.AuthenticateAsync(authorizeUri, callbackUri).ConfigureAwait(false);
        return result.Properties;
    }
}
