using EVSystem.Communication;
using EVSystem.Components;
using EVSystem.Mock;
using System;
using System.Threading;

namespace EVSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== EV System Simulation ===");

            var j1939 = new MockJ1939Adapter();
            j1939.Register();

            var batteryMonitor = new BatteryMonitor(j1939, "battery_data.txt");

            Console.WriteLine("Starting battery simulation\n");

            
            while (true)
            {
                batteryMonitor.LoadNextData();

                Console.WriteLine(batteryMonitor.GetStatus());

                Thread.Sleep(2000);
            }

            
            j1939.Leave();
        }
    }
}
