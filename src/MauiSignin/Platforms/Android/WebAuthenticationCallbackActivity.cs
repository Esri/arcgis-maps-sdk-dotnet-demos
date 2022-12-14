using Android.App;
using Android.Content.PM;

namespace MauiSignin.Platforms.Android;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { global::Android.Content.Intent.ActionView },
    Categories = new[] { global::Android.Content.Intent.CategoryDefault, global::Android.Content.Intent.CategoryBrowsable },
    DataScheme = AppSettings.OAuthRedirectScheme)]
public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
{
}