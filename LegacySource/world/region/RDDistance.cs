using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace World.Region
{
    using Game.Boosting;
    using Game.Faction;
    using Game.Faction.Diplomacy;
    using Game.Faction.NPC;
    using Init.Sprite.UI;
    using Init.Value;
    using Snake2D.Pathing;
    using Snake2D.Util.File;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Sets;
    using Util.Data;
    using Util.Text;
    using World.Map.Pathing;
    using World.Map.Regions;
    using World.Region.RD.RDOutputs;

    public class RDDistance
    {
        private static readonly string ¤¤Name = "¤Proximity";
        private static readonly string ¤¤NameD = "¤Proximity is the physical distance from a region to your capital. It determines the amount tribute you receive from it and the loyalty of its subjects.";

        private static readonly string ¤¤Distance = "¤Distance";
        private static readonly string ¤¤DistanceD = "¤Distance to your capital. Distance affect trade prices.";

        private static readonly string ¤¤Borders = "¤Borders";
        private static readonly string ¤¤Reachable = "¤Reachable";

        private readonly INT_OE<Region> distance;
        private readonly INT_OE<Faction> factionBorders;
        private readonly INT_OE<Faction> factionReachable;
        private readonly INT_OE<Faction> factionBorderThroughAlly;
        private readonly INT_OE<Region> regionBorders;
        private readonly INT_OE<Region> regionReachable;
        public readonly Boostable bProximity;
        public readonly Boostable bProximityToll;

        private readonly ArrayList<FactionNPC> borders = new ArrayList<FactionNPC>(FACTIONS.MAX());
        private bool bDirty = true;

        private int[] dists = Alloc.Ii(FACTIONS.MAX());

        static RDDistance()
        {
            D.ts<RDDistance>();
        }

        public RDDistance(RDInit init)
        {
            distance = init.count.new DataShort("DISTANCE_DATA", ¤¤Distance, ¤¤DistanceD);
            factionReachable = init.rCount.new DataBit("DISTANCE_REACHABLE");
            factionBorders = init.rCount.new DataBit("DISTANCE_NEIGHBOURS");
            factionBorderThroughAlly = init.rCount.new DataBit("DISTANCE_NEIGHBOURS:ALLY");
            regionReachable = init.count.new DataBit("REGION_REACHABLE");
            regionBorders = init.count.new DataBit("REGION_NEIGHBOURS");
            dists.Fill(-1);
            bProximity = BOOSTING.Push("PROXIMITY", 0, ¤¤Name, ¤¤NameD, UI.icons().s.wheel, BoostableCat.ALL().WORLD);
            bProximityToll = BOOSTING.Push("PROXIMITY_TOLL", 1, ¤¤Name + " (" + Dic.¤¤Toll + ")", ¤¤NameD, UI.icons().s.wheel, BoostableCat.ALL().WORLD);

            new RBooster(new BSourceInfo(Dic.¤¤Distance, UI.icons().s.wheel), 0, 1, false)
            {
                public override double Get(Region t)
                {
                    return DistancePenalty(t);
                }
            }.Add(bProximity);

            BOOSTING.Connecter(new ACTION()
            {
                public override void Exe()
                {
                    RBooster bo = new RBooster(new BSourceInfo(Dic.¤¤Distance, UI.icons().s.wheel), 0.1, 1, true)
                    {
                        public override double Get(Region t)
                        {
                            if (t.Faction() != FACTIONS.Player())
                                return 0;
                            return CLAMP.D(bProximity.Get(t), 0, 1);
                        }
                    };

                    foreach (RDOutput o in RD.OUTPUT().ALL)
                    {
                        bo.Add(o.boost);
                        bo.Add(o.boostYearlyPart);
                    }
                }
            });

            init.savable.Add(new SAVABLE()
            {
                public void Save(FilePutter file)
                {
                    // TODO Auto-generated method stub
                }

                public void Load(FileGetter file) => bDirty = true;

                public void Clear() => bDirty = true;
            });

            new RD.RDOwnerChanger()
            {
                public void Change(Region reg, Faction oldOwner, Faction newOwner) => bDirty = true;
            };

            new DIP.DipActivityListener()
            {
                public void Change(Faction faction, Faction other, DipStance old, DipStance nn) => bDirty = true;
            };

            GVALUES.FACTION.Push("PLAYER_BORDERS", ¤¤Borders, UI.icons().s.wheel, new BOOLEANO<Faction>()
            {
                public bool Is(Faction t) => factionBorders.IsMax(t);
            });

            GVALUES.FACTION.Push("PLAYER_REACHABLE", ¤¤Reachable, UI.icons().s.wheel, new BOOLEANO<Faction>()
            {
                public bool Is(Faction t) => factionReachable.IsMax(t);
            });

            GVALUES.REGION.PushI("PLAYER_DISTANCE", ¤¤Distance, UI.icons().s.wheel, distance);
            GVALUES.REGION.Push("PLAYER_REACHABLE", ¤¤Reachable, UI.icons().s.wheel, new BOOLEANO<Region>()
            {
                public bool Is(Region t) => regionReachable.IsMax(t);
            });

            GVALUES.REGION.Push("PLAYER_BORDERS", ¤¤Borders, UI.icons().s.wheel, new BOOLEANO<Region>()
            {
                public bool Is(Region t) => regionBorders.IsMax(t);
            });
        }

        public double DistancePenalty(Region reg)
        {
            const double min = 60;
            const double half = 150;

            const double c = half - 2 * min;
            const double k = min + c;

            return CLAMP.D(k / (distance.Get(reg) + c), 0, 1);
        }

        private void Init()
        {
            if (!bDirty)
                return;

            Region cap = FACTIONS.Player().CapitolRegion();
            if (cap == null)
                return;

            WORLD.FOW().SetDirty();
            bDirty = false;

            foreach (Region reg in WORLD.REGIONS().All())
            {
                regionReachable.Set(reg, 0);
                regionBorders.Set(reg, 0);
                distance.SetD(reg, 0);
            }

            foreach (RegDist d in WORLD.PATH().regFinder.All(cap, Treaty.FACTION_REACHABLE, selTrade))
            {
                regionReachable.Set(d.reg, 1);
            }

            foreach (RegDist d in WORLD.PATH().regFinder.All(cap, Treaty.FACTION_BORDERS, selTrade))
            {
                regionBorders.Set(d.reg, 1);
                if (d.reg.Faction() != null)
                {
                    factionBorders.Set(d.reg.Faction(), 1);
                }
            }

            foreach (RegDist d in WORLD.PATH().regFinder.All(cap, Treaty.FACTION_BORDERS, selTrade))
            {
                regionBorders.Set(d.reg, 1);
                if (d.reg.Faction() != null)
                {
                    factionBorders.Set(d.reg.Faction(), 1);
                }
            }

            foreach (RegDist d in WORLD.PATH().regFinder.All(cap, Treaty.FACTION_CAN_ATTACK_ALLIES, selTrade))
            {
                factionBorderThroughAlly.Set(d.reg.Faction(), 1);
            }
        }

        public int Distance(Faction f)
        {
            Init();
            return distance.Get(f.CapitolRegion());
        }

        public INT_O<Region> Distance()
        {
            Init();
            return distance;
        }

        public bool Reachable(Region reg)
        {
            Init();
            return regionReachable.Get(reg) == 1;
        }

        public bool Neighbours(Region reg)
        {
            Init();
            return regionBorders.Get(reg) == 1;
        }

        public bool Reachable(Faction reg)
        {
            Init();
            return factionReachable.Get(reg) == 1;
        }

        public bool FactionHasRegionBorderingPlayer(Faction reg)
        {
            Init();
            return factionBorders.Get(reg) == 1;
        }

        public bool FactionCanAttackPlayerAllies(Faction reg)
        {
            Init();
            return factionBorderThroughAlly.Get(reg) == 1;
        }

        public LIST<RegDist> TradePartners(Faction start)
        {
            selTradeF = start;

            if (start is FactionNPC)
            {
                return WORLD.PATH().regFinder.All(start.CapitolRegion(), Treaty.FACTION_REACHABLE_NPC_TRADE, selTrade);
            }

            return WORLD.PATH().regFinder.All(start.CapitolRegion(), Treaty.FACTION_REACHABLE, selTrade);
        }

        public LIST<FactionNPC> Neighs()
        {
            Init();
            return borders;
        }

        public int CapitolDist(FactionNPC f)
        {
            if (dists[f.Index()] == -1)
            {
                PathTile d = WORLD.PATH().Path(FACTIONS.Player().Cx(), FACTIONS.Player().Cy(), f.Cx(), f.Cy(), Treaty.DUMMY);
                if (d == null)
                    dists[f.Index()] = 0;
                else
                {
                    dists[f.Index()] = (int)d.Value;
                }
            }
            return dists[f.Index()];
        }

        private Faction selTradeF;

        private readonly WRegSel selTrade = new WRegSel()
        {
            public bool Is(Region t)
            {
                if (t.Faction() == null)
                    return false;
                if (!t.Capitol())
                    return false;
                if (t.Faction() == selTradeF)
                    return false;
                if (DIP.Get(selTradeF, t.Faction()).Trades)
                    return true;
                if (selTradeF is FactionNPC && t.Faction() is FactionNPC)
                    return DIP.Get(selTradeF, t.Faction()) == DIP.NEUTRAL();
                return false;
            }
        };
    }
}