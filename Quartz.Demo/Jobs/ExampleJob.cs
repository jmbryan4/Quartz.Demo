using System;
using System.Threading.Tasks;
using Serilog;

namespace Quartz.Demo.Jobs
{
    [DisallowConcurrentExecution]
    public class ExampleJob : IJob, IDisposable
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Log.Logger.Information("Example Job!");
            Log.Logger.Information(context.JobDetail.Key + " job executing, triggered by " + context.Trigger.Key);
            await Task.Delay(TimeSpan.FromSeconds(20));
        }

        public void Dispose()
        {
            Log.Logger.Information("Example job disposing");
        }
    }
}
