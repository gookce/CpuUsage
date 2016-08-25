using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Getit
{
    public class Program
    {
        public static System.Timers.Timer aTimer = new System.Timers.Timer(1000);
        public static int count = 0;
        public static Process currentProcess = new Process();

        static void Main(string[] args)
        {
            var processes = Process.GetProcesses();

            for (int i = 0; i < 10; i++)
            {
                foreach (var p in processes)
                {
                    currentProcess = p;

                    var counter = ProcessCpuCounter.GetPerfCounterForProcessId(p.Id);
                    if (counter == null)
                        continue;
                    else
                        counter.NextValue();

                    Thread.Sleep(200);

                    int memsize = 0; // memsize in Megabyte
                    PerformanceCounter PC = new PerformanceCounter();
                    PC.CategoryName = "Process";
                    PC.CounterName = "Working Set - Private";
                    PC.InstanceName = p.ProcessName;
                    memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
                    PC.Close();
                    PC.Dispose();

                    var cpu = counter.NextValue() / (float)Environment.ProcessorCount;
                    Console.WriteLine(counter.InstanceName + " -  Cpu : % " + cpu + "-  Ram : " + memsize);


                    if(cpu>=5)
                    {
                        aTimer.Elapsed += OnTimedEvent;
                        aTimer.Start();
                        Console.WriteLine("Cpu Value For "+ currentProcess.ProcessName);
                        ProcessLongAlive(currentProcess.ProcessName);
                        break;
                    }
                }
            }

            Console.WriteLine("Any key to exit...");
            Console.Read();
        }

        public static void ProcessLongAlive(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            for (int i = 0; i < 10; i++)
            {
                foreach (var p in processes)
                {
                    var counter = ProcessCpuCounter.GetPerfCounterForProcessId(p.Id);
                    if (counter == null)
                        continue;
                    else
                        counter.NextValue();

                    Thread.Sleep(200);

                    int memsize = 0; // memsize in Megabyte
                    PerformanceCounter PC = new PerformanceCounter();
                    PC.CategoryName = "Process";
                    PC.CounterName = "Working Set - Private";
                    PC.InstanceName = p.ProcessName;
                    memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
                    PC.Close();
                    PC.Dispose();

                    var cpu = counter.NextValue() / (float)Environment.ProcessorCount;
                    Console.WriteLine(counter.InstanceName + " -  Cpu : % " + cpu + "-  Ram : " + memsize);

                    if (count > 1)
                        StopProcess();
                }
            }
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            count++;        
        }

        public static void StopProcess ()
        {
            try
            {
                var workers = Process.GetProcessesByName(currentProcess.ProcessName);

                foreach (Process worker in workers)
                {
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                    Console.WriteLine("Killed");
                }
            }
            catch
            {
                Console.WriteLine("Access is denied");
            }
           
        }
}
}
