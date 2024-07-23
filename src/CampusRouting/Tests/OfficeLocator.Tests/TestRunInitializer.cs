using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeLocator.Core.Tests
{
    [TestClass]
    public class TestRunInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            ProvisionDataHelper.AppSettings = new Preferences();
            ProvisionDataHelper.AppDataDirectory = Path.GetTempPath();
        }

        private class Preferences : IPreferences
        {
            Dictionary<string,object?> _preferences = new Dictionary<string,object?>();
            public bool ContainsKey(string key) => _preferences.ContainsKey(key);

            public T Get<T>(string key, T defaultValue) => ContainsKey(key) ? (T)_preferences[key]! : defaultValue;

            public void Remove(string key) => _preferences.Remove(key);

            public void Set<T?>(string key, T? value) => _preferences[key] = value;
        }
    }
}
