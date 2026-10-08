using GRYLibrary.Core.APIServer.Settings;
using System;

namespace OpenIMSBackend.Tests.TestUtilities
{
    public class IntegrationTestConfiguration
    {
        public bool RunInOwnThread { get; set; }
        public Action<FunctionalInformation<OpenIMSBackend.Core.Constants.CodeUnitSpecificConstants, OpenIMSBackend.Core.Configuration.CodeUnitSpecificConfiguration, OpenIMSBackend.Core.Configuration.CommandlineParameter>>? SetupMocks { get; set; }
        public IntegrationTestConfiguration(Action<FunctionalInformation<OpenIMSBackend.Core.Constants.CodeUnitSpecificConstants, OpenIMSBackend.Core.Configuration.CodeUnitSpecificConfiguration, OpenIMSBackend.Core.Configuration.CommandlineParameter>>? setupMocks = null, bool runInOwnThread = false)
        {
            this.SetupMocks = setupMocks;
            this.RunInOwnThread = runInOwnThread;
        }
    }
}
