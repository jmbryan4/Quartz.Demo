using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;

namespace Quartz.Demo.JobFactories
{
    /// <summary>
    /// By default, Quartz will try and "new-up" instances of a job using Activator.CreateInstance, effectively calling new HelloWorldJob().
    /// If we want to use constructor injection, that won't work.
    /// It is only safe to create IJob implementations that are Singletons or Transient
    /// using Scoped dependency injection services in your Quartz jobs will require additional effort b/c of captive dependency problem.
    /// </summary>
    public class SingletonJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public SingletonJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job) { }
    }
}
