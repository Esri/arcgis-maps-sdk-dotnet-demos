using Esri.ArcGISRuntime.Portal;
using System.ComponentModel;

namespace MauiSignin;

internal class AppSettings : ModelBase
{
    private AppSettings()
    {
        
    }
    public static AppSettings Instance { get; } = new AppSettings();
    
    public static Uri PortalUri { get; } = new Uri("https://www.arcgis.com/sharing/rest");

    public static string OAuthClientId { get; } = "SET_CLIENT_ID";

    // Also update 'mauisignin' scheme in 
    //   ./Platforms/iOS/Info.plist
    //   ./Platforms/Windows/Package.appxmanifest
    public const string OAuthRedirectScheme = "mauisignin";

    public static Uri OAuthRedirectUri { get; } = new Uri(OAuthRedirectScheme + "://SET_REDIRECT_URL");

    public ArcGISPortal? Portal => _PortalUser?.Portal;
    
    public ImageSource? PortalUserThumbnail { get; private set; }

    private PortalUser? _PortalUser;

    public PortalUser? PortalUser
    {
        get { return _PortalUser; }
        private set
        {
            _PortalUser = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Portal));
        }
    }
    
    public void SetUser(PortalUser? value)
    {
        PortalUser = value;
        if (value != null)
        {
            if (value.ThumbnailUri == null)
            {
                PortalUserThumbnail = null;
            }
            else
            {
                PortalUserThumbnail = ImageSource.FromStream((c) => value.GetThumbnailDataAsync(c));
            }
            _ = RefreshMaps(value.Portal);
        }
        else
        {
            PortalUserThumbnail = null;
            MapItems = null;
            OnPropertyChanged(nameof(MapItems));
        }
        OnPropertyChanged(nameof(PortalUserThumbnail));
    }

    private async Task RefreshMaps(ArcGISPortal value)
    {
        try
        {
            var result = await value.FindItemsAsync(PortalQueryParameters.CreateForItemsOfTypes(new PortalItemType[] { PortalItemType.WebMap }));
            MapItems = result.Results;
            OnPropertyChanged(nameof(MapItems));
        }
        catch
        {
        }
    }

    public IEnumerable<PortalItem>? MapItems { get; private set; }

    public async Task SignOut()
    {
        SetUser(null);
        OnPropertyChanged(nameof(Portal));
        SecureStorage.Remove("License");
        await Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
    }
}
