using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeLocator.Core.Tests
{
    [TestClass]
    [TestCategory("Locator")]
    public class GeocodeTests
    {
        [TestMethod]
        [TestCategory("Suggest")]
        public async Task SuggestM3NW040_SingleResult()
        {
            await ProvisioningTests.EnsureData();
            var result = await GeocodeHelper.SuggestAsync("M3NW040");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("M3nw040", result.First());
        }

        [TestMethod]
        [TestCategory("Suggest")]
        public async Task SuggestM3NW0_MultipleResults()
        {
            await ProvisioningTests.EnsureData();
            var result = await GeocodeHelper.SuggestAsync("M3NW0");
            Assert.AreEqual(8, result.Count());
            Assert.IsTrue(result.All(r => r.StartsWith("M3nw0")));
        }

        [TestMethod]
        [TestCategory("Geocode")]
        public async Task GeocodeM3NW040()
        {
            await ProvisioningTests.EnsureData();
            var result = await GeocodeHelper.GeocodeAsync("M3NW040");
            Assert.IsNotNull(result);
        }
    }
}
