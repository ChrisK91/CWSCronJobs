using CWSProtocol;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWSCronJobs
{
    class Program
    {
        public static Client c = new Client("CWSCronJobs");


        static void Main(string[] args)
        {
#if DEBUG
            Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info };
#endif

            bool exit = false;

            TimeOfDay restart = new TimeOfDay(0, 0);

            if (args.Length == 2)
            {
                int hour, minute;

                if(int.TryParse(args[0], out hour) && int.TryParse(args[1], out minute))
                {
                    if(hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59)
                    {
                        restart = new TimeOfDay(hour, minute);
                    }
                    else
                    {
                        Console.WriteLine("First parameter can be an hour (between 0 and 23), second one a minute (between 0 and 59)");
                    }
                }
            }

            Console.WriteLine("Restart will be daily at {0:00}:{1:00}", restart.Hour, restart.Minute);

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();


            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(restart)
                  )
                .Build();

            IJobDetail job = JobBuilder.Create<RestartJob>().WithIdentity("RestartJob","CWSJobs").Build();

            scheduler.ScheduleJob(job, trigger);

            while (!exit)
            {
                if (c.Test())
                {
                    Console.WriteLine("CWSCronJobs is connected to CWSRestart");

                    var result = c.GetWatcherStatus();

                    bool resultStatus = false;

                    if (result != null && bool.TryParse(result["ENABLED"].ToString(), out resultStatus))
                    {
                        if (resultStatus)
                            Console.WriteLine("The watcher is running");
                        else
                            Console.WriteLine("The watcher is suspended");
                    }

                }

                Console.WriteLine("Press \"q\" to quit");
                string input = Console.ReadLine();

                if (string.Equals("q", input, StringComparison.InvariantCultureIgnoreCase))
                    exit = true;
            }

            scheduler.Shutdown();
            c.Dispose();
        }
    }
}
