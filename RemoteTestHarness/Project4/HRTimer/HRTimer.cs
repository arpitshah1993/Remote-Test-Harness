/////////////////////////////////////////////////////////////////////
// HRTimer.cs - It is high resolution Timer which helps to measure //
// the time.                                                        //
//                                                                 //
// Application: CSE681 - Software Modelling and Analysis,          //
//  Remote Test Harness Project-4                                  //
// Author:      Arpit Shah, Syracuse University,                   //
//              aushah@syr.edu, (646) 288-9410                     //
// Insturctor: Jim Fawcett                                         //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operation:
 * ================
 * It is used to measure the time with high resolution. It measures 
 * the time with great acuteness. It is used like a stopwatch. User 
 * just have to start and stop it. After stopping the timer, it will 
 * return elapsed time.    
 * 
 * Public Interface 
 * ================
 *  public void Start() //Used to start the timer
 *  public ulong Stop() //Used to stop the timer
 *
 * Build Process
 * =============
 * - Required Files: HRTimer.cs 
 * - Compiler Command: csc HRTimer.cs 
 * 
 * Maintainance History
 * ====================
 * ver 1.0 : 20 November 2016
 *     - first release 
 */
//
using System;
using System.Runtime.InteropServices; 
using System.ComponentModel; 
using System.Threading; 

namespace HRTimer
{
    [Serializable]
    public class HiResTimer
    {
        protected ulong a, b, f;

        public HiResTimer()
        {
            a = b = 0UL;
            if (QueryPerformanceFrequency(out f) == 0)
                throw new Win32Exception();
        }

        public ulong ElapsedTicks
        {
            get
            { return (b - a); }
        }

        public ulong ElapsedMicroseconds
        {
            get
            {
                ulong d = (b - a);
                if (d < 0x10c6f7a0b5edUL) // 2^64 / 1e6
                    return (d * 1000000UL) / f;
                else
                    return (d / f) * 1000000UL;
            }
        }

        public TimeSpan ElapsedTimeSpan
        {
            get
            {
                ulong t = 10UL * ElapsedMicroseconds;
                if ((t & 0x8000000000000000UL) == 0UL)
                    return new TimeSpan((long)t);
                else
                    return TimeSpan.MaxValue;
            }
        }

        public ulong Frequency
        {
            get
            { return f; }
        }

        /// <summary>
        /// Used to start the timer
        /// </summary>
        public void Start()
        {
            Thread.Sleep(0);
            QueryPerformanceCounter(out a);
        }

        /// <summary>
        /// Used to stop the timer
        /// </summary>
        /// <returns></returns>
        public ulong Stop()
        {
            QueryPerformanceCounter(out b);
            return ElapsedTicks;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern
           int QueryPerformanceFrequency(out ulong x);

        [DllImport("kernel32.dll")]
        protected static extern
           int QueryPerformanceCounter(out ulong x);
    }
    class Program
    {
#if(TEST_HRTIMER)
        static void Main(string[] args)
        {
            try
            {
                HRTimer.HiResTimer hr = new HiResTimer();
                hr.Start();
                Thread.Sleep(1000);
                hr.Stop();
                Console.Write("\nmicro seconds:{0}", hr.ElapsedMicroseconds);
                Console.Write("\nticks:{0}", hr.ElapsedTicks);
                Console.Write("\ntimespan:{0}",hr.ElapsedTimeSpan);
            }
            catch (Exception ex)
            {
                Console.Write("\n Exception caught:{0}", ex.Message);
            }
            Console.ReadLine();
        }
#endif
    }
}