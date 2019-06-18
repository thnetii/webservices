using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using THNETII.WebServices.WindowsImpersonation;

namespace THNETII.WebServices.WindowsIdentityImpersonation.Sample
{
    internal static class Impersonation
    {
        internal static int id = 1;

        public static Task Run(IHost host, WindowsIdentity identity)
        {
            var id = Interlocked.Increment(ref Impersonation.id);
            var serviceProvider = host.Services;
            var appLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(Impersonation));
            var cancelToken = appLifetime.ApplicationStopping;

            var eventId = new EventId(id, identity.Name);
            return WindowsIdentityAsync.RunImpersonatedAsync(identity.AccessToken,
                () => RunImpersonated(logger, eventId, cancelToken));
        }

        private static async Task RunImpersonated(ILogger logger, EventId eventId, CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                var id = WindowsIdentity.GetCurrent(ifImpersonating: true);
                if (id is null)
                {
                    logger.Log(LogLevel.Error, new EventId(-eventId.Id, eventId.Name), $"Expected impersonated windows identity, but got (null)");
                    continue;
                }

                logger.Log(LogLevel.Information, eventId, $"Current User: {{{nameof(id.Name)}}}, Impersonation: {{{nameof(id.ImpersonationLevel)}}}", id.Name, id.ImpersonationLevel);

                try { await Task.Delay(TimeSpan.FromSeconds(5), cancelToken).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
