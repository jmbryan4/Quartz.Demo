using System;
using System.Threading.Tasks;
using Serilog;

namespace Quartz.Demo.Jobs
{
    [DisallowConcurrentExecution]
    public class HelloWorldJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Log.Logger.Information("Hello world!");
            await Console.Out.WriteLineAsync("HelloJob is executing.");
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}
