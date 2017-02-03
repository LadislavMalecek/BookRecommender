using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BookRecommender.DataManipulation{

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

        public void Update(){
            count++;
            System.Console.Write(this);
        }

        public void Dispose()
        {
            Console.WriteLine();
            sW.Stop();
            var elapsed = ((double)sW.ElapsedMilliseconds)/1000;
            System.Console.WriteLine($"Query execution time: {elapsed}s");
        }

        public override string ToString()
        {
            return String.Format("\r{0}/{1}", count, max);
        }
    }
}