using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Settlement.Room.Main.Employment
{
    public static class RoomEmployments
    {
        private static readonly string ¤¤allFarms = "Farm Employees";
        private static readonly string ¤¤allRefiner = "Refiner Employees";
        private static readonly string ¤¤allMine = "Mine Employees";
        private static readonly string ¤¤pasture = "Pasture Employees";
        private static readonly string ¤¤workshop = "Workshop Employees";
        private static readonly string ¤¤orchard = "Orchard Employees";
        private static readonly string ¤¤fishery = "Fishery Employees";

        static RoomEmployments()
        {
            D.ts(typeof(RoomEmployments));
        }

        private readonly List<RoomEmployment> all;
        private readonly List<RoomEmploymentSimple> allS;
        private int needed = 0;
        private int current = 0;
        private int target = 0;
        private int upI = 0;
        private readonly RaceGroup[] groups;
        private int[] searchIS;
        public readonly HistoryInt history = new HistoryInt(32, TIME.days(), true);

        public readonly Employer employer;
        public readonly RoomEquips equip;

        public RoomEmployments(ROOMS rooms)
        {
            all = new List<RoomEmployment>(RoomEmployment.WORK);
            allS = new List<RoomEmploymentSimple>(RoomEmploymentSimple.WORK_ALL);
            RoomEmployment.WORK.Clear();
            searchIS = Alloc.ii(all.Count);

            for (int i = 0; i < WGROUP.all().Count; i++)
            {
                groups[i] = new RaceGroup(WGROUP.all()[i], this);
            }

            equip = new RoomEquips(rooms, this);

            employer = new Employer(all);

            pushReq("FARM", ¤¤allFarms, UI.icons().l.farm, rooms.FARMS);
            pushReq("REFINER", ¤¤allRefiner, UI.icons().l.refiner, rooms.REFINERS);
            pushReq("MINE", ¤¤allMine, UI.icons().l.mine, rooms.MINES);
            pushReq("PASTURE", ¤¤pasture, UI.icons().l.pasture, rooms.PASTURES);
            pushReq("WORKSHOP", ¤¤workshop, UI.icons().l.workshop, rooms.WORKSHOPS);
            pushReq("ORCHARD", ¤¤orchard, rooms.ORCHARDS[0].iconBig(), rooms.ORCHARDS);
            pushReq("FISHERY", ¤¤fishery, UI.icons().l.fish, rooms.FISHERIES);
        }

        private static void pushReq(string key, string name, SPRITE icon, IList<RoomBlueprintIns> li)
        {
            GVALUES.FACTION.push("EMPLOYED_" + key, name, icon, new DOUBLE_O<Faction>()
            {
                public double getD(Faction t)
                {
                    int a = 0;
                    foreach (RoomBlueprintIns b in li)
                    {
                        a += b.employment().employed();
                    }
                    return a;
                }
            }, false);
        }

        void changeCurrent(int current, WGROUP g)
        {
            this.current += current;
            groups[g.index()].change(current);
        }

        void changeNeeded(RoomBlueprintIns b, int total)
        {
            this.needed += total;
        }

        void changeTarget(int current, WGROUP g)
        {
            this.target += current;
            groups[g.index()].target += current;
        }

        public void update(double ds)
        {
            groups[upI].update();
            upI++;
            if (upI == groups.Length)
            {
                upI = 0;
            }

            employer.update();
        }

        public void setTargets()
        {
            employer.updateAll();
        }

        public readonly SAVABLE saver = new SAVABLE()
        {
            public void save(FilePutter file)
            {
                history.save(file);
                equip.saver.save(file);

                file.i(allS.Count);
                foreach (RoomEmploymentSimple s in allS)
                {
                    file.chars(s.blueprint().key);
                    int pos = file.getPosition();
                    file.i(0);
                    s.save(file);
                    int le = file.getPosition() - pos - 4;
                    file.setAtPosition(pos, le);
                }
            }

            public void load(FileGetter file)
            {
                clear();

                history.load(file);
                equip.saver.load(file);

                int am = file.i();
                for (int i = 0; i < am; i++)
                {
                    string k = file.chars();

                    RoomBlueprint p = SETT.ROOMS().collection.tryGet(k);
                    int skip = file.i();
                    if (p == null || p.employment() == null)
                    {
                        file.setPosition(file.getPosition() + skip);
                    }
                    else
                    {
                        p.employment().load(file);
                    }
                }

                foreach (RoomEmploymentSimple ss in allS)
                {
                    if (ss is RoomEmployment s)
                    {
                        foreach (WGROUP g in WGROUP.all())
                        {
                            changeCurrent(s.employed(g), g);
                            changeTarget(s.target.group(g), g);
                        }
                    }
                    changeNeeded(ss.blueprint(), ss.neededWorkers());
                    RoomBlueprintIns blue = ss.blueprint();
                    if (blue.employment() != null)
                    {
                        for (int i = 0; i < blue.instancesSize(); i++)
                        {
                            RoomInstance ins = blue.getInstance(i);
                            blue.employment().loadadd(ins.employees());
                        }
                    }
                }
            }

            public void clear()
            {
                needed = 0;
                current = 0;
                target = 0;
                for (int i = 0; i < groups.Length; i++)
                {
                    groups[i].clear();
                }
                possibles.clear();
            }
        };

        private class RaceGroup : SAVABLE
        {
            private readonly IntegerStack possibles;
            private readonly RoomEmployments es;
            private WGROUP group;

            public RaceGroup(WGROUP group, RoomEmployments es)
            {
                this.es = es;
                possibles = new IntegerStack(es.all.Count);
                this.group = group;
            }

            void change(int current)
            {
                this.current += current;
            }

            bool setWork(Humanoid i, int[] searchI)
            {
                RoomInstance old = STATS.WORK().EMPLOYED.get(i);

                if (old != null && old.blueprintI().employment() is RoomEmployment)
                {
                    RoomEmployment ee = (RoomEmployment)old.blueprintI().employment();
                    if (ee.employed(group) > ee.target.group(group))
                    {
                        STATS.WORK().EMPLOYED.set(i, null);
                    }
                    else if (old.employees().isOverstaffed())
                    {
                        STATS.WORK().EMPLOYED.set(i, null);
                    }
                }

                if (STATS.WORK().EMPLOYED.get(i) != null)
                    return true;

                if (current >= target)
                {
                    return false;
                }

                while (!possibles.isEmpty())
                {
                    RoomEmployment e = es.all[possibles.pop()];
                    if (e.employed() < e.neededWorkers() && e.employed(group) < e.target.group(group))
                    {
                        possibles.push(e.index());
                        return setWork(i, e, searchI);
                    }
                }

                return false;
            }

            bool hasWork(Humanoid i)
            {
                return current < target;
            }

            private bool setWork(Humanoid i, RoomEmployment e, int[] searchI)
            {
                int am = e.blueprint().instancesSize();

                for (int k = 0; k < am; k++)
                {
                    if (searchI[e.index()] >= am)
                        searchI[e.index()] = 0;
                    RoomInstance ins = e.blueprint().getInstance(searchI[e.index()]);
                    if (ins.active() && ins.employees().employed() < ins.employees().target())
                    {
                        STATS.WORK().EMPLOYED.set(i, ins);
                        return true;
                    }
                    searchI[e.index()]++;
                }

                GAME.Notify("oh no!" + e.blueprint().info.name + " " + i.race().info.name + " " + e.target.group(group) + " " + e.employed(group) + " " + e.employed() + " " + e.neededWorkers());
                for (int ii = 0; ii < am; ii++)
                {
                    RoomInstance ins = e.blueprint().getInstance(ii);
                    Console.WriteLine(ins.employees().employed() + "  " + ins.employees().target());
                }

                return false;
            }

            void update()
            {
                possibles.clear();
                foreach (RoomEmployment p in SETT.ROOMS().employment.all)
                {
                    if (p.employed() < p.neededWorkers() && p.employed(group) < p.target.group(group))
                    {
                        possibles.push(p.index());
                    }
                }
            }

            public void save(FilePutter file)
            {
                file.i(current);
                file.i(target);
            }

            public void load(FileGetter file)
            {
                current = file.i();
                target = file.i();
                possibles.clear();
            }

            public void clear()
            {
                current = 0;
                target = 0;
                possibles.clear();
            }
        }
    }
}