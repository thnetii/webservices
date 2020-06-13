using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using THNETII.CdnJs;
using THNETII.WebServices.Claims.RazorPagesSample.Data;

namespace THNETII.WebServices.Claims.RazorPagesSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostEnv)
        {
            Configuration = configuration;
            HostEnvironment = hostEnv;

            AssemblyName = GetType().Assembly.GetName();
        }

        public AssemblyName AssemblyName { get; }
        public IConfiguration Configuration { get; }
        public IHostEnvironment HostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(this);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                if (HostEnvironment.IsDevelopment())
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password = new PasswordOptions
                    {
                        RequireDigit = false,
                        RequiredLength = 0,
                        RequiredUniqueChars = 0,
                        RequireLowercase = false,
                        RequireNonAlphanumeric = false,
                        RequireUppercase = false,
                    };
                }
            }).AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages()
                .AddJQueryApplicationPart()
                .AddPopperJSApplicationPart()
                .AddJQueryValidateApplicationPart()
                .AddJQueryValidationUnobtrusiveApplicationPart()
                .AddClaimsApplicationPart();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
