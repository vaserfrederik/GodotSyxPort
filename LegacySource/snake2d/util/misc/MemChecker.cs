using System;

namespace Snake2D.Util.Misc
{
    public static class MemChecker
    {
        private static long memo = 0;

        public static bool Check()
        {
            Runtime rt = Runtime.GetRuntime();
            long mem = rt.TotalMemory() - rt.FreeMemory();

            if (mem != memo)
            {
                Console.WriteLine(mem - memo);
                memo = mem;
                return true;
            }
            return false;
        }

        public static bool Check(int i)
        {
            Runtime rt = Runtime.GetRuntime();
            long mem = rt.TotalMemory() - rt.FreeMemory();

            if (mem != memo)
            {
                Console.WriteLine($"{i} {mem - memo}");
                memo = mem;
                return true;
            }
            return false;
        }

        public static void Clear()
        {
            Runtime rt = Runtime.GetRuntime();
            memo = rt.TotalMemory() - rt.FreeMemory();
        }
    }
}