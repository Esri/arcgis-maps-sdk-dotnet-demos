using Esri.ArcGISRuntime.Security;

namespace MauiSignin;

public class OAuthAuthorizeHandler : IOAuthAuthorizeHandler
{
    private OAuthAuthorizeHandler() { }

    public static OAuthAuthorizeHandler Instance { get; } = new OAuthAuthorizeHandler();

    public async Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
    {
#if WINDOWS
        var result = await WinUIEx.WebAuthenticator.AuthenticateAsync(authorizeUri, callbackUri);
#else
        var result = await WebAuthenticator.AuthenticateAsync(new WebAuthenticatorOptions()
        {
            CallbackUrl = callbackUri,
            Url = authorizeUri
        });
#endif
        return result.Properties;
    }
}