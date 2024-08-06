using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Portal;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace ArcGISMapViewer.ViewModels;

public partial class ApplicationViewModel : ObservableObject
{
    public static ApplicationViewModel Instance { get; } = new ApplicationViewModel();

    private ApplicationViewModel()
    {
        _ = LocationDataSource.StartAsync();
    }

    [ObservableProperty]
    private LocationDataSource _LocationDataSource = LocationDataSource.CreateDefault();

    public AppSettings AppSettings { get; } = new AppSettings();

    [ObservableProperty]
    private Map _map = new Map(BasemapStyle.ArcGISStreets);

    [ObservableProperty]
    private string _windowSubTitle;

    private PortalUser? _PortalUser;

    public PortalUser? PortalUser
    {
        get { return _PortalUser; }
        private set
        {
            _PortalUser = value; OnPropertyChanged();
            AppSettings.PortalUser = value?.FullName;
        }
    }

    [ObservableProperty]
    private bool _isMapVisible;
    [ObservableProperty]
    private bool _isAppMenuVisible;
    partial void OnIsAppMenuVisibleChanged(bool value)
    {
        IsMapVisible = !value;
    }
    partial void OnIsMapVisibleChanged(bool value)
    {
        IsAppMenuVisible = !value;
        if (value)
        {
            this.WindowSubTitle = Map?.Item?.Title ?? string.Empty;
        }
    }

    public async Task SetUserAsync(PortalUser? value)
    {
        PortalUserThumbnail = null;
        OnPropertyChanged(nameof(PortalUserThumbnail));
        PortalUser = value;
        if (value != null)
        {
            try
            {
                await RefreshUserThumbnail(value);
            }
            catch { }
        }
    }

    private async Task RefreshUserThumbnail(PortalUser value)
    {
        try
        {
            using var stream = await value.GetThumbnailDataAsync();
            if (stream is not null)
            {
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                PortalUserThumbnail = bitmap;
            }
            else
            {
                PortalUserThumbnail = null;
            }
            OnPropertyChanged(nameof(PortalUserThumbnail));
        }
        catch
        {
        }
    }

    public ImageSource? PortalUserThumbnail { get; private set; }

    public async Task SignOut()
    {
        PortalUser = null;
        await Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
    }
}
