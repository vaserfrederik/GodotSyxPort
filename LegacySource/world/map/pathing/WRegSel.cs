using System;

namespace World.Map.Pathing
{
    using Game.Faction;
    using Game.Faction.Diplomacy;
    using Util.Data;
    using World.Map.Regions;

    public abstract class WRegSel : BOOLEANO<Region>
    {
        public WRegSel()
        {
        }

        private static Region home;
        private static Faction faction;

        private static readonly WRegSel DUMDUM = new WRegSel()
        {
            public override bool Is(Region t)
            {
                return true;
            }
        };

        private static readonly WRegSel DUMMY = new WRegSel()
        {
            public override bool Is(Region t)
            {
                return t != home;
            }
        };

        private static readonly WRegSel CAPITOLS = new WRegSel()
        {
            public override bool Is(Region t)
            {
                return t.Capitol();
            }
        };

        private static readonly WRegSel SINGLE = new WRegSel()
        {
            public override bool Is(Region t)
            {
                return t == home;
            }
        };

        private static readonly WRegSel FACTION = new WRegSel()
        {
            public override bool Is(Region t)
            {
                return t.Faction() == faction;
            }
        };

        private static readonly WRegSel ENEMYFACTION = new WRegSel()
        {
            public override bool Is(Region t)
            {
                if (faction == null)
                    return t.Faction() == FACTIONS.Player();
                return t.Faction() != null && DIP.WAR().Is(t.Faction(), faction);
            }
        };

        private static readonly WRegSel ENEMY = new WRegSel()
        {
            public override bool Is(Region t)
            {
                return DIP.WAR().Is(t.Faction(), faction);
            }
        };

        public static WRegSel DUMMY()
        {
            return DUMDUM;
        }

        public static WRegSel DUMMY(Region home)
        {
            WRegSel.home = home;
            return DUMMY;
        }

        public static WRegSel CAPITOLS()
        {
            return CAPITOLS;
        }

        public static WRegSel SINGLE(Region home)
        {
            WRegSel.home = home;
            return SINGLE;
        }

        public static WRegSel FACTION(Faction home)
        {
            WRegSel.faction = home;
            return FACTION;
        }

        public static WRegSel ENEMYFACTION(Faction home)
        {
            WRegSel.faction = home;
            return ENEMYFACTION;
        }

        public static WRegSel ENEMY(Faction home)
        {
            WRegSel.faction = home;
            return ENEMY;
        }
    }
}