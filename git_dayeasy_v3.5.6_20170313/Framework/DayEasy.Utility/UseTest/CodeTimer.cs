using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace DayEasy.Utility.UseTest
{
    /// <summary>
    /// 代码性能测试计时器
    /// </summary>
    public static class CodeTimer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadTimes(IntPtr hThread, out long lpCreationTime,
                                          out long lpExitTime, out long lpKernelTime, out long lpUserTime);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        public delegate void ActionDelegate();

        private static long GetCurrentThreadTimes()
        {
            long l;
            long kernelTime, userTimer;
            GetThreadTimes(GetCurrentThread(), out l, out l, out kernelTime,
                           out userTimer);
            return kernelTime + userTimer;
        }

        static CodeTimer()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

        }

        /// <summary> 方法监控 </summary>
        /// <param name="name">名称</param>
        /// <param name="action">方法</param>
        /// <param name="iteration">重复次数：默认1次</param>
        /// <returns></returns>
        public static CodeTimerResult Time(string name, Action action, int iteration = 0)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (action == null)
                return null;

            var result = new CodeTimerResult();
            result = result.Reset();
            result.Name = name;
            result.Iteration = iteration;

            GC.Collect(GC.MaxGeneration);
            var gcCounts = new int[GC.MaxGeneration + 1];
            for (var i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i);
            }

            // 3. Run action
            var watch = new Stopwatch();
            watch.Start();
            var ticksFst = GetCurrentThreadTimes(); //100 nanosecond one tick
            if (iteration <= 0) action();
            else
            {
                for (var i = 0; i < iteration; i++) action();
            }
            var ticks = GetCurrentThreadTimes() - ticksFst;
            watch.Stop();

            // 4. Print CPU
            result.TimeElapsed = watch.ElapsedMilliseconds;
            result.CpuCycles = ticks * 100;

            // 5. Print GC
            for (var i = 0; i <= GC.MaxGeneration; i++)
            {
                var count = GC.CollectionCount(i) - gcCounts[i];
                result.GenerationList[i] = count;
            }
            return result;
        }

        public static string UseTest()
        {
            var result1 = Time("contact", () =>
            {
                string str = "";
                for (int i = 0; i < 10; i++)
                {
                    str += "dddddddddddddddddddddd";
                }
            }, 1000 * 200);
            var result2 = Time("stringbuilder", () =>
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < 10; i++)
                {
                    sb.Append("dddddddddddddddddddddd");
                }
            }, 1000 * 200);
            return result1 + result2.ToString();
        }
    }
}