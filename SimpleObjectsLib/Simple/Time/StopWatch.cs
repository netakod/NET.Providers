
/* Usage Example:
   
    Stopwatch sw = new Stopwatch();
    
    sw.Start();

    for(int i=0; i<10; i++)
    {
	    System.Threading.Thread.Sleep(100);
	    Console.Write("Split time: ");
	    Console.Write(sw.GetSplitTimeInMicroseconds().ToString());
	    Console.WriteLine(" microseconds.");
    }

    sw.Stop();

    Console.Write("Total process time: ");
    Console.Write(sw.GetElapsedTimeSpan().ToString());
 */


using System;
using System.Runtime.InteropServices;

namespace Simple
{
    /// <summary>
    /// 
    /// </summary>
    public class Stopwatch
    {
        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceCounter(ref long x);
        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceFrequency(ref long x);

        private long StartTime;
        private long StopTime;
        private long ClockFrequency;
        private long CalibrationTime;

        public Stopwatch()
        {
            StartTime = 0;
            StopTime = 0;
            ClockFrequency = 0;
            CalibrationTime = 0;
            Calibrate();
        }

        public void Calibrate()
        {
            QueryPerformanceFrequency(ref ClockFrequency);

            for (int i=0; i < 1000; i++)
            {
                Start();
                Stop();
                CalibrationTime += StopTime - StartTime;
            }

            CalibrationTime /= 1000;
        }

        public void Reset()
        {
            StartTime = 0;
            StopTime = 0;
        }

        public void Start()
        {
            QueryPerformanceCounter(ref StartTime);
        }

        public void Stop()
        {
            QueryPerformanceCounter(ref StopTime);
        }

        public TimeSpan GetElapsedTimeSpan()
        {
            return TimeSpan.FromMilliseconds(_GetElapsedTime_ms());
        }

        public TimeSpan GetSplitTimeSpan()
        {
            return TimeSpan.FromMilliseconds(_GetSplitTime_ms());
        }

        public double GetElapsedTimeInMicroseconds()
        {
            return (((StopTime - StartTime - CalibrationTime) * 1000000.0 / ClockFrequency));
        }

        public double GetSplitTimeInMicroseconds()
        {
            long current_count = 0;
            QueryPerformanceCounter(ref current_count);
            return (((current_count - StartTime - CalibrationTime) * 1000000.0 / ClockFrequency));
        }

        private double _GetSplitTime_ms()
        {
            long current_count = 0;
            QueryPerformanceCounter(ref current_count);
            return (((current_count - StartTime - CalibrationTime) * 1000000.0 / ClockFrequency) / 1000.0);
        }

        private double _GetElapsedTime_ms()
        {
            return (((StopTime - StartTime - CalibrationTime) * 1000000.0 / ClockFrequency) / 1000.0);
        }

    }
}
