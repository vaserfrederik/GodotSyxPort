using System;
using System.Collections.Generic;
using game;
using game.battle.util;
using game.boosting;
using init.paths;
using settlement.stats;
using settlement.stats.equip;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;

namespace game.battle.util
{
    public class ArmyFormations
    {
        public readonly ArrayListGrower<ArmyFormation> all = new ArrayListGrower<ArmyFormation>();
        public readonly ArmyFormation player;

        public ArmyFormations()
        {
            PATH p = PATHS.INIT().getFolder("battle").getFolder("formation");
            player = new ArmyFormation(new Json(p.gets("_player")));
            foreach (string f in p.getFiles())
            {
                all.add(new ArmyFormation(new Json(p.gets(f))));
            }
        }

        public class ArmyFormation
        {
            private readonly LIST<Pair> CENTRE;
            private readonly LIST<Pair> FLANK;
            private readonly LIST<Pair> FRONT;
            private readonly LIST<Pair> REAR;

            private readonly LIST<PairE> CENTRE_E;
            private readonly LIST<PairE> FLANK_E;
            private readonly LIST<PairE> FRONT_E;
            private readonly LIST<PairE> REAR_E;

            public ArmyFormation(Json json)
            {
                CENTRE = boost("PRIORITY_CENTRE", json);
                FLANK = boost("PRIORITY_FLANK", json);
                FRONT = boost("PRIORITY_FRONT", json);
                REAR = boost("PRIORITY_REAR", json);

                CENTRE_E = boostE("PRIORITY_CENTRE_E", json);
                FLANK_E = boostE("PRIORITY_FLANK_E", json);
                FRONT_E = boostE("PRIORITY_FRONT_E", json);
                REAR_E = boostE("PRIORITY_REAR_E", json);
            }

            private LIST<Pair> boost(string key, Json json)
            {
                ArrayListGrower<Pair> boosts = new ArrayListGrower<Pair>();
                BOOSTING.connecter(new ACTION()
                {
                    public void exe()
                    {
                        BOOSTING.MAP().new KJson(key, json)
                        {
                            protected void process(Boostable s, Json j, string key, bool isWeak)
                            {
                                foreach (Pair pp in boosts)
                                {
                                    if (pp.bo == s)
                                    {
                                        pp.value = j.d(key);
                                        return;
                                    }
                                }
                                Pair p = new Pair(s);
                                p.value = j.d(key);
                                boosts.add(p);
                            }
                        };
                    }
                });
                return boosts;
            }

            private LIST<PairE> boostE(string key, Json json)
            {
                ArrayListGrower<PairE> boosts = new ArrayListGrower<PairE>();
                BOOSTING.connecter(new ACTION()
                {
                    public void exe()
                    {
                        STATS.EQUIP().militaryColl.new KJson(key, json)
                        {
                            protected void process(EquipBattle s, Json j, string key, bool isWeak)
                            {
                                foreach (PairE pp in boosts)
                                {
                                    if (pp.bo == s)
                                    {
                                        pp.value = j.d(key);
                                        return;
                                    }
                                }
                                PairE p = new PairE(s);
                                p.value = j.d(key);
                                boosts.add(p);
                            }
                        };
                    }
                });
                return boosts;
            }

            public ArrayList<ArmyFormationDiv> getFirstRow(LIST<DivGeneration> all)
            {
                ArrayList<ArmyFormationDiv> li = new ArrayList<ArmyFormationDiv>(all.size());

                int di = 0;
                foreach (DivGeneration g in all)
                {
                    DIV_SPECImp s = g.makeSpec();
                    ArmyFormationDiv d = new ArmyFormationDiv(g, di);

                    foreach (Pair p in CENTRE)
                    {
                        d.centre += p.value * GAME.battle().boost(s, p.bo) / (1 + p.bo.baseValue);
                    }
                    foreach (Pair p in FLANK)
                    {
                        d.flank += p.value * GAME.battle().boost(s, p.bo) / (1 + p.bo.baseValue);
                    }
                    foreach (Pair p in FRONT)
                    {
                        d.front += p.value * GAME.battle().boost(s, p.bo) / (1 + p.bo.baseValue);
                    }
                    foreach (Pair p in REAR)
                    {
                        d.rear += p.value * GAME.battle().boost(s, p.bo) / (1.0 + p.bo.baseValue);
                    }

                    foreach (PairE p in CENTRE_E)
                    {
                        d.centre += p.value * s.equip(p.bo);
                    }
                    foreach (PairE p in FLANK_E)
                    {
                        d.flank += p.value * s.equip(p.bo);
                    }
                    foreach (PairE p in FRONT_E)
                    {
                        d.front += p.value * s.equip(p.bo);
                    }
                    foreach (PairE p in REAR_E)
                    {
                        d.rear += p.value * s.equip(p.bo);
                    }

                    li.add(d);
                    di++;
                }
                return li;
            }

            public ArmyFormationDiv get(LIST<ArmyFormationDiv> all, double centre, double flank, double front, double rear)
            {
                ArmyFormationDiv res = null;
                double value = double.MinValue;

                foreach (ArmyFormationDiv d in all)
                {
                    double v = d.value(centre, flank, front, rear);
                    if (res == null || v > value)
                    {
                        res = d;
                        value = v;
                    }
                }

                return res;
            }
        }

        private class Pair
        {
            public readonly Boostable bo;
            public double value;

            public Pair(Boostable bo)
            {
                this.bo = bo;
            }
        }

        private class PairE
        {
            public readonly EquipBattle bo;
            public double value;

            public PairE(EquipBattle bo)
            {
                this.bo = bo;
            }
        }

        public class ArmyFormationDiv
        {
            public double centre;
            public double flank;
            public double front;
            public double rear;

            public DivGeneration g;
            public readonly int divID;

            public ArmyFormationDiv(DivGeneration g, int divID)
            {
                this.g = g;
                this.divID = divID;
            }

            public double value(double centre, double flank, double front, double rear)
            {
                return this.centre * centre + this.flank * flank + this.front * front + this.rear * rear;
            }
        }
    }
}