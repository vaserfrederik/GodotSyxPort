using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Main.Employment
{
    public class RoomEquip : INDEXED
    {
        private readonly List<RoomEmploymentSimple> rooms = new List<RoomEmploymentSimple>();
        public readonly List<Target> targets;
        private readonly int[] currents;
        private int total;
        private readonly int index;
        public readonly double degradePerDay;
        public readonly int defaultTarget;
        public readonly RESOURCE resource;
        public readonly BoostSpecs boosts;
        public readonly int maxAm;
        private static readonly string ¤¤equipment = "Equipment";
        private static readonly string ¤¤equipmentD = "Equipment boosts the efficiency of rooms. When equipped, each {0} degrades with a rate of {1} % per day.";

        private BoostSpec[] boostMap;

        public readonly INFO info;

        static RoomEquip()
        {
            D.ts(typeof(RoomEquip));
        }

        private readonly SAVABLE saver = new SAVABLE
        {
            Save = file =>
            {
                file.WriteInt(targets.Count);
                for (int i = 0; i < targets.Count; i++)
                {
                    file.WriteInt(targets[i].Get());
                }
            },

            Load = file =>
            {
                Clear();
                int am = file.ReadInt();

                if (am != targets.Count)
                {
                    for (int i = 0; i < am; i++)
                    {
                        file.ReadInt();
                    }
                    Clear();
                }
                else
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        targets[i].Set(file.ReadInt());
                    }
                }
            },

            Clear = () =>
            {
                total = 0;
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].Set(defaultTarget);
                    currents[i] = 0;
                }
            }
        };

        public RoomEquip(string kkey, LISTE<RoomEquip> all, RoomEmployments emps, Json data, ROOMS ROOMS, BoostableCat cat)
        {
            targets = new List<Target>(emps.ALLS().Count);
            while (targets.HasRoom())
                targets.Add(new Target());
            currents = Alloc.ii(emps.ALLS().Count);
            boostMap = new BoostSpec[emps.ALLS().Count];

            index = all.Add(this);
            resource = RESOURCES.map().Read(data);
            degradePerDay = data.d("WEAR_PER_DAY", 0, 1);
            defaultTarget = data.i("DEFAULT_TARGET");
            info = new INFO(¤¤equipment + ": " + resource.names, "" + Str.TMP.Clear().Add(¤¤equipmentD).Insert(0, resource.name).Insert(1, degradePerDay * 100, 2));
            boosts = new BoostSpecs(resource.names, resource.icon(), true);
            double add = data.d("BOOST_MAX_VALUE");

            int[] ams = Alloc.ii(emps.ALLS().Count);
            ROOMS.collection.new KJson("EQUIP_AMOUNTS", data)
            {
                protected override void Process(RoomBlueprint bp, Json j, string key, bool isWeak)
                {
                    if (bp is RoomBlueprintImp)
                    {
                        RoomBlueprintImp room = (RoomBlueprintImp)bp;
                        if (room.bonus() == null || room.employment() == null)
                        {
                            GAME.WarnLight(data.errorGet("Not a valid boostable room " + key, key));
                            return;
                        }
                        int am = j.i(key, 0, 100);
                        ams[room.employment().eindex()] = am;

//                      rooms.Add(room.employment());
//
//
//                      targets[room.employment().eindex()].init(kkey, am, room, resource, cat);
//
//                      targets[room.employment().eindex()].max = am;
//                      targets[room.employment().eindex()].set(defaultTarget);
                    }
                }
            };

            foreach (RoomEmploymentSimple e in emps.ALLS())
            {
                int am = ams[e.eindex()];
                if (am > 0)
                {
                    rooms.Add(e);

                    targets[e.eindex()].init(kkey, am, e.blueprint(), resource, cat);

                    targets[e.eindex()].max = am;
                    targets[e.eindex()].Set(defaultTarget);
                }
            }

            int m = 0;

            foreach (RoomEmploymentSimple e in rooms)
            {
                m = Math.Max(m, targets[e.eindex()].max);
            }

            maxAm = m;

            bool mul = data.bool("BOOST_MUL", false);
//          double cost = ROOMS.industries.vanillaRate(resource.tr());
//
//          cost *= maxAm*degradePerDay/add;
//          if (cost > 1) {
//              GAME.Notify(kkey + " is useless!");
//          }
//          cost = 1-cost;

            foreach (RoomEmploymentSimple e in rooms)
            {
                m = targets[e.eindex()].max;
                double to = Math.Ceiling(add * 100 * targets[e.eindex()].max) / (maxAm * 100.0);
                BoosterImp bo = new BoosterImp(new BSourceInfo(resource.names, resource.icon()), to, mul)
                {
                    vGet = (Faction f) => 0,
                    vGet = (Player f) => RoomEquip.this.Value(e),
                    vGet = (FactionNPC f) => 0,
                    vGet = (HCLASS_RACE popTime) => RoomEquip.this.Value(e),
                    vGet = (Induvidual indu) =>
                    {
                        RoomInstance ins = STATS.WORK().EMPLOYED.Get(indu);
                        if (ins == null || ins.blueprint() != e.blueprint())
                            return 0;

                        double t = ins.employees().toolsTargetMax(RoomEquip.this);
                        if (t == 0)
                            return 0;
                        double tt = ins.employees().tools(RoomEquip.this);
                        tt = CLAMP.d(tt, 0, ins.employees().toolsTarget(RoomEquip.this));
                        return CLAMP.d(tt / t, 0, 1);
                    }
                };

                BoostSpec s = boosts.push(bo, e.blueprint().bonus());
                boostMap[e.eindex()] = s;
            }
        }

        public Target target(RoomEmploymentSimple e)
        {
            return targets[e.eindex()];
        }

        public int targetI(RoomEmploymentSimple e)
        {
            return targets[e.eindex()].Get() * e.employed();
        }

        public int current(RoomEmploymentSimple e)
        {
            return currents[e.eindex()];
        }

        public double Value(RoomEmploymentSimple e)
        {
            double tt = targetI(e);
            if (tt == 0)
                return 0;
            double value = currents[e.eindex()] / tt;
            return Math.Max(value, 0);
        }

        public List<RoomEmploymentSimple> rooms()
        {
            return rooms;
        }

        public BoostSpec boost(RoomEmploymentSimple e)
        {
            return boostMap[e.eindex()];
        }

        public bool has(RoomEmploymentSimple e)
        {
            return targets[e.eindex()].max() > 0;
        }

        void count(RoomEmploymentSimple e, int am)
        {
            currents[e.eindex()] += am;
            total += am;
        }

        public int index()
        {
            return index;
        }

        public class Target : INTE
        {
            private Boostable maxLevel;
            int max;
            int i;
            public RoomBlueprintImp blue;
            static readonly ListGrower<Target> boos = new ListGrower<Target>();
            static Target()
            {
                new GameDisposable
                {
                    Dispose = () => boos.Clear()
                };
            }

            public Target()
            {
            }

            public void init(string kkey, int am, RoomBlueprintImp bp, RESOURCE resource, BoostableCat cat)
            {
                this.max = am;
                this.blue = bp;
                string key = "LEVEL_" + kkey + "_" + bp.key;
                string name = resource.names + "(" + bp.info.names + ")";
                SPRITE s = new SPRITE.Imp(Icon.L, Icon.S)
                {
                    render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
                    {
                        int dim = Y2 - Y1;
                        if (dim <= Icon.S)
                            bp.icon.render(r, X1, X2, Y1, Y2);
                        else
                        {
                            bp.icon.render(r, X1, X1 + dim, Y1, Y2);
                            resource.icon().render(r, X1 + dim, X1 + dim + dim, Y1, Y2);
                        }
                    }
                };

                maxLevel = BOOSTING.push(key, 0, name, name, s, cat);
                boos.add(this);
            }

            public int Get()
            {
                return CLAMP.i(i, 0, availableMax());
            }

            public int min()
            {
                return 0;
            }

            public int max()
            {
                return max;
            }

            public int availableMax()
            {
                return (int)(maxLevel == null ? 0 : maxLevel.get(FACTIONS.player()));
            }

            public void Set(int t)
            {
                i = t;
            }

            public Boostable boost()
            {
                return maxLevel;
            }
        }
    }
}