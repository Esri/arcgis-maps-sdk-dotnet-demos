using Esri.ArcGISRuntime.Security;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;

namespace MauiSignin.WinUI;

internal sealed class AppDataCredentialPersistence : CredentialPersistence
{
    private const string DataProtectionDescriptor = "LOCAL=user";
    private readonly ConcurrentDictionary<string, Credential> _credentials = new ConcurrentDictionary<string, Credential>();

    private AppDataCredentialPersistence()
    {
    }

    public static async Task<CredentialPersistence> CreateAsync()
    {
        var persistence = new AppDataCredentialPersistence();
        try
        {
            var settings = persistence.GetLocalSettings();
            if (settings.Values.Any())
            {
                var provider = new DataProtectionProvider(DataProtectionDescriptor);
                foreach (var value in settings.Values)
                {
                    try
                    {
                        var protectedBytes = (byte[])value.Value;
                        var bytes = await provider.UnprotectAsync(protectedBytes.AsBuffer()).AsTask().ConfigureAwait(false);
                        var credential = Deserialize(bytes.ToArray());
                        if (credential != null)
                            persistence._credentials.TryAdd(value.Key, credential);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"There was an error deserializing a credential, skipping.{Environment.NewLine}Exception: {ex}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"There was an error retrieving all credentials.{Environment.NewLine}Exception: {ex}");
        }

        return persistence;
    }
    
    protected override void Add(Credential credential) => AddOrUpdate(credential);

    protected override void Update(Credential credential) => AddOrUpdate(credential);

    private async void AddOrUpdate(Credential credential)
    {
        try
        {
            var serviceUrl = credential.ServiceUri!.GetLeftPart(UriPartial.Path);
            var bytes = Serialize(credential);

            var provider = new DataProtectionProvider(DataProtectionDescriptor);
            var protectedBytes = await provider.ProtectAsync(bytes.AsBuffer()).AsTask().ConfigureAwait(false);

            var settings = GetLocalSettings();
            settings.Values[serviceUrl] = protectedBytes.ToArray();
            _credentials[serviceUrl] = credential;
        }
        catch
        {
        }
    }

    internal static byte[] Serialize(Credential credential)
    {
        using (var ms = new MemoryStream())
        {
            var s = new DataContractSerializer(typeof(Credential));
            s.WriteObject(ms, credential);
            return ms.ToArray();
        }
    }

    internal static Credential? Deserialize(byte[] bytes)
    {
        using (var ms = new MemoryStream(bytes))
        {
            var s = new DataContractSerializer(typeof(Credential));
            var obj = s.ReadObject(ms);
            return (Credential?)obj;
        }
    }
    protected override void Remove(Credential credential)
    {
        var serviceUrl = credential.ServiceUri!.ToString();
        try
        {
            var settings = GetLocalSettings();
            settings.Values.Remove(serviceUrl);
            _credentials.TryRemove(serviceUrl, out _);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"There was an error removing the credential for \"{credential.ServiceUri}\".{Environment.NewLine}Exception: {ex}");
        }
    }

    protected override IEnumerable<Credential> GetCredentials() => _credentials.Values;

    protected override void Clear()
    {
        try
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Containers.ContainsKey("CredentialStore"))
                localSettings.DeleteContainer("CredentialStore");
            _credentials.Clear();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"There was an error removing all credentials.{Environment.NewLine}Exception: {ex}");
        }
    }

    private ApplicationDataContainer GetLocalSettings()
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        if (localSettings.Containers.TryGetValue("CredentialStore", out var container))
            return container;
        return localSettings.CreateContainer("CredentialStore", ApplicationDataCreateDisposition.Always);
    }
}
