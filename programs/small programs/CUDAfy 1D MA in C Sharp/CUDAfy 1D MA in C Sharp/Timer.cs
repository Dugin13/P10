using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUDAfy_1D_MA_in_C_Sharp
{
    // timer class taken from the paper: Microbenchmarks in Java and C# by Peter Sestoft (sestoft@itu.dk) IT University of Copenhagen, Denmark
    // plan on using Mark3: automate multiple runs for tests
    class Timer
    {
        private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        public Timer() { Play(); }
        public double Check() { return stopwatch.ElapsedMilliseconds; }
        public void Pause() { stopwatch.Stop(); }
        public void Play() { stopwatch.Start(); }
    }
}
