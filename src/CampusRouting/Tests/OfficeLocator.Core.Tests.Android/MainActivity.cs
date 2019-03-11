using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OfficeLocator.Core.Tests.Droid
{
    [Activity(Name = "officeLocator.RunTestsActivity", Label = "OfficeLocator.Core.Tests", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : MSTestX.TestRunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(bundle);
        }

        protected override MSTestX.TestOptions GenerateTestOptions()
        {
            var testOptions = base.GenerateTestOptions(); // Creates default test options and initializes some values based on intent arguments.
            // Set/override test settings...
            return testOptions;
        }

        protected override void OnTestRunStarted(IEnumerable<TestCase> testCases)
        {
            base.OnTestRunStarted(testCases);
        }

        protected override void OnTestRunCompleted(IEnumerable<TestResult> results)
        {
            base.OnTestRunCompleted(results);
        }

        protected override void OnTestsDiscovered(IEnumerable<TestCase> testCases)
        {
            base.OnTestsDiscovered(testCases);
            // Run all tests:
            // Task<IEnumerable<TestResult>> results = base.RunTestsAsync(testCases);
        }
    }
}

