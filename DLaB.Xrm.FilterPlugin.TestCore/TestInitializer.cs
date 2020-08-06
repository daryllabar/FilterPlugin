using DLaB.Xrm.Test;
using DLaB.Xrm.FilterPlugin.Test.Builders;
using DLaB.Xrm.FilterPlugin.Entities;

namespace DLaB.Xrm.FilterPlugin.Test
{
    /// <summary>
    /// Class to Initialize all TestSettings used by the Framework
    /// </summary>
    public class TestInitializer
    {
        /// <summary>
        /// Initializes the test settings.
        /// </summary>
        public static void InitializeTestSettings()
        {
            if (!TestSettings.AssumptionXmlPath.IsConfigured)
            {
                TestSettings.AssumptionXmlPath.Configure(new PatherFinderProjectOfType(typeof(MsTestProvider), "Assumptions\\Entity Xml"));
            }
            if (!TestSettings.UserTestConfigPath.IsConfigured)
            {
                TestSettings.UserTestConfigPath.Configure(new PatherFinderProjectOfType(typeof(MsTestProvider), "UnitTestSettings.user.config"));
            }
            if (!TestSettings.EntityBuilder.IsConfigured)
            {
                TestSettings.EntityBuilder.ConfigureDerivedAssembly<AccountBuilder>();
            }
            if (!TestSettings.EarlyBound.IsConfigured)
            {
                TestSettings.EarlyBound.ConfigureDerivedAssembly<CdsContext>();
            }
            if (!TestSettings.SourceControlProvider.IsConfigured)
            {
                TestSettings.SourceControlProvider.ConfigureNone();
            }
            if (!TestSettings.TestFrameworkProvider.IsConfigured)
            {
                TestSettings.TestFrameworkProvider.Configure(new MsTestProvider());
            }
        }
    }
}
