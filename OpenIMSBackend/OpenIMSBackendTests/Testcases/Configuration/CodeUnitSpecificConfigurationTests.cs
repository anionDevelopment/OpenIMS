using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIMSBackend.Core.Configuration;

namespace OpenIMSBackend.Tests.Testcases.Configuration
{
    [TestClass]
    public class CodeUnitSpecificConfigurationTests
    {
        [TestMethod(DisplayName = nameof(CodeUnitSpecificConfigurationRegistrationIsEnabled))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CodeUnitSpecificConfigurationRegistrationIsEnabled()
        {
            //arrange
            bool expected = true;
            CodeUnitSpecificConfiguration configuration = new CodeUnitSpecificConfiguration();

            //act
            configuration.RegistrationIsEnabled = expected;
            bool actual = configuration.RegistrationIsEnabled;

            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod(DisplayName = nameof(CodeUnitSpecificConfigurationLoginIsEnabled))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CodeUnitSpecificConfigurationLoginIsEnabled()
        {
            //arrange
            bool expected = true;
            CodeUnitSpecificConfiguration configuration = new CodeUnitSpecificConfiguration();

            //act
            configuration.LoginIsEnabled = expected;
            bool actual = configuration.LoginIsEnabled;

            //assert
            Assert.AreEqual(expected, actual);
        }
    }
}
