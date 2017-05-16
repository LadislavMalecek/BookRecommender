using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace BookRecommender.DataManipulation
{
    /// <summary>
    /// counter used to view progress when mining from command line
    /// not used when mining from the web manage interface
    /// It is designed to be used with USING(){} pattern
    /// </summary>
    class Counter : IDisposable
    {
        int count = 0;
        int max;
        int cwInterval;
        Stopwatch sW = new Stopwatch();
        public Counter(int max, int cwInterval = 100)
        {
            this.max = max;
            this.cwInterval = cwInterval;
            System.Console.Write(this);
            sW.Start();
        }
        /// <summary>
        /// Updates and writes progress to the console
        /// </summary>
        public void Update()
        {
            Interlocked.Increment(ref count);
            if (count < 100)
            {
                System.Console.Write(this);
            }
            else
            {
                if (count % cwInterval == 0)
                {
                    System.Console.Write(this);
                }
            }
        }
        /// <summary>
        /// Updates timer without writing to console
        /// </summary>
        public void UpdateOnly()
        {
            Interlocked.Increment(ref count);
        }
        /// <summary>
        /// Method to enable using pattern
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine();
            sW.Stop();
            var elapsed = ((double)sW.ElapsedMilliseconds) / 1000;
            System.Console.WriteLine($"Query execution time: {elapsed}s");
        }

        /// <summary>
        /// Custom to string format to write always to the current line, not newline
        /// Counts time left based on the current count, time from start and final count
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (count != 0)
            {
                var eta = (int)(((double)sW.ElapsedMilliseconds) / count * (max - count))/1000;
                var hours = eta / 3600;
                var minutes = (eta - 3600 * hours) / 60;
                var seconds = (eta - 3600 * hours) - 60 * minutes;
                return String.Format($"\r{count}/{max}   ETA: {hours}h{minutes}m{seconds}s");
            }
            else
            {
                return String.Format("\r{0}/{1}   ETA: n\a", count, max);
            }
        }
    }
}