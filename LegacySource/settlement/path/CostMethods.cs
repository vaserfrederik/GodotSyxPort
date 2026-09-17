using System;

namespace Settlement.Path
{
    using static Settlement.Main.SETT;

    public class CostMethods
    {
        public CostMethods()
        {
        }

        public readonly COST Player = new COST()
        {
            GetCost = (fromX, fromY, toX, toY) =>
            {
                AVAILABILITY a = PATH().GetAvailability(toX, toY);
                if (a.Player < 0)
                {
                    return BLOCKED;
                }
                if (fromX != toX && fromY != toY)
                {
                    if (PATH().GetAvailability(fromX, toY).Player <= -1 || PATH().GetAvailability(toX, fromY).Player <= -1)
                    {
                        return SKIP;
                    }
                }

                return a.Player + PATH().GetAvailability(fromX, fromY).From;
            }
        };

        public readonly COST Enemy = new COST()
        {
            GetCost = (fromX, fromY, toX, toY) =>
            {
                AVAILABILITY a = PATH().GetAvailability(toX, toY);
                if (a.Enemy < 0)
                {
                    return BLOCKED;
                }
                if (fromX != toX && fromY != toY)
                {
                    if (PATH().GetAvailability(fromX, toY).Enemy <= -1 || PATH().GetAvailability(toX, fromY).Enemy <= -1)
                    {
                        return SKIP;
                    }
                }

                return a.Enemy;
            }
        };
    }
}