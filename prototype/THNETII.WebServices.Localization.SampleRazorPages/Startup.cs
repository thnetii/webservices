using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace THNETII.WebServices.Localization.SampleRazorPages
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        [SuppressMessage("Performance", "CA1822: Mark members as static")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.AddOptions<RequestLocalizationOptions>()
                .Configure(options =>
                {
                    var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                    foreach (var culture in allCultures.Except(options.SupportedCultures).ToList())
                        options.SupportedCultures.Add(culture);
                    foreach (var culture in allCultures.Except(options.SupportedUICultures).ToList())
                        options.SupportedUICultures.Add(culture);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [SuppressMessage("Performance", "CA1822: Mark members as static")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<RequestLocalizationOptions> requestLocalizationOptions)
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

            app.UseRequestLocalization(requestLocalizationOptions?.Value ?? new RequestLocalizationOptions());

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
