using System;
using System.Threading;

namespace snake2d
{
    class Updater : Thread
    {
        private const long nanoMax = (long)(1000000000 * CORE.UPDATE_SECONDS_MAX);
        private const long nanoMin = (long)(1000000000 * CORE.UPDATE_SECONDS_MIN);
        private float secondsRender;

        private long maxAmount;
        private long minAmount;
        private long nanoAccumilator;

        private long nowTemp;

        private long lastUpdate;
        private long lastRender;

        private readonly CORE_STATE.Constructor constructor;
        private CORE_STATE current;
        private volatile bool hasTheRightToLive = true;

        private readonly CoreTime info = new CoreTime();

        private double slowDown = 1.0;

        public Updater(CORE_STATE.Constructor current)
        {
            this.constructor = current;
        }

        public override void Run()
        {
            Thread.CurrentThread.Name = "updater";

            this.current = constructor.GetState();

            constructor.DoAfterSet();
            GC.Collect();
            //current.hover(CORE.GetInput().GetMouse().GetCoo(), true);

            nanoAccumilator = 0;
            lastUpdate = DateTime.Now.Ticks;
            lastRender = lastUpdate;

            while (CORE.IsRunning() && hasTheRightToLive)
            {
                long now = DateTime.Now.Ticks;

                if (hasTheRightToLive)
                    Update();
                if (hasTheRightToLive)
                    CORE.GetInput().Poll(current);
                if (hasTheRightToLive)
                {
                    Render();
                }

                now = DateTime.Now.Ticks - now;
                double d = now / 1000000000.0;
                double f = 1.0 / 40;

                slowDown = d / f;
                slowDown = Math.Clamp(slowDown, 0, slowDown);

                if (hasTheRightToLive)
                {
                    CORE.SwapAndPoll();
                    CoreStats.EndOfLoopCalc();
                }
            }

            current.Exit();
        }

        private void Render()
        {
            nowTemp = DateTime.Now.Ticks;
            secondsRender = (nowTemp - lastRender) / 1000000000f;
            lastRender = nowTemp;
            current.Render(CORE.Renderer(), secondsRender);
            CoreStats.RenderPercentage.Set(DateTime.Now.Ticks - nowTemp);
        }

        private void Update()
        {
            nanoAccumilator += DateTime.Now.Ticks - lastUpdate;
            lastUpdate = DateTime.Now.Ticks;
            maxAmount = nanoAccumilator / nanoMax;
            if (maxAmount > 0)
            {
                nanoAccumilator = 0;
            }
            else
            {
                minAmount = nanoAccumilator / nanoMin;
                nanoAccumilator -= minAmount * nanoMin;
            }

            nowTemp = DateTime.Now.Ticks;
            float total = CORE.UPDATE_SECONDS_MAX * maxAmount + CORE.UPDATE_SECONDS_MIN * minAmount;
            info.Update(total, nowTemp / 1000000, nowTemp);

            nowTemp = DateTime.Now.Ticks;
            if (maxAmount > 0)
            {
                maxAmount--;
                current.Update(CORE.UPDATE_SECONDS_MAX, slowDown);
            }
            else
            {
                current.Update(CORE.UPDATE_SECONDS_MIN * minAmount, slowDown);
            }
            CoreStats.DroppedTicks.Set(maxAmount);
            CoreStats.SmallUpdates.Set(minAmount);

            CoreStats.UpdatePercentage.Set(DateTime.Now.Ticks - nowTemp);
        }

        public void DieHard()
        {
            hasTheRightToLive = false;
        }

        public CoreTime GetCoreInfo()
        {
            return info;
        }
    }
}