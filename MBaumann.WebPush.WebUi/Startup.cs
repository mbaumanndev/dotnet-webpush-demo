using System;
using System.IO;
using MBaumann.WebPush.WebUi.Configuration;
using MBaumann.WebPush.WebUi.Data;
using MBaumann.WebPush.WebUi.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace MBaumann.WebPush.WebUi
{
    public sealed class Startup
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>The exit code that is given to the operating system after the program ends.</returns>
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.RollingFile($"Logs{Path.DirectorySeparatorChar}log-{{Date}}.txt")
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                BuildWebHost(args).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Builds the web host.
        /// </summary>
        /// <returns>The web host.</returns>
        /// <param name="p_args">Command line arguments.</param>
        public static IWebHost BuildWebHost(string[] p_args) =>
            WebHost.CreateDefaultBuilder(p_args)
            .UseStartup<Startup>()
                .UseSerilog()
                .Build();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MBaumann.WebPush.WebUi.Startup"/> class.
        /// </summary>
        /// <param name="p_configuration">Configuration.</param>
        /// <param name="p_env">Hosting Environment</param>
        public Startup(IConfiguration p_configuration, IHostingEnvironment p_env)
        {
            Configuration = p_configuration;
            Environment = p_env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="p_services">Services container.</param>
        public void ConfigureServices(IServiceCollection p_services)
        {
            p_services.AddDbContext<WebPushDbContext>();

            p_services.Configure<WebPushOptions>(Configuration.GetSection("WebPush"));

            p_services.AddScoped<ISubscriptionService, SubscriptionService>();

            p_services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        /// <summary>
        /// Configure the app.
        /// </summary>
        /// <param name="p_app">Application builder.</param>
        public void Configure(IApplicationBuilder p_app)
        {
            InitializeDatabase(p_app);

            if (Environment.IsDevelopment())
            {
                p_app.UseDeveloperExceptionPage();
            }
            else
            {
                p_app.UseExceptionHandler("/Home/Error");
                p_app.UseHsts();
            }

            p_app.UseStaticFiles();

            p_app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="p_app">Application builder.</param>
        void InitializeDatabase(IApplicationBuilder p_app)
        {
            using (var serviceScope = p_app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<WebPushDbContext>();
                context.Database.Migrate();
            }
        }
    }
}
