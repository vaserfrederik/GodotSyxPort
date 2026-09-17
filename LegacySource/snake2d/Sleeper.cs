using System;

namespace Snake2D
{
    class Sleeper
    {
        private Sleeper()
        {
        }

        private static long variableYieldTime, lastTime;

        /**
         * An accurate sync method that adapts automatically
         * to the system it runs on to provide reliable results.
         * 
         * @param fps The desired frame rate, in frames per second
         * @author kappa (On the LWJGL Forums)
         */
        public static void Sync(int fps)
        {
            if (fps <= 0) return;

            long sleepTime = 1000000000 / fps; // nanoseconds to sleep this frame
            // yieldTime + remainder micro & nano seconds if smaller than sleepTime
            long yieldTime = Math.Min(sleepTime, variableYieldTime + sleepTime % (1000 * 1000));
            long overSleep = 0; // time the sync goes over by

            try
            {
                while (true)
                {
                    long t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastTime;

                    if (t < sleepTime - yieldTime)
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                    else if (t < sleepTime)
                    {
                        // burn the last few CPU cycles to ensure accuracy
                        System.Threading.Thread.Yield();
                    }
                    else
                    {
                        overSleep = t - sleepTime;
                        break; // exit while loop
                    }
                }
            }
            catch (System.Threading.ThreadInterruptedException e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Math.Min(overSleep, sleepTime);

                // auto tune the time sync should yield
                if (overSleep > variableYieldTime)
                {
                    // increase by 200 microseconds (1/5 a ms)
                    variableYieldTime = Math.Min(variableYieldTime + 200 * 1000, sleepTime);
                }
                else if (overSleep < variableYieldTime - 200 * 1000)
                {
                    // decrease by 2 microseconds
                    variableYieldTime = Math.Max(variableYieldTime - 2 * 1000, 0);
                }
            }
        }
    }
}