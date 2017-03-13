using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace BookRecommender.DataManipulation
{

    class Counter : IDisposable
    {
        int count = 0;
        int max;
        Stopwatch sW = new Stopwatch();
        public Counter(int max)
        {
            this.max = max;
            System.Console.Write(this);
            sW.Start();
        }

        public void Update()
        {
            Interlocked.Increment(ref count);
            if (count < 100)
            {
                System.Console.Write(this);
            }
            else
            {
                if (count % 100 == 0)
                {
                    System.Console.Write(this);
                }
            }
        }
        public void UpdateOnly()
        {
            Interlocked.Increment(ref count);
        }

        public void Dispose()
        {
            Console.WriteLine();
            sW.Stop();
            var elapsed = ((double)sW.ElapsedMilliseconds) / 1000;
            System.Console.WriteLine($"Query execution time: {elapsed}s");
        }

        public override string ToString()
        {
            if (count != 0)
            {
                var eta = (int)(((double)sW.ElapsedMilliseconds) / count * (max - count));
                return String.Format("\r{0}/{1}   ETA: {2}s", count, max, eta / 1000);
            }
            else
            {
                return String.Format("\r{0}/{1}   ETA: n\a", count, max);
            }
        }
    }
}