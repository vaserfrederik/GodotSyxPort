using System;

namespace Game.Battle.Util
{
    class BattleFightingTest
    {
        /**
         * 0.3% end damage is nice
         */

        private const double CHANCE = 100;
        private const double CHANCE_MIN = 1.0 / CHANCE;
        private const double CHANCE_SPAN = CHANCE - CHANCE_MIN;

        public static void Main(string[] args)
        {
            RND.rInt();

            Attack a = new Attack();
            Defence d = new Defence();

            a.offence = 1;
            a.aim = 5;
            a.damage = 1;

            d.defence = 1;
            d.parry = 1;
            d.armour = 1;

            d.armourDir = 1;

            Perform("normal", a, d);

            Test();
        }

        private static void Perform(string name, Attack a, Defence d)
        {
            Console.WriteLine(name);
            a.Print();
            d.Print();

            Count c = new Count();
            double amount = 1000000;

            for (int i = 0; i < amount; i++)
            {
                double hit = a.offence / ((d.defence) * (CHANCE_MIN + RND.rFloat() * CHANCE_SPAN));

                if (hit > RND.rFloat())
                {
                    c.hits++;
                    double damage = a.damage;
                    double block = a.aim / ((d.parry) * (CHANCE_MIN + RND.rFloat() * CHANCE_SPAN));

                    if (block > RND.rFloat())
                    {
                        c.bypass++;
                    }
                    else
                    {
                        damage /= (d.armourDir);
                    }

                    double r = damage / ((d.armour) * (CHANCE_MIN + RND.rFloat() * CHANCE_SPAN));

                    if (r > RND.rFloat())
                    {
                        c.impacts++;
                        c.damage += r;
                        if (r > 1)
                            c.deaths++;
                    }
                }
            }

            c.Print(amount);

            Average(a, d);
        }

        static double GetDamage(Attack a, Defence d)
        {
            double hit = AverageOver1(a.offence, d.defence, CHANCE_MIN, CHANCE_SPAN);

            hit = Clamp(hit, 0, 1);

            double unblocked = AverageOver1(a.aim, d.parry, CHANCE_MIN, CHANCE_SPAN);
            unblocked = Clamp(unblocked, 0, 1);

            double damageBlocked = (unblocked) * hit * AverageAddative(a.damage, d.armour, CHANCE_MIN, CHANCE_SPAN);
            double damageUnblocked = (1.0 - unblocked) * hit * AverageAddative(a.damage / (d.armourDir), d.armour, CHANCE_MIN, CHANCE_SPAN);

            double damage = damageBlocked + damageUnblocked;
            return damage;
        }

        private static void Average(Attack a, Defence d)
        {
            Count c = new Count();

            double hit = AverageOver1(a.offence, d.defence, CHANCE_MIN, CHANCE_SPAN);

            hit = Clamp(hit, 0, 1);
            c.hits += hit * 1000;

            double unblocked = AverageOver1(a.aim, d.parry, CHANCE_MIN, CHANCE_SPAN);
            unblocked = Clamp(unblocked, 0, 1);
            c.bypass += hit * 1000 * unblocked;

            double damageBlocked = (unblocked) * hit * AverageAddative(a.damage, d.armour, CHANCE_MIN, CHANCE_SPAN);
            double damageUnblocked = (1.0 - unblocked) * hit * AverageAddative(a.damage / (d.armourDir), d.armour, CHANCE_MIN, CHANCE_SPAN);

            double damage = damageBlocked + damageUnblocked;

            c.damage += 1000 * (damage);

            c.Print(1000);
        }

        private static double AverageAddative(double A, double B, double DAMAGE_MIN, double DAMAGE_SPAN)
        {
            double C = A / B;
            double m = DAMAGE_MIN;
            double s = DAMAGE_SPAN;
            double h = m + s;

            if (C <= m)
            {
                return C * C / (s * m * h);
            }
            else if (C >= h)
            {
                return C * Math.Log(h / m) / s;
            }
            else
            {
                return (C * Math.Log(C / m) + C - C * C / h) / s;
            }
        }

        private static double AverageOver1(double A, double B, double DAMAGE_MIN, double DAMAGE_SPAN)
        {
            double C = A / B;
            double m = DAMAGE_MIN;
            double s = DAMAGE_SPAN;
            double h = m + s;
            return (C <= m) ? C * Math.Log(h / m) / s : (C >= h) ? 1.0 : ((C - m) + C * Math.Log(h / C)) / s;
        }

        private static void Test()
        {
            double amount = 1000000;

            double DAMAGE_MIN = 1.0 / 100;
            double DAMAGE_SPAN = 100 - DAMAGE_MIN;

            double A = 1;
            double B = 1;

            double totA = 0;
            for (int i = 0; i < amount; i++)
            {
                double hit = A / ((B + 1) * (DAMAGE_MIN + RND.rFloat() * DAMAGE_SPAN));
                if (hit > RND.rFloat()) totA++;
            }
            Console.WriteLine("A (sim): " + totA / amount);
            Console.WriteLine("A (ana): " + AverageOver1(A, B + 1, DAMAGE_MIN, DAMAGE_SPAN));

            double totB = 0;
            for (int i = 0; i < amount; i++)
            {
                double hit = A / (B * (DAMAGE_MIN + RND.rFloat() * DAMAGE_SPAN));
                if (hit > RND.rFloat())
                    totB += hit;
            }
            Console.WriteLine("B (sim): " + totB / amount);
            Console.WriteLine("B (ana): " + AverageAddative(A, B, DAMAGE_MIN, DAMAGE_SPAN));
        }

        static class Attack
        {
            public double offence = 1;
            public double aim = 5;
            public double damage = 1;

            public void Print()
            {
                Console.WriteLine("attacker");
                Console.WriteLine("  offence   " + offence);
                Console.WriteLine("  aim   " + aim);
                Console.WriteLine("  damage  " + damage);
            }
        }

        static class Defence
        {
            public double defence = 1;
            public double parry = 1;
            public double armourDir = 1;
            public double armour = 1;

            public void Print()
            {
                Console.WriteLine("defender");
                Console.WriteLine("  defence   " + defence);
                Console.WriteLine("  parry   " + parry);
                Console.WriteLine("  shield  " + armourDir);
                Console.WriteLine("  armour  " + armour);
            }
        }

        private static class Count
        {
            public double hits = 0;
            public double bypass = 0;
            public double impacts = 0;
            public double deaths = 0;
            public double damage = 0;

            public void Print(double amount)
            {
                Console.WriteLine("result");
                Console.WriteLine("  Hitrate  " + (int)(1000 * (hits / amount)) / 10.0 + "%");
                Console.WriteLine("  Bypass   " + (int)(1000 * (bypass / hits)) / 10.0 + "%");
                Console.WriteLine("  impacts   " + (int)(1000 * (impacts / hits)) / 10.0 + "%");
                Console.WriteLine("     damage   " + (int)(1000 * (damage / hits)) / 10.0 + "%");
                Console.WriteLine("     deaths   " + (int)(1000 * (deaths / hits)) / 10.0 + "%");

                Console.WriteLine("  tot damage   " + (int)(10000 * (damage / amount)) / 100.0 + "%");
                Console.WriteLine("  tot deaths   " + (int)(10000 * (deaths / amount)) / 100.0 + "%");
            }
        }

        static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}