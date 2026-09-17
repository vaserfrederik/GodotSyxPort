using System;

namespace snake2d.util.datatypes
{
    public class Pendulum
    {
        private double current = -1;
        private double min = -1;
        private double max = 1;
        private double factor = 1;
        private double dir = 1;

        public Pendulum()
        {
        }

        public bool Update(double incr)
        {
            current += incr * factor * dir;
            bool change = false;
            while (true)
            {
                if (current > max)
                {
                    current = max - (current - max);
                    dir *= -1;
                    change = true;
                }
                else if (current < min)
                {
                    current = min - (current - min);
                    dir *= -1;
                    change = true;
                }
                else
                {
                    break;
                }
            }
            return change;
        }

        public Pendulum SetFactor(double factor)
        {
            if (factor < 0)
                throw new RuntimeException(" " + factor);
            this.factor = factor;
            return this;
        }

        public Pendulum SetMinMax(double min, double max)
        {
            this.min = min;
            if (min > max)
                throw new RuntimeException();
            if (max < 0 || max < min)
                throw new RuntimeException();
            this.max = max;
            if (current < min)
                current = min;
            if (current > max)
                current = max;
            return this;
        }

        public Pendulum SetZero(double max)
        {
            return SetMinMax(0, max);
        }

        public Pendulum SetMinMax(double bound)
        {
            SetMinMax(-bound, bound);
            return this;
        }

        public double Get()
        {
            return current;
        }
    }
}