using System;
using CrystalQuartz.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz.Demo.Jobs;
using Serilog;

namespace Quartz.Demo
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
            // base quartz scheduler, job and trigger configuration
            services.AddQuartz(q =>
            {
                // optional: Sets the instance id of the scheduler (must be unique within a cluster)
                // handy when part of cluster or you want to otherwise identify multiple schedulers
                //q.SchedulerId = "Scheduler-Core";

                // we could leave DI configuration intact and then jobs need to have public no-arg constructor
                // the MS DI is expected to produce transient job instances
                // this WONT'T work with scoped services like EF Core's DbContext
                q.UseMicrosoftDependencyInjectionJobFactory(options =>
                {
                    // if we don't have the job in DI, allow fallback to configure via default constructor
                    options.AllowDefaultConstructor = true;
                });

                // or for scoped service support like EF Core DbContext
                // q.UseMicrosoftDependencyInjectionScopedJobFactory();

                // these are the defaults
                // q.UseSimpleTypeLoader();
                // q.UseInMemoryStore();
                //q.UseDefaultThreadPool(tp =>
                //{
                //    tp.MaxConcurrency = 10;
                //});

                q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 90);

                // example of persistent job store using JSON serializer as an example
                // https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/job-stores.html#ado-net-job-store-adojobstore
                // https://www.quartz-scheduler.net/documentation/best-practices.html#ado-net-jobstore
                q.UsePersistentStore(s =>
                {
                    // it's generally recommended to stick with string property keys and values when serializing
                    s.UseProperties = true;
                    s.RetryInterval = TimeSpan.FromSeconds(15);
                    s.UsePostgres(Configuration.GetConnectionString("QuartzConnection"));
                    s.UseJsonSerializer();
                    //s.UseClustering(c => // enterprise/pro feature
                    //{
                    //    c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
                    //    c.CheckinInterval = TimeSpan.FromSeconds(10);
                    //});
                });

                // convert time zones using converter that can handle Windows/Linux differences
                q.UseTimeZoneConverter();

                // quickest way to create a job with single trigger is to use ScheduleJob
                q.ScheduleJob<HelloWorldJob>(trigger => trigger
                    .WithIdentity("Combined Configuration Trigger")
                    .StartAt(DateTimeOffset.UtcNow.AddMinutes(2))
                    .WithDescription("my awesome trigger configured for a job with single call")
                );

                // you can also configure individual jobs and triggers with code
                // this allows you to associated multiple triggers with same job
                // (if you want to have different job data map per trigger for example)
                //var jobKey = new JobKey("example job", "example group");
                //q.AddJob<ExampleJob>(j => j
                //    .StoreDurably() // we need to store durably if no trigger is associated
                //    .WithIdentity(jobKey)
                //    .WithDescription("my example job")
                //);

                //q.AddTrigger(t => t
                //    .ForJob(jobKey)
                //    .StartNow()
                //    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(30)).RepeatForever())
                //    .WithDescription("my example simple trigger")
                //);
            })
                .AddQuartzServer(o => o.WaitForJobsToComplete = true); // ASP.NET Core hosting

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISchedulerFactory schedulerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging(o =>
            {
                o.EnrichDiagnosticContext = (diagnosticContext, httpContext) => diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            });

            app.UseRouting();
            app.UseAuthorization();

            var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            app.UseCrystalQuartz(() => scheduler); // endpoint /quartz

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
