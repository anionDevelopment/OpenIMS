using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OpenIMSBackend.Core;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace OpenIMSBackend.Tests.TestUtilities
{
    /// <summary>
    /// Starts a real instance of the backend for an integration-test, waits until it reports itself healthy and
    /// gives the testcase a http-client for it. Disposing it stops the server again.
    /// </summary>
    public sealed class IntegrationTestFramework : IDisposable
    {
        private bool Started = false;
        private Program? _Program = null;
        private readonly IntegrationTestConfiguration _IntegrationTestConfiguration;
        internal IGRYLog? _Log;

        public IntegrationTestFramework(bool startServer) : this(new IntegrationTestConfiguration(), startServer)
        {
        }

        public IntegrationTestFramework(IntegrationTestConfiguration integrationTestConfiguration, bool startServer)
        {
            this._IntegrationTestConfiguration = integrationTestConfiguration;
            if (startServer)
            {
                this.StartServer();
            }
        }

        public void StartServer()
        {
            Action action = () =>
            {
                this._Program = new Program
                {
                    RunAsync = !this._IntegrationTestConfiguration.RunInOwnThread,
                    ListenOnEveryIP = false,
                    SetupMocks = this._IntegrationTestConfiguration.SetupMocks
                };

                string[] args = Array.Empty<string>();//TODO add option to pass more configuration-values for the test-run like port etc. so that this can not go wrong due to a different configuration from a previous (manual) run.
                int exitCode = this._Program.MainImplementation(args);
                Thread.Sleep(TimeSpan.FromSeconds(5));
                GRYLibrary.Core.Misc.Utilities.AssertCondition(exitCode == 0, () =>
                {
                    string message = $"Exitcode of main-method was {exitCode}.";
                    if (this._Program._Log != null)
                    {
                        IGRYLog log = GRYLibrary.Core.Misc.Utilities.AssertNotNull(this._Program._Log, nameof(Program._Log));
                        LogItem[] logMessages = log.LastLogEntries.GetEntries();
                        if (logMessages.Any())
                        {
                            message = $"{message} Last log entries:\n" + string.Join("\n", logMessages.Select(item =>
                            {
                                item.Format(this._Program._Log.Configuration, out string result, out int _, out int _, out ConsoleColor _, GRYLogLogFormat.GRYLogFormat);
                                return result;
                            }));
                        }
                    }
                    return message;
                });
            };
            if (this._IntegrationTestConfiguration.RunInOwnThread)
            {
                Thread t = new Thread(() => action())
                {
                    Name = nameof(Program)
                };
                t.Start();
            }
            else
            {
                action();
            }
            Exception? lastException = null;
            if (!GRYLibrary.Core.Misc.Utilities.RunWithTimeout(() =>
            {
                while (!this.IsReady(out lastException))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }, TimeSpan.FromSeconds(120)))
            {
                if (lastException == null)
                {
                    throw new DependencyNotAvailableException("Could not start service.");
                }
                else
                {
                    throw lastException;
                }
            }
            Program program = GRYLibrary.Core.Misc.Utilities.AssertNotNull(this._Program, nameof(this._Program));
            this.Started = true;
            this._Log = program._Log;
        }

        private bool IsReady(out Exception? exception)
        {
            try
            {
                using HttpClient client = this.GetClient();
                string url = $"{this.GetServerURL()}{ServerConfiguration.APIRoutePrefix}/Other/Maintenance/HealthCheck";
                HttpResponseMessage response = client.GetAsync(url).WaitAndGetResult();
                Assert.IsTrue(response.IsSuccessStatusCode);
                string content = response.Content.ReadAsStringAsync().WaitAndGetResult();
                dynamic obj = JsonConvert.DeserializeObject(content)!;
                int status = (int)obj["status"];
                exception = null;
                if (status == 2) //2 means healthy.
                {
                    return true;
                }
                else
                {
                    exception = new NotReadyException($"Service is not healthy yet due to status \"{status}\".");
                    return false;
                }
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }

        /// <summary>
        /// Returns a http-client for the started server.
        /// <para>
        /// It sends no access-token: a server which is started by a testcase runs in the execution-mode
        /// <c>TestRun</c> (only "--RealRun true" enters the production-path), and in that mode the
        /// initialization-service does not run. So neither the roles nor the admin-user exist, which means no
        /// account can be created and nobody can log in. A testcase which needs a logged-in user therefore does
        /// not belong here but to the unit-tests of the business-logic, which run against the transient
        /// persistence.
        /// </para>
        /// </summary>
        public HttpClient GetClient()
        {
            return new HttpClient();
        }

        public string GetServerURL()
        {
            return $"http://127.0.0.1:{HTTP.DefaultPort}";
        }

        public void Dispose()
        {
            this.EnsureServerIsStopped();
        }

        private void EnsureServerIsStopped()
        {
            if (this.Started)
            {
                this._Program!.Stop();
                this.Started = false;
            }
        }
    }
}
