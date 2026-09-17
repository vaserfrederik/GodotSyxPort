using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace World.Battle
{
    using Game.Faction;
    using Snake2D.Log;
    using Snake2D.Util.File;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Sets;
    using Snake2D.Util.Sprite.Text;
    using Util.Text;
    using View.Ui.Message;
    using World;
    using World.Army;
    using World.Battle.Side;
    using World.Entity.Army;
    using World.Map.Regions;
    using World.Region;

    internal class PSiege : ISavable
    {
        private readonly ArrayListInt besieged = new ArrayListInt(WREGIONS.MAX);
        private readonly Bitmap1D besigedMap = new Bitmap1D(WREGIONS.MAX, false);
        private readonly Bitmap1D first = new Bitmap1D(WREGIONS.MAX, false);
        private readonly double[] besigeTime = new double[WREGIONS.MAX];
        private readonly ArrayListInt active = new ArrayListInt(WREGIONS.MAX);
        private readonly Bitmap1D map = new Bitmap1D(WREGIONS.MAX, false);
        private double dd;
        private static readonly double dTime = 128;
        private static readonly double dTimeI = WREGIONS.MAX / dTime;

        private readonly Util util;
        private readonly Conflict conflict;
        private readonly Resolver resolver;
        private int playerRegAttackRegion = -1;

        private static readonly CharSequence ¤¤name = "Besieged";
        private static readonly CharSequence ¤¤desc = "The city of {0} have been besieged by our enemies!";

        static PSiege()
        {
            D.ts(typeof(PSiege));
        }

        public PSiege(Util util, Conflict conflict, Resolver resolver)
        {
            this.util = util;
            this.conflict = conflict;
            this.resolver = resolver;
        }

        public void Save(FilePutter file)
        {
            besieged.Save(file);
            besigedMap.Save(file);
            file.Ds(besigeTime);
            file.D(dd);
            active.Save(file);
            map.Save(file);
            first.Save(file);
        }

        public void Load(FileGetter file)
        {
            besieged.Load(file);
            besigedMap.Load(file);
            file.Ds(besigeTime);
            dd = file.D();
            active.Load(file);
            map.Load(file);
            first.Load(file);
        }

        public void Clear()
        {
            besieged.Clear();
            besigedMap.Clear();
            besigeTime.Fill(0);
            dd = 0;
            active.Clear();
            map.Clear();
            first.Clear();
        }

        public void Register(WArmy a)
        {
            Region reg = a.Besieging();
            if (reg != null && !map.Get(reg.Index()))
            {
                map.Set(reg.Index(), true);
                first.Set(reg.Index(), true);
                active.Add(reg.Index());
            }
        }

        public void PlayerBesige(Region reg)
        {
            playerRegAttackRegion = reg.Index();
        }

        public void Update(double ds)
        {
            for (int i = 0; i < besieged.Size(); i++)
            {
                int ri = besieged.Get(i);
                Region reg = WORLD.REGIONS().GetByIndex(ri);
                if (util.GetBesieger(reg) == null)
                {
                    besieged.Remove(i);
                    besigedMap.Set(ri, false);
                    i--;
                }
            }

            int current = (int)dd;
            dd += ds * dTimeI;
            int next = (int)dd;

            while (current < next)
            {
                int ri = current % WREGIONS.MAX;
                current++;
                Region reg = WORLD.REGIONS().GetByIndex(ri);
                if (!reg.Active())
                    continue;
                WArmy a = util.GetBesieger(reg);
                if (a != null)
                {
                    besigeTime[ri] += dTime;
                }
                else
                {
                    besigeTime[ri] -= dTime * 4;
                }
                besigeTime[ri] = Clamp.D(besigeTime[ri], 0, double.MaxValue / 2);
            }

            while (dd >= WREGIONS.MAX)
                dd -= WREGIONS.MAX;
        }

        public double BesigedTime(Region reg)
        {
            return besigeTime[reg.Index()];
        }

        public bool Besiged(Region reg)
        {
            return besigedMap.Get(reg.Index());
        }

        public void Besige(WArmy a, Region reg)
        {
            if (!besigedMap.Get(reg.Index()))
            {
                besigedMap.Set(reg.Index(), true);
                besieged.Add(reg.Index());
                if (reg.Faction() == FACTIONS.Player() && !reg.Capitol())
                {
                    double regPow = RD.MILITARY().DefensePower(reg);
                    double aPow = AD.Power().Get(a);

                    Pair allies = util.Fill(a.Faction(), reg.Faction(), reg.Cx(), reg.Cy());
                    foreach (WArmy a2 in allies.A)
                    {
                        if (a2 != a)
                            aPow += AD.Power().Get(a2);
                    }

                    if (regPow > aPow)
                    {
                        new MessageText(¤¤name, Str.TMP.Clear().Add(¤¤desc).Insert(0, reg.Info.Name())).Send();
                    }
                }
            }

            if (a.Faction() == FACTIONS.Player())
                playerRegAttackRegion = reg.Index();
        }

        public bool Poll()
        {
            if (playerRegAttackRegion >= 0)
            {
                Region reg = WORLD.REGIONS().GetByIndex(playerRegAttackRegion);
                playerRegAttackRegion = -1;
                WArmy a = util.GetBesieger(reg);

                if (a != null && a.Faction() == FACTIONS.Player())
                {
                    if (Create(a, reg, true))
                        return true;
                }
            }

            int death = 1000;

            while (!active.IsEmpty())
            {
                int ai = active.Get(active.Size() - 1);
                Region reg = WORLD.REGIONS().All().Get(ai);
                bool f = first.Get(reg.Index());
                first.Set(reg.Index(), false);
                if (Create(reg, f))
                    return true;
                else
                {
                    map.Set(active.Get(active.Size() - 1), false);
                    active.Remove(active.Size() - 1);
                }
                if (death-- < 0)
                {
                    LOG.Err("NOHA!");
                    break;
                }
            }
            return false;
        }

        private bool Create(Region reg, bool first)
        {
            if (reg == null)
                return false;

            WArmy a = util.GetBesieger(reg);
            if (a != null && a.Faction() != FACTIONS.Player())
                return Create(a, reg, first);
            return false;
        }

        private bool Create(WArmy a, Region reg, bool first)
        {
            if (!Util.Enemies(reg.Faction(), a.Faction()))
                return false;

            if (a.Besieging() != reg)
                return false;

            double regPow = RD.MILITARY().DefensePower(reg);
            double aPow = AD.Power().Get(a);

            Pair allies = util.Fill(a.Faction(), reg.Faction(), reg.Cx(), reg.Cy());
            foreach (WArmy a2 in allies.A)
            {
                if (a2 != a)
                    aPow += AD.Power().Get(a2);
            }

            foreach (WArmy a2 in allies.B)
            {
                regPow += AD.Power().Get(a2);
            }

            if (a.Faction() != FACTIONS.Player() && regPow > aPow)
                return false;

            conflict.Clear();
            conflict.A.Add(a);

            foreach (WArmy a2 in allies.A)
            {
                if (a != a2)
                    conflict.A.Add(a2);
            }
            conflict.B.Add(reg);
            foreach (WArmy a2 in allies.B)
            {
                conflict.B.Add(a2);
            }

            return resolver.Besige(conflict.A, conflict.B, first);
        }
    }
}