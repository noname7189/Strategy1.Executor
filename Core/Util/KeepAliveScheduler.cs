using Quartz;
using Quartz.Impl;
using Strategy1.Executor.Core.Provider;

namespace Strategy1.Executor.Core.Util
{
    internal class KeepAliveJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            bool success = await BinanceProvider.KeepAlive();

            if (success)
            {
                Console.WriteLine("KeepAlive Success");
            }
            else
            {
                Console.WriteLine("KeepAlive Fail");
            }
        }
    }

    internal class KeepAliveScheduler
    {
        private static readonly IScheduler _scheduler = new StdSchedulerFactory()
            .GetScheduler()
            .Result;

        internal static void Run()
        {
            IJobDetail job = JobBuilder
                .Create<KeepAliveJob>()
                .WithIdentity("KeepAliveJob", "JobGroup")
                .Build();

            ICronTrigger trigger = (ICronTrigger)
                TriggerBuilder
                    .Create()
                    .WithIdentity("RepeatingTrigger", "TriggerGroup")
                    .WithCronSchedule("0 0/30 * * * ?")
                    .StartNow()
                    .Build();

            _ = _scheduler.ScheduleJob(job, trigger);
            _scheduler.Start();
        }
    }
}
