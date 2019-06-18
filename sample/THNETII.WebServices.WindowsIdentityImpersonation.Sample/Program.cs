using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace THNETII.WebServices.WindowsIdentityImpersonation.Sample
{
    public static class Program
    {
        internal static readonly object consoleSync = new object();

        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .Build();

            using (host)
            {
                await host.StartAsync().ConfigureAwait(false);

                await Run(host, args).ConfigureAwait(false);

                await host.StopAsync().ConfigureAwait(false);
            }
        }

        private static async Task Run(IHost host, string[] args)
        {
            var serviceProvider = host.Services;
            var appLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(Program));

            var impersonationTasks = (args ?? Array.Empty<string>())
                .Select(a => Impersonation.Run(host, a))
                .ToList();

            var cancelToken = appLifetime.ApplicationStopping;

            while (!cancelToken.IsCancellationRequested)
            {
                var id = WindowsIdentity.GetCurrent();
                logger.LogInformation($"Current User: {{{nameof(id.Name)}}}, Impersonation: {{{nameof(id.ImpersonationLevel)}}}", id.Name, id.ImpersonationLevel);
                try { await Task.Delay(TimeSpan.FromSeconds(5), cancelToken).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }

            await Task.WhenAll(impersonationTasks).ConfigureAwait(false);
        }
    }
}
