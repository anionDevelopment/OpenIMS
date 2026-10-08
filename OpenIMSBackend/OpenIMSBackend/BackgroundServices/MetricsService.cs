using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.Settings;
using OpenIMSBackend.Core.Constants;
using OpenIMSBackend.Core.Misc.Logger;
using OpenIMSBackend.Core.Services;
using Prometheus;
using System;

namespace OpenIMSBackend.Core.BackgroundServices
{
    public class MetricsService : IteratingBackgroundService, IMetricsService
    {
        public Gauge MetricAmountOfUsers { get; private set; }
        private readonly IPersistence _Persistence;

        /// <summary>Initializes a new instance of <see cref="MetricsService"/>.</summary>
        /// <param name="constants">Application-wide constants including the execution mode.</param>
        /// <param name="logger">The logger for diagnostic output.</param>
        /// <param name="persistence">The persistence service used to read the amount of users.</param>
        public MetricsService(IApplicationConstants<CodeUnitSpecificConstants> constants, IMetricsServiceLog logger, IPersistence persistence) : base(constants.ExecutionMode, logger.Logger)
        {
            this.Enabled = true;
            this.AdditionalDelay = TimeSpan.FromSeconds(5);
            this._Persistence = persistence;
            this.MetricAmountOfUsers = Metrics.CreateGauge(CodeUnitSpecificConstants.MetricsNameAmountOfUsers, "Amount of existing user-accounts");
        }

        /// <summary>Reads the current amount of user-accounts from the persistence and updates the Prometheus gauge.</summary>
        public void CalculateMetrics()
        {
            if (this._Persistence.IsAvailable().Item1)
            {
                try
                {
                    this.MetricAmountOfUsers.Set(this._Persistence.GetAmountOfUsers());
                }
                catch (Exception exception)
                {
                    this._Logger.Log("Error while calculating metrics", exception);
                }
            }
        }

        protected override void Run()
        {
            this.CalculateMetrics();
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
