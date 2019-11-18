using System;
using System.Diagnostics;

namespace CommsLIBLite.Helper
{
    public class TimeTools
    {
        private static readonly double microsPerCycle = 1000000.0 / Stopwatch.Frequency;


        // Ojo. Reset every 24 days. 0 --> Max --> Min
        public static int GetCoarseMillisNow()
        {
            return Environment.TickCount & Int32.MaxValue;
        }

        internal static long GetLocalMicrosTime(long offset = 0)
        {
            return ((long)(Stopwatch.GetTimestamp() * microsPerCycle) - offset);
        }
    }
}
