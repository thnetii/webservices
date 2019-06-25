using System.Net.Http;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using AspNet.Security.OAuth.GitHub;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Octokit;

using THNETII.WebServices.FileProviders.GitHub;

using IAspNetCoreHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace THNETII.WebServices.GitHubRepositoryBrowser
{
    public class Startup
    {
        private static readonly ProductHeaderValue productHeader =
            new ProductHeaderValue(
                typeof(Startup).Assembly.GetName().Name,
                typeof(Startup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? typeof(Startup).Assembly.GetName().Version.ToString()
                );

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(
                o =>
                {
                    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(o =>
                {
                    var configPath = ConfigurationPath.Combine(
                        nameof(Microsoft.AspNetCore.Authentication),
                        nameof(Microsoft.AspNetCore.Authentication.Cookies)
                        );
                    Configuration.GetSection(configPath).Bind(o);
                })
                .AddGitHub(o =>
                {
                    o.SaveTokens = true;

                    var configPath = ConfigurationPath.Combine(
                        nameof(Microsoft.AspNetCore.Authentication),
                        nameof(AspNet.Security.OAuth.GitHub)
                        );
                    Configuration.GetSection(configPath).Bind(o);
                })
                ;
            services.AddMvc();
            services.AddHttpClient(nameof(Octokit));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IAspNetCoreHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(router =>
            {
                router.MapRoute("/login", httpContext => httpContext.ChallengeAsync(GitHubAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" }));
                router.MapRoute("/logout", httpContext => httpContext.SignOutAsync(new AuthenticationProperties { RedirectUri = "/" }));

                router.MapRoute("/github/{owner}/{repository}/{**subpath}", async httpContext =>
                {
                    var owner = (string)httpContext.GetRouteValue("owner");
                    var repository = (string)httpContext.GetRouteValue("repository");
                    var subPath = (string)httpContext.GetRouteValue("subpath");

                    var serviceProvider = httpContext.RequestServices;
                    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                    var htmlEncoder = serviceProvider.GetRequiredService<HtmlEncoder>();

                    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                    var gitHubClient = new GitHubClient(productHeader);

                    var accessToken = await httpContext.GetTokenAsync(GitHubAuthenticationDefaults.AuthenticationScheme, "access_token");
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        gitHubClient.Connection.Credentials = new Credentials(accessToken);
                    }

                    var fileServerOptions = new FileServerOptions
                    {
                        EnableDefaultFiles = true,
                        EnableDirectoryBrowsing = true,
                        RequestPath = $"/github/{owner}/{repository}",
                        FileProvider = new GitHubFileProvider(owner, repository, gitHubClient, httpClientFactory.CreateClient(nameof(Octokit)))
                    };
                    fileServerOptions.StaticFileOptions.ServeUnknownFileTypes = true;

                    var staticFiles = new StaticFileMiddleware(_ => Task.CompletedTask, env, Options.Create(fileServerOptions.StaticFileOptions), loggerFactory);
                    var directoryBrowsing = new DirectoryBrowserMiddleware(staticFiles.Invoke, env, htmlEncoder, Options.Create(fileServerOptions.DirectoryBrowserOptions));
                    var defaultFiles = new DefaultFilesMiddleware(directoryBrowsing.Invoke, env, Options.Create(fileServerOptions.DefaultFilesOptions));
                    await defaultFiles.Invoke(httpContext);
                });
            });
        }
    }
}
