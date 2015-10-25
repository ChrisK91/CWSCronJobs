using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CWSCronJobs
{
    public class RestartJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            if(Program.c.Test())
            {
                var result = Program.c.GetWatcherStatus();

                bool resultStatus = false;

                if (result != null && bool.TryParse(result["ENABLED"].ToString(), out resultStatus))
                {
                    if (resultStatus)
                    {
                        // Watcher is running, let's restart
                        Program.c.StopWatcher();
                        Program.c.SendRestart();
                        Console.WriteLine("Restart command sent, waiting five seconds");
                        Thread.Sleep(5000);
                        Program.c.StartWatcher();
                    }
                }
            }
        }
    }
}
