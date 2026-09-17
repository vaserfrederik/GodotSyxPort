using System.Collections.Generic;
using System.Text;

namespace World.Region
{
    using Snake2D.Util.Sets;
    using Snake2D.Util.Sprite.Text;
    using Util.Text;
    using World.Map.Regions;
    using World.Region.Building;

    public class RDProblem
    {
        private static string ¤¤efficiency = "The {0} is operating at low efficiency.";
        private static string ¤¤popDecline = "The population is declining.";
        private static string ¤¤loyalty = "Loyalty is at critical levels.";
        private static string ¤¤health = "Health is at critical levels.";

        static RDProblem()
        {
            D.ts(typeof(RDProblem));
        }

        private ArrayListGrower<RDBuilding> notis = new ArrayListGrower<RDBuilding>();
        private readonly StringBuilder str = new StringBuilder(128);

        public RDProblem()
        {
            foreach (RDBuilding b in RD.BUILDINGS().All)
            {
                if (b.Notify)
                {
                    notis.Add(b);
                }
            }
        }

        public string Problem(Region reg)
        {
            foreach (RDBuilding bu in notis)
            {
                if (bu.Efficiency.Get(reg) < 1)
                {
                    str.Clear().AppendFormat(¤¤efficiency, bu.Info.Name);
                    return str.ToString();
                }
            }

            if (RD.RACES().PopTarget.GetD(reg) < RD.RACES().Population.Get(reg))
            {
                return str.Clear().Append(¤¤popDecline).ToString();
            }

            if (RD.RACES().LoyaltyAll.GetD(reg) < 0)
            {
                return str.Clear().Append(¤¤loyalty).ToString();
            }

            if (RD.HEALTH().Boostablee.Get(reg) < 0.5)
            {
                return str.Clear().Append(¤¤health).ToString();
            }
            return null;
        }
    }
}