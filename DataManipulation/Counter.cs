using System;
using System.Collections.Generic;


namespace BookRecommender.DataManipulation{

class Counter
    {
        int count = 0;
        int max;
        public static Counter operator ++(Counter c)
        {
            c.count++;
            return c;
        }
        public override string ToString()
        {
            return String.Format("\r{0}/{1}", count, max);
        }
        public Counter(int max)
        {
            this.max = max;
        }
    }



}