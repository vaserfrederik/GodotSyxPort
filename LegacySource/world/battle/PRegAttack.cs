using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.file;
using snake2d.util.sets;
using world;
using world.army;
using world.battle.Util;
using world.entity.army;
using world.map.regions;
using world.region;

namespace world.battle
{
    class PRegAttack : SAVABLE
    {
        private readonly ArrayListInt active = new ArrayListInt(WREGIONS.MAX);
        private readonly Bitmap1D map = new Bitmap1D(WREGIONS.MAX, false);
        private readonly ArrayListResize<WArmy> armies = new ArrayListResize<WArmy>(120);
        private readonly Conflict conflict;
        private readonly Resolver init;
        private readonly Util util;
        private int playerRegAttackRegion = -1;
        private int playerRegAttackArmy = -1;

        public PRegAttack(Conflict conflict, Resolver init, Util util)
        {
            this.conflict = conflict;
            this.util = util;
            this.init = init;
        }

        public void save(FilePutter file)
        {
            active.save(file);
            map.save(file);
        }

        public void load(FileGetter file)
        {
            active.load(file);
            map.load(file);
        }

        public void clear()
        {
            active.clear();
            map.clear();
        }

        public void register(WArmy a)
        {
            if (a.region() != null && !map.get(a.region().index()))
            {
                map.set(a.region().index(), true);
                active.add(a.region().index());
            }
        }

        public void regAttack(Region reg, WArmy a)
        {
            playerRegAttackArmy = a.armyIndex();
            playerRegAttackRegion = reg.index();
        }

        public bool poll()
        {
            if (playerRegAttackArmy >= 0)
            {
                Region reg = WORLD.REGIONS().getByIndex(playerRegAttackRegion);
                WArmy a = WORLD.ENTITIES().armies.get(playerRegAttackArmy);
                playerRegAttackArmy = -1;
                if (reg.faction() == FACTIONS.player())
                {
                    if (create(reg, a))
                        return true;
                }
            }

            int death = 1000;

            while (!active.isEmpty())
            {
                int ai = active.get(active.size() - 1);
                Region reg = WORLD.REGIONS().all().get(ai);
                if (create(reg))
                    return true;
                else
                {
                    map.set(active.get(active.size() - 1), false);
                    active.remove(active.size() - 1);
                }
                if (death-- < 0)
                {
                    LOG.err("NOHA!");
                    break;
                }
            }
            return false;
        }

        private bool create(Region reg)
        {
            if (reg == null)
                return false;

            if (RD.MILITARY().garrison.get(reg) <= 0)
                return false;

            if (reg.faction() == FACTIONS.player())
                return false;

            if (reg.besieged())
            {
                WArmy a = util.getBesieger(reg);
                if (a != null)
                {
                    return create(reg, a);
                }
            }

            armies.clearSoft();
            armies.add(WORLD.ENTITIES().armies.fill(reg));

            foreach (WArmy a in armies)
            {
                if (a.region() == reg && create(reg, a))
                    return true;
            }

            return false;
        }

        private bool create(Region reg, WArmy a)
        {
            if (Util.valid(a) == null)
                return false;

            if (reg == null)
                return false;

            if (RD.MILITARY().garrison.get(reg) <= 0)
                return false;

            if (!Util.enemies(reg.faction(), a.faction()))
                return false;

            if (reg.besieged())
            {
                if (a.besieging() != reg)
                    return false;
            }

            double regPow = RD.MILITARY().power.getD(reg);
            double aPow = AD.power().get(a);

            Pair allies = util.fill(reg.faction(), a.faction(), a.ctx(), a.cty());
            bool p = reg.faction() == FACTIONS.player();
            foreach (WArmy a2 in allies.a)
            {
                if ((p || a2.faction() == FACTIONS.player()))
                    aPow += AD.power().get(a2);
            }

            foreach (WArmy a2 in allies.b)
            {
                if (a2 != a)
                    aPow += AD.power().get(a2);
            }

            if (!p && regPow < aPow)
                return false;

            conflict.clear();
            conflict.A.add(reg);
            foreach (WArmy a2 in allies.a)
            {
                conflict.A.add(a2);
            }
            conflict.B.add(a);
            foreach (WArmy a2 in allies.b)
            {
                if (a != a2)
                    conflict.B.add(a2);
            }

            init.init(conflict.A, conflict.B);
            return true;
        }
    }
}