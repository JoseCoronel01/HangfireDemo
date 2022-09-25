using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;

namespace HangfireDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddHangfire(config => config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170).
            UseSimpleAssemblyNameTypeSerializer().
            UseDefaultTypeSerializer().
            UseMemoryStorage());

            services.AddHangfireServer();

            services.AddSingleton<IPrintJob, PrintJob>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IWebHostEnvironment env, 
            IBackgroundJobClient backgroundJobClient,
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider)
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

            app.UseHangfireDashboard();

            backgroundJobClient.Enqueue(() => Console.WriteLine("Hello Hangfire!!!"));

            //recurringJobManager.AddOrUpdate("Run every minute", () => Console.WriteLine("Test recurring job"), "* * * * *");

            recurringJobManager.AddOrUpdate("Run every minute", () => serviceProvider.GetService<IPrintJob>().Print(), "* * * * *");
        }
    }
}
