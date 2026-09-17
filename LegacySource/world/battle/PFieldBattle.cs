using System;
using System.Collections.Generic;
using System.IO;

namespace World.Battle
{
    public class PFieldBattle : SAVABLE
    {
        private readonly Util util;
        private readonly Conflict con;
        private readonly Resolver init;
        private readonly Bitmap1D map = new Bitmap1D(WArmyConstructor.MAX, false);
        private readonly ArrayListInt current = new ArrayListInt(WArmyConstructor.MAX);

        public PFieldBattle(Conflict con, Resolver init, Util util)
        {
            this.con = con;
            this.init = init;
            this.util = util;
        }

        public override void save(FilePutter file)
        {
            map.save(file);
            current.save(file);
        }

        public override void load(FileGetter file)
        {
            map.load(file);
            current.load(file);
        }

        public override void clear()
        {
            map.clear();
            current.clear();
        }

        public void add(WArmy a)
        {
            if (map.get(a.armyIndex()))
                return;

            map.set(a.armyIndex(), true);
            current.add(a.armyIndex());
        }

        public bool poll()
        {
            int death = 1000;

            while (!current.isEmpty())
            {
                int ai = current.get(current.size() - 1);
                WArmy a = WORLD.ENTITIES().armies.get(ai);
                if (create(a))
                    return true;
                else
                {
                    map.set(current.get(current.size() - 1), false);
                    current.remove(current.size() - 1);
                }
                if (death-- < 0)
                {
                    LOG.err("NOHA!");
                    break;
                }
            }
            return false;
        }

        private bool create(WArmy a)
        {
            if (!valid(a))
                return false;

            WArmy e = enemy(a);
            if (e == null)
                return false;
            Pair allies = util.fill(a.faction(), e.faction(), a.ctx(), a.cty());
            con.clear();

            con.clear();
            con.A.add(a);
            foreach (WArmy a2 in allies.a)
            {
                if (a != a2)
                    con.A.add(a2);
            }
            con.B.add(e);
            foreach (WArmy a2 in allies.b)
            {
                if (a != a2)
                    con.B.add(a2);
            }
            init.init(con.A, con.B);
            return true;
        }

        private bool valid(WArmy a)
        {
            return (a != null && AD.men(null).get(a) > 0);
        }

        private WArmy enemy(WArmy a)
        {
            if (WORLD.PATH().map.is.is(a.ctx(), a.cty()))
            {
                for (int di = 0; di < DIR.ALLC.size(); di++)
                {
                    DIR d = DIR.ALLC.get(di);
                    if (d == DIR.C || WORLD.PATH().map.can(a.ctx(), a.cty(), d))
                    {
                        int dx = a.ctx() + d.x();
                        int dy = a.cty() + d.y();

                        foreach (WArmy a2 in WORLD.ENTITIES().armies.fillTile(dx, dy))
                        {
                            if (a2.ctx() == dx && a2.cty() == dy)
                            {
                                if (valid(a2) && Util.enemies(a.faction(), a2.faction()))
                                {
                                    return a2;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void register(WArmy a)
        {
            if (map.get(a.armyIndex()))
                return;

            map.set(a.armyIndex(), true);
            current.add(a.armyIndex());
        }
    }
}