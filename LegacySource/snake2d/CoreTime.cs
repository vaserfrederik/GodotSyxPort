using System;

namespace snake2d
{
    public class CoreTime
    {
        private double secondsSinceFirstUpdate = 0;
        private long nowMillis;
        private long nowNanos;
        private float zeroToOneToZero1s = 0;
        private float zeroToOneToZero1sDir = 1;

        public CoreTime()
        {
        }

        public void update(float ds, long nowMillis, long nowNanos)
        {
            secondsSinceFirstUpdate += ds;
            this.nowMillis = nowMillis;
            this.nowNanos = nowNanos;

            zeroToOneToZero1s += zeroToOneToZero1sDir * ds;
            if (zeroToOneToZero1s > 1f)
            {
                zeroToOneToZero1s -= (int)zeroToOneToZero1s;
                zeroToOneToZero1s = 1f - zeroToOneToZero1s;
                zeroToOneToZero1sDir = -1f;
            }
            else if (zeroToOneToZero1s < 0f)
            {
                zeroToOneToZero1s *= -1f;
                zeroToOneToZero1sDir = 1f;
            }
        }

        public double getSecondsSinceFirstUpdate()
        {
            return secondsSinceFirstUpdate;
        }

        public long getNowMillis()
        {
            return nowMillis;
        }

        public long getNowNanos()
        {
            return nowNanos;
        }

        public float getPendulum0To1s1()
        {
            return zeroToOneToZero1s;
        }
    }
}