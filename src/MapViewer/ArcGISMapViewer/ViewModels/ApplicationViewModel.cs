﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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

    public async Task<GeoModel?> LoadLastMapAsync()
    {
        var item = await AppSettings.GetLastPortalItemAsync();
        if(item != null)
        {
            PortalItem = item;
            return Map as GeoModel ?? Scene as GeoModel;
        }
        else
        {
            GeoModel = new Map(BasemapStyle.ArcGISStreets);
            return null;
        }
    }

    [ObservableProperty]
    public partial LocationDataSource LocationDataSource { get; set; } = LocationDataSource.CreateDefault();

    public AppSettings AppSettings { get; } = new AppSettings();

    [ObservableProperty]
    public partial GeoModel? GeoModel { get; set; }

    partial void OnGeoModelChanged(GeoModel? oldValue, GeoModel? newValue)
    {
        if (newValue is Map || oldValue is Map)
            OnPropertyChanged(nameof(Map));
        if (newValue is Scene || oldValue is Scene)
            OnPropertyChanged(nameof(Scene));
        if (oldValue is Map && newValue is Scene || oldValue is Scene && newValue is Map)
        {
            OnPropertyChanged(nameof(Is2D));
            OnPropertyChanged(nameof(Is3D));
        }
    }

    public Map? Map => GeoModel as Map;
    public Scene? Scene => GeoModel as Scene;

    public bool Is2D => GeoModel is Map;
    public bool Is3D => GeoModel is Scene;

    [ObservableProperty]
    public partial PortalItem? PortalItem { get; set; }

    partial void OnPortalItemChanged(PortalItem? value)
    {
        if (value is not null)
        {
            Scene? scene = null;
            var map = new Map(BasemapStyle.ArcGISStreets);
            if(value.Type == PortalItemType.WebScene)
                scene = new Scene(value);
            else if (value.Type == PortalItemType.WebMap)
                map = new Map(value);
            else if(value.Type == PortalItemType.FeatureService || value.Type == PortalItemType.WFS)
                map.OperationalLayers.Add(new FeatureLayer(value));
            else if (value.Type == PortalItemType.WMS)
                map.OperationalLayers.Add(new WmsLayer(value));
            else if (value.Type == PortalItemType.KML)
                map.OperationalLayers.Add(new KmlLayer(value));
            else if (value.Type == PortalItemType.VectorTileService)
                map.OperationalLayers.Add(new ArcGISVectorTiledLayer(value));
            else if (value.Type == PortalItemType.MapService)
                map.OperationalLayers.Add(new ArcGISMapImageLayer(value));
            else if (value.Type == PortalItemType.FeatureCollection)
                map.OperationalLayers.Add(new FeatureCollectionLayer(new Esri.ArcGISRuntime.Data.FeatureCollection(value)));
            else if(value.Type == PortalItemType.SceneService)
            {
                scene = new Scene();
                scene.OperationalLayers.Add(new ArcGISSceneLayer(value));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"{value.Type} not implemented");
            }
            if (value.Extent != null)
            {
                if(scene != null)
                    scene.InitialViewpoint = new Viewpoint(value.Extent);
                else
                    map.InitialViewpoint = new Viewpoint(value.Extent);
            }
            GeoModel = scene is null ? map : scene;
        }
        AppSettings.SetLastPortalItem(value);
    }

    [ObservableProperty]
    public partial string WindowSubTitle { get; set; } = string.Empty;

    private PortalUser? _PortalUser;

    public PortalUser? PortalUser
    {
        get { return _PortalUser; }
        private set
        {
            _PortalUser = value; OnPropertyChanged();
            OnPropertyChanged(nameof(Favorites));
            AppSettings.PortalUser = value?.FullName;
        }
    }

    [ObservableProperty]
    public partial bool IsMapVisible { get; set; }
    [ObservableProperty]
    public partial bool IsAppMenuVisible { get; set; } = true;
    partial void OnIsAppMenuVisibleChanged(bool value)
    {
        IsMapVisible = !value;
    }
    partial void OnIsMapVisibleChanged(bool value)
    {
        IsAppMenuVisible = !value;
        if (value)
        {
            this.WindowSubTitle = GeoModel?.Item?.Title ?? string.Empty;
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

    private IList<PortalItem>? _Favorites;
    public IList<PortalItem> Favorites
    {
        get
        {
            if (_Favorites is null && this.PortalUser?.Portal != null)
            {
                LoadFavorites();
            }
            return _Favorites ?? new List<PortalItem>(0);
        }
    }

    private void LoadFavorites()
    {
        if (!string.IsNullOrEmpty(this.PortalUser?.FavoritesGroupId))
        {
            var group = new PortalGroup(this.PortalUser.Portal, this.PortalUser.FavoritesGroupId);
            _Favorites = new PortalPageViewModel.PortalGroupItemQuerySource(group, new PortalGroupContentSearchParameters(""));
            OnPropertyChanged(nameof(Favorites));
        }
    }

    internal async void AddToFavorites(PortalItem item)
    {
        try
        {
            if (PortalUser is null) return;
            await PortalUser.AddToFavoritesAsync(item);
            if (_Favorites is PortalPageViewModel.PortalGroupItemQuerySource source)
            {
                if (!source.HasMoreItems)
                    _Favorites.Add(item);
                else
                    LoadFavorites();
            }
        }
        catch
        {
        }
    }

    internal async void RemoveFromFavorites(PortalItem item)
    {
        try
        {
            if (PortalUser is null) return;
            await PortalUser.RemoveFromFavoritesAsync(item);
            if (_Favorites is PortalPageViewModel.PortalGroupItemQuerySource source)
            {
                if (source.Where(i => i.ItemId == item.ItemId).FirstOrDefault() is PortalItem removedItem)
                {
                    _Favorites.Remove(removedItem);
                }
            }
        }
        catch
        {
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
        _Favorites = null;
        AppSettings.SetLastPortalItem(null);
        await Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
    }

    internal async Task AddDataFromFileAsync(string path)
    {
        if (GeoModel is null) return;
        var info = new FileInfo(path);
        try
        {
            Layer? layer;
            switch (info.Extension.ToLowerInvariant())
            {
                case ".mmpk":
                    MobileMapPackage mpk = await MobileMapPackage.OpenAsync(path);
                    if (mpk.Maps.Any())
                    {
                        GeoModel = mpk.Maps.First();
                        await GeoModel.LoadAsync();
                    }
                    break;
                case ".slpk":
                    layer = new ArcGISSceneLayer(new Uri(path));
                    await layer.LoadAsync();
                    GeoModel?.OperationalLayers.Add(layer);
                    break;
                case ".mspk":
                    MobileScenePackage msp = await MobileScenePackage.OpenAsync(path);
                    if (msp.Scenes.Any())
                        GeoModel = msp.Scenes.First();
                    break;
                case ".kml":
                case ".kmz":
                    layer = new KmlLayer(new Uri(path));
                    await layer.LoadAsync();
                    GeoModel?.OperationalLayers.Add(layer);
                    break;
                case ".shp":
                    layer = new FeatureLayer(new Esri.ArcGISRuntime.Data.ShapefileFeatureTable(path));
                    await layer.LoadAsync();
                    GeoModel?.OperationalLayers.Add(layer);
                    break;
                case ".tpk":
                    layer = new ArcGISTiledLayer(new TileCache(path));
                    await layer.LoadAsync();
                    GeoModel?.OperationalLayers.Add(layer);
                    break;
                case ".tif":
                    layer = new RasterLayer(new Esri.ArcGISRuntime.Rasters.Raster(path));
                    await layer.LoadAsync();
                    GeoModel?.OperationalLayers.Add(layer);
                    break;
                case ".vtpk":
                    layer = new ArcGISVectorTiledLayer(new Uri(path));
                    await layer.LoadAsync();
                    GeoModel?.OperationalLayers.Add(layer);
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
