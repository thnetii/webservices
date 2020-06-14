using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace THNETII.WebServices.SampleRazorPagesApp
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var cmdRoot = new RootCommand { Handler = Handler };
            var cmdParser = new CommandLineBuilder(cmdRoot)
                .UseDefaults()
                .UseHost(CreateHostBuilder)
                .Build();
            return cmdParser.InvokeAsync(args);
        }

        internal static ICommandHandler Handler = CommandHandler.Create(
            (IHost host, CancellationToken cancelToken) =>
                host.WaitForShutdownAsync(cancelToken));

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
