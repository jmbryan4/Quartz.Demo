using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quartz.Demo.Jobs;

namespace Quartz.Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public JobsController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        [HttpGet]
        public IActionResult Get() => Ok("Quartz Demo app");

        [HttpGet("schedule")]
        public async Task<IActionResult> QuartzTest()
        {
            // define the job detail and tie it to our job class
            IJobDetail job = JobBuilder.Create<ExampleJob>()
                //.WithIdentity("job1", "group1") // (optional)
                .Build();

            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                // .WithIdentity("trigger1", "group1") // (optional)
                .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Minute)) // use DateBuilder to create a date in the future
                // .ForJob(job.Key)
                .Build();

            var scheduler = await _schedulerFactory.GetScheduler();

            await scheduler.ScheduleJob(job, trigger);
            return Ok("scheduled");
        }

        [HttpGet("schedule/{numberOfJobs}")]
        public async Task<IActionResult> QuartzTest(int numberOfJobs)
        {
            if (numberOfJobs > 1000) numberOfJobs = 1000;

            var scheduler = await _schedulerFactory.GetScheduler();

            var scheduledTasks = new List<Task<DateTimeOffset>>();
            for (int i = 0; i < numberOfJobs; i++)
            {
                var job = JobBuilder.Create<ExampleJob>().Build();
                var trigger = (ISimpleTrigger)TriggerBuilder.Create().StartNow().Build();
                scheduledTasks.Add(scheduler.ScheduleJob(job, trigger));
            }
            await Task.WhenAll(scheduledTasks);

            return Ok(numberOfJobs + " jobs scheduled");
        }
    }
}
