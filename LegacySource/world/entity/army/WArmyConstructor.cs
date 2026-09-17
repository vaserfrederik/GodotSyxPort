using System;
using System.Collections.Generic;
using System.IO;
using Game.Faction;
using Game.Faction.Diplomacy;
using Init.Race;
using Snake2D.Util.Rnd;
using Snake2D.Util.Sets;
using Util.Text;
using View.Tool;
using View.World.Panel;
using World;
using World.Army;
using World.Entity;

namespace World.Entity.Army
{
    public sealed class WArmyConstructor : WEntityConstructor<WArmy>
    {
        private readonly Stack<WArmy> free = new Stack<WArmy>(512);
        private WArmy[] all = new WArmy[128];
        private int amount = 0;
        public readonly WArmySprite sprite = new WArmySprite();
        private bool tmpStop;
        public const int MAX = 1024;

        public WArmyConstructor(LISTE<WEntityConstructor<? extends WEntity>> all) : base(all, true)
        {
            IDebugPanelWorld.Add(new PlacableSingle("ArmyBig")
            {
                public override void PlaceFirst(int tx, int ty)
                {
                    if (!FACTIONS.Player().Armies().CanCreate())
                        return;
                    WArmy e = Create(tx, ty, FACTIONS.Player());
                    for (int i = 0; i <= 100; i++)
                    {
                        WDivRegional d = AD.Regional().Create(RACES.All().Rnd(), 0.25 + 0.75 * (1.0 - (1 - RND.RFloatP(2))), e);
                        d.Randomize(RND.RFloat(), RND.RFloat());
                        d.MenSet(d.MenTarget());
                    }
                    AD.Supplies().FillAll(e);
                }

                public override CharSequence IsPlacable(int tx, int ty)
                {
                    return WORLD.PATH().Map.Is.Is(tx, ty) ? null : E;
                }
            });

            IDebugPanelWorld.Add(new PlacableSingle("ArmySmall")
            {
                public override void PlaceFirst(int tx, int ty)
                {
                    WArmy e = Create(tx, ty, FACTIONS.Player());
                    for (int i = 0; i <= 1; i++)
                    {
                        WDivRegional d = AD.Regional().Create(RACES.All().Rnd(), 0.25 + 0.75 * (1.0 - (1 - RND.RFloatP(2))), e);
                        d.Randomize(RND.RFloat(), RND.RFloat());
                        d.MenSet(d.MenTarget());
                    }
                    AD.Supplies().FillAll(e);
                }

                public override CharSequence IsPlacable(int tx, int ty)
                {
                    return WORLD.PATH().Map.Is.Is(tx, ty) ? null : E;
                }
            });

            IDebugPanelWorld.Add(new PlacableSingle("ArmyEnemyBig")
            {
                public override void PlaceFirst(int tx, int ty)
                {
                    WArmy e = Create(tx, ty, null);
                    AD.FactionSet(e, null);
                    for (int i = 0; i <= 100; i++)
                    {
                        WDivRegional d = AD.Regional().Create(RACES.All().Rnd(), 0.25 + 0.75 * (1.0 - (1 - RND.RFloatP(2))), e);
                        d.Randomize(RND.RFloat(), RND.RFloat());
                        d.MenSet(d.MenTarget());
                    }
                    AD.Supplies().FillAll(e);
                    AD.UpdateArmy(e);
                }

                public override CharSequence IsPlacable(int tx, int ty)
                {
                    return WORLD.PATH().Map.Is.Is(tx, ty) ? null : E;
                }
            });

            IDebugPanelWorld.Add(new PlacableSingle("ArmyEnemySmall")
            {
                public override void PlaceFirst(int tx, int ty)
                {
                    WArmy e = Create(tx, ty, null);
                    WDivRegional d = AD.Regional().Create(RACES.All().Rnd(), 0.25 + 0.75 * (1.0 - (1 - RND.RFloatP(2))), e);
                    d.Randomize(RND.RFloat(), RND.RFloat());
                    d.MenSet(d.MenTarget());
                    AD.Supplies().FillAll(e);
                    AD.UpdateArmy(e);
                }

                public override CharSequence IsPlacable(int tx, int ty)
                {
                    return WORLD.PATH().Map.Is.Is(tx, ty) ? null : E;
                }
            });

            IDebugPanelWorld.Add(new PlacableSingle("ArmyEnemyFaction")
            {
                public override void PlaceFirst(int tx, int ty)
                {
                    Faction f = FACTIONS.NPCs().Get(0);
                    if (f.CapitolRegion() == null)
                        return;
                    WArmy e = Create(tx, ty, f);
                    for (int i = 0; i <= 50; i++)
                    {
                        WDivRegional d = AD.Regional().Create(RACES.All().Rnd(), 0.25 + 0.75 * (1.0 - (1 - RND.RFloatP(2))), e);
                        d.Randomize(RND.RFloat(), RND.RFloat());
                        d.MenSet(d.MenTarget());
                    }
                    AD.Supplies().FillAll(e);
                    DIP.WAR().Set(f, FACTIONS.Player());
                }

                public override CharSequence IsPlacable(int tx, int ty)
                {
                    return WORLD.PATH().Map.Is.Is(tx, ty) ? null : E;
                }
            });
        }

        protected override WArmy Create()
        {
            if (!free.IsEmpty())
                return free.Pop();
            return new WArmy();
        }

        public bool TmpStop()
        {
            return tmpStop;
        }

        public int Armies()
        {
            return amount;
        }

        public int Max()
        {
            return all.Length;
        }

        public WArmy Get(int index)
        {
            return all[index];
        }

        public WArmy TryGet(int index)
        {
            if (index < 0 || index > all.Length)
                return null;
            return all[index];
        }

        public bool CanCreate()
        {
            return amount < MAX && WORLD.ENTITIES().CanAdd(fast);
        }

        public WArmy Create(int tx, int ty, Faction f)
        {
            if (!CanCreate())
            {
                throw new RuntimeException("too many armies on the map!" + " " + amount + " " + WORLD.ENTITIES().AllFast().Size());
            }

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null)
                {
                    return Add(Create(), i, tx, ty, f);
                }
            }

            int nsize = all.Length + 64;
            if (nsize > short.MaxValue)
                nsize = short.MaxValue;
            if (nsize <= all.Length)
                throw new RuntimeException();

            WArmy[] newAll = new WArmy[nsize];
            for (int i = 0; i < all.Length; i++)
            {
                newAll[i] = all[i];
            }
            int i = all.Length;
            all = newAll;
            return Add(Create(), i, tx, ty, f);
        }

        public void Ret(WArmy wArmyEntity)
        {
            all[wArmyEntity.Index] = null;
            if (!free.IsFull())
                free.Push(wArmyEntity);
        }

        private WArmy Add(WArmy a, int index, int tx, int ty, Faction f)
        {
            a.Index = (short)index;
            all[index] = a;
            a.Init(tx, ty, f);
            if (!a.Added())
                throw new RuntimeException();
            amount++;

            a.Name.Clear();
            if (f != null)
            {
                a.Name.Add(Dic.¤¤Army).S().Add(f.Armies().All().Size());
            }
            else
            {
                a.Name.Add(Dic.¤¤Army);
            }
            return a;
        }

        public WArmy Load(WArmy a)
        {
            if (a.Index > all.Length)
            {
                WArmy[] nn = all;
                while (a.Index > nn.Length)
                    nn = new WArmy[nn.Length + 64];
                for (int i = 0; i < all.Length; i++)
                    nn[i] = all[i];
                all = nn;
            }
            all[a.Index] = a;
            amount++;
            return a;
        }

        protected override void Clear()
        {
            all = new WArmy[128];
            amount = 0;
        }
    }
}