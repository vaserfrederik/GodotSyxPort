using System;
using System.IO;

namespace game
{
    public sealed class GameSpeed
    {
        private bool tmpPaused;
        private double speed;
        private double prevSpeed;
        private double actualSpeed;
        private double actualSpeedI;
        private bool updateOnce;

        public readonly int speed0 = 0;
        public readonly double speed05 = 0.25;
        public readonly int speed1 = 1;
        public readonly int speed2 = 5;
        public readonly int speed3 = 25;
        public readonly int speed4 = 250;

        public GameSpeed()
        {
        }

        public double Update(double slowTheFuckDown)
        {
            if (tmpPaused)
                return ClearAndReturn(0);

            if (updateOnce)
                return ClearAndReturn(1);

            double s = speed;
            if (GAME.ARMIES().Enemy().Men() > 0)
                s = Math.Min(s, 25);

            if (actualSpeed < s && slowTheFuckDown < 1)
            {
                actualSpeed++;
            }
            else if (slowTheFuckDown >= 1)
            {
                actualSpeed /= 1.0 + (slowTheFuckDown - 1.0) * 0.5;
            }

            actualSpeed = Math.Min(Math.Max(actualSpeed, 1), s);
            if (actualSpeed > s)
                actualSpeed = s;
            if (actualSpeed < 1 && s >= 1)
                actualSpeed = 1;

            actualSpeedI = 1.0 / Math.Min(Math.Max(actualSpeed, 1), 1000);

            return ClearAndReturn(actualSpeed);
        }

        private double ClearAndReturn(double i)
        {
            tmpPaused = false;
            updateOnce = false;
            return i;
        }

        public bool IsPaused()
        {
            return speed == 0 || tmpPaused;
        }

        public double SpeedTarget()
        {
            return tmpPaused ? 0 : speed;
        }

        public void UpdateOnce()
        {
            updateOnce = true;
        }

        public void TmpPause()
        {
            tmpPaused = true;
        }

        public void TogglePause()
        {
            if (speed == 0)
            {
                if (prevSpeed == 0)
                    prevSpeed = 1;
                speed = prevSpeed;
            }
            else
            {
                prevSpeed = speed;
                speed = 0;
            }
            actualSpeed = speed;
            actualSpeedI = 1.0 / Math.Min(Math.Max(actualSpeed, 1), 1000);
        }

        public double Speed()
        {
            return actualSpeed;
        }

        public double SpeedI()
        {
            return actualSpeedI;
        }

        public void SpeedSet(double speed)
        {
            this.prevSpeed = this.speed;
            this.speed = speed;
            this.actualSpeed = speed;
            actualSpeedI = 1.0 / Math.Min(Math.Max(actualSpeed, 1), 1000);
        }

        public void Save(BinaryWriter file)
        {
            file.Write(speed);
        }

        public void Load(BinaryReader file)
        {
            prevSpeed = file.ReadDouble();
            Clear();
        }

        public void Clear()
        {
            speed = 0;
            actualSpeed = 0;
            tmpPaused = false;
            updateOnce = false;
        }

        public void Poll()
        {
            if (KEYS.MAIN().PAUSE.ConsumeClick())
            {
                TogglePause();
            }
            if (KEYS.MAIN().SPEED1.ConsumeClick())
            {
                if (speed == speed1)
                    SpeedSet(speed05);
                else
                    SpeedSet(speed1);
            }
            if (KEYS.MAIN().SPEED2.ConsumeClick())
            {
                SpeedSet(speed2);
            }
            if (KEYS.MAIN().SPEED3.ConsumeClick())
            {
                if (speed == 25)
                    SpeedSet(speed4);
                else
                    SpeedSet(speed3);
            }
        }
    }
}