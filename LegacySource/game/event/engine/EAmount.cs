using System;
using Snake2D.Util.File;

namespace Game.Event.Engine
{
    class EAmount
    {
        public double Rel = 0;
        public double PerPerson = 0;
        public double Abs = 0;

        public EAmount(int am)
        {
            this.Abs = am;
        }

        public EAmount(Json json, int min)
        {
            Rel = json.dTry("RELATIVE", min, 1000, 0);
            PerPerson = json.dTry("PER_PERSON", min, 1000, 0);
            Abs = json.dTry("AMOUNT", min, int.MaxValue, 0);
            json.checkUnused();
        }

        public int Am(double perPerson, double rel)
        {
            double am = Abs;
            am += perPerson * this.PerPerson;
            am += rel * this.Rel;
            return (int)Math.Ceiling(am);
        }
    }
}