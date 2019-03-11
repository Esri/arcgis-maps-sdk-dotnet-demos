using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeLocator.Core.Tests
{
    [TestClass]
    [TestCategory("Provisioning")]
    public class ProvisioningTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task ProvisioningTest()
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), DateTime.Now.Ticks.ToString());
            try
            {
                List<string> events = new List<string>();
                await ProvisionDataHelper.GetDataAsync(appDataFolder,
                    s =>
                    {
                        events.Add(s);
                        TestContext?.WriteLine(s); //Write to the test context for review
                    }).ConfigureAwait(false);
                Assert.IsTrue(events.Count > 0);
                Assert.IsTrue(Directory.Exists(appDataFolder));
                Assert.IsTrue(File.Exists(Path.Combine(appDataFolder, "Basemap/CampusBasemap.vtpk")));
                Assert.IsTrue(File.Exists(Path.Combine(appDataFolder, "Network", "IndoorNavigation.geodatabase")));
                Assert.IsTrue(File.Exists(Path.Combine(appDataFolder, "Geocoder", "CampusRooms.loc")));
                Assert.IsTrue(events.Where(e => e.Contains("Downloading data")).Any());
                Assert.IsNotNull(events.Where(e => e.EndsWith(" 100%")).Single());
                Assert.AreEqual("Complete", events.Last());
            }
            finally
            {
                Directory.Delete(appDataFolder, true); //Clean up
            }
        }

        private static Task provisioningTask;

        internal static Task EnsureData()
        {
            if (provisioningTask == null)
                provisioningTask = ProvisionDataHelper.GetDataAsync(null);
            return provisioningTask;
        }
    }
}
