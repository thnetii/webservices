using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using THNETII.CdnJs;

namespace THNETII.WebServices.SampleRazorPagesApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            AssemblyName = GetType().Assembly.GetName();
        }

        public IConfiguration Configuration { get; }
        public AssemblyName AssemblyName { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages()
                .AddBootstrapApplicationPart()
                .AddJQueryValidateApplicationPart()
                .AddJQueryValidationUnobtrusiveApplicationPart()
                ;

            services.AddSingleton(this);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseRouting(); 

            app.UseAuthorization();

            app.UseEndpoints(endpoints => 
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
