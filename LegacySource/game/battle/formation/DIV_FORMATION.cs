using System;
using System.Collections.Generic;

namespace Game.Battle.Formation
{
    using Game.Battle.Div;
    using Game.Battle.Util;
    using Settlement.Stats;
    using Settlement.Stats.Equip;
    using Snake2D.Util.Sets;

    public enum DIV_FORMATION
    {
        TIGHT = 1.2,
        LOOSE = 1.75
    }

    public static class DIV_FORMATION_Extensions
    {
        public static readonly LIST<DIV_FORMATION> All = new ArrayList<DIV_FORMATION>(Enum.GetValues(typeof(DIV_FORMATION)));

        private static readonly Dictionary<DIV_FORMATION, double> sizeMap = new Dictionary<DIV_FORMATION, double>
        {
            { DIV_FORMATION.TIGHT, 1.2 },
            { DIV_FORMATION.LOOSE, 1.75 }
        };

        public static int Size(this DIV_FORMATION formation, DIV_SPEC div)
        {
            int am = 0;
            for (int i = 0; i < STATS.EQUIP().BATTLE_ALL().Count; i++)
            {
                EquipBattle b = STATS.EQUIP().BATTLE_ALL()[i];
                if (div.EquipI(b) > 0)
                    am = Math.Max(b.formationAdd, am);
            }

            return (int)((div.Race().Physics.HitBoxSize() + am) * sizeMap[formation]);
        }

        public static int SizeH(this DIV_FORMATION formation, DIV_SPEC div)
        {
            return formation.Size(div) / 2;
        }

        public static int Size(this DIV_FORMATION formation, Div div)
        {
            int am = 0;
            for (int i = 0; i < STATS.EQUIP().BATTLE_ALL().Count; i++)
            {
                EquipBattle b = STATS.EQUIP().BATTLE_ALL()[i];
                if (b.Stat().Div().Get(div) > 0)
                    am = Math.Max(b.formationAdd, am);
            }
            return (int)((div.Race().Physics.HitBoxSize() + am) * sizeMap[formation]);
        }
    }
}