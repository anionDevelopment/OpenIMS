using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.Settings;
using OpenIMSBackend.Core.Misc.Logger;
using System;

namespace OpenIMSBackend.Core.BackgroundServices
{
    public class ManagementService : IteratingBackgroundService, IManagementScheduler
    {
        /// <summary>Initializes a new instance of <see cref="ManagementService"/>.</summary>
        /// <param name="logger">The logger used for diagnostic output.</param>
        /// <param name="applicationConstants">Application-wide constants, including the execution mode.</param>
        public ManagementService(IManagementServiceLog logger, IApplicationConstants applicationConstants) : base(applicationConstants.ExecutionMode, logger.Logger)
        {
            this.Enabled = true;
            this.AdditionalDelay = TimeSpan.FromSeconds(2);
        }

        protected override void Run()
        {
            //This is the place for work which has to happen regularly and does not belong to a request. There is
            //no such work yet; the removal of expired access-token belongs here as soon as it is implemented.
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //add dispose logic here if required
            }
            base.Dispose(disposing);
        }
    }
}
