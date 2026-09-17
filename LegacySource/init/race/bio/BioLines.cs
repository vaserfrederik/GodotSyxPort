using System;
using System.Collections.Generic;
using init.type;
using settlement.entity.animal;
using settlement.entity.humanoid;
using settlement.room.home;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.race.bio
{
    internal sealed class BioLines
    {
        internal readonly LinkedList<BioLine> descs = new LinkedList<BioLine>();
        internal readonly LinkedList<BioLine> houseP = new LinkedList<BioLine>();

        internal BioLines(BioLines org, Json json) : this(json)
        {
            for (int i = 0; i < descs.Count; i++)
            {
                BioLine l = descs[i];
                if (!json.Has(l.key))
                {
                    l.strings = org.descs[i].strings;
                }
            }
            if (json.Has("TRAIT"))
            {
                Json tj = json.Json("TRAIT");

                foreach (TRAIT t in TRAITS.ALL())
                {
                    if (!tj.Has(t.key()))
                    {
                        CharSequence[] dd1 = descs[4 + t.index()].strings;
                        CharSequence[] dd2 = org.descs[4 + t.index()].strings;

                        CharSequence[] dd = new CharSequence[dd1.Length + dd2.Length];
                        for (int i = 0; i < dd1.Length; i++)
                            dd[i] = dd1[i];
                        for (int i = 0; i < dd2.Length; i++)
                            dd[i + dd1.Length] = dd2[i];
                        descs[4 + t.index()].strings = dd;
                    }
                }
            }
        }

        internal BioLines(Json json)
        {
            new BioLine(descs, json, "INFO_OTHER")
            {
                protected override bool Use(Humanoid a)
                {
                    if (!a.indu().player())
                        return true;
                    return false;
                }
            };

            new BioLine(descs, json, "INFO_GENERAL");
            new BioLine(descs, json, "INFO_TITLE");
            new BioLine(descs, json, "INFO_GENERAL2");
            new BioLine(descs, json, "INFO_GENERAL3").nlSet();

            houseP.Add(new BioLine(descs, json, "HOME_NONE_WORK")
            {
                protected override bool Use(Humanoid a)
                {
                    if (!a.indu().player())
                        return false;
                    return STATS.HOME().GETTER.hasSearched.indu().isMax(a.indu()) && !STATS.HOME().GETTER.has(a) && STATS.WORK().EMPLOYED.get(a) != null;
                }
            }.nlSet());

            houseP.Add(new BioLine(descs, json, "HOME_NONE")
            {
                protected override bool Use(Humanoid a)
                {
                    if (!a.indu().player())
                        return false;
                    return STATS.HOME().GETTER.hasSearched.indu().isMax(a.indu()) && !STATS.HOME().GETTER.has(a) && STATS.WORK().EMPLOYED.get(a) == null;
                }
            }.nlSet());

            houseP.Add(new BioLine(descs, json, "HOME_NONE_SEARCH")
            {
                protected override bool Use(Humanoid a)
                {
                    if (!a.indu().player())
                        return false;
                    return !STATS.HOME().GETTER.hasSearched.indu().isMax(a.indu()) && !STATS.HOME().GETTER.has(a);
                }
            }.nlSet());

            {
                Json tj = json.Json("TRAIT");

                foreach (TRAIT t in TRAITS.ALL())
                {
                    new BioLine(descs, tj, t.key(), t.bios)
                    {
                        protected override bool Use(Humanoid a)
                        {
                            if (!a.indu().clas().player)
                                return false;
                            return STATS.TRAITS().stat(t).getD(a.indu()) > 0.35;
                        }
                    };
                }
            }

            new Friend(descs, json, "FRIEND")
            {
                protected override bool Use(Humanoid a)
                {
                    if (base.Use(a))
                    {
                        Humanoid b = (Humanoid)STATS.POP().FRIEND.get(a.indu());
                        return a.race().pref().race(b.indu().race()) >= 0.5;
                    }
                    return false;
                }
            }.nlSet();

            new Friend(descs, json, "FRIEND_ENEMY")
            {
                protected override bool Use(Humanoid a)
                {
                    if (base.Use(a))
                    {
                        Humanoid b = (Humanoid)STATS.POP().FRIEND.get(a.indu());
                        return a.race().pref().race(b.indu().race()) < 0.5;
                    }
                    return false;
                }
            }.nlSet();

            new BioLine(descs, json, "FRIEND_OTHER")
            {
                protected override bool Use(Humanoid a)
                {
                    if (!a.indu().player())
                        return false;
                    return STATS.POP().FRIEND.get(a.indu()) != null && (STATS.POP().FRIEND.get(a.indu()) is Animal);
                }
            }.nlSet();

            new Origin(descs, json, "ORIGIN_NATIVE", CAUSE_ARRIVES.BORN());
            new Origin(descs, json, "ORIGIN_IMMI", CAUSE_ARRIVES.IMMIGRATED());
            new Origin(descs, json, "ORIGIN_FREED", CAUSE_ARRIVES.EMANCIPATED());
            new Origin(descs, json, "ORIGIN_PAROLE", CAUSE_ARRIVES.PAROLE());
            new Origin(descs, json, "ORIGIN_SOLDIER", CAUSE_ARRIVES.SOLDIER_RETURN());
            new Origin(descs, json, "ORIGIN_INSANE", CAUSE_ARRIVES.CURED());

            houseP.Add(new BioLine(descs, json, "HOME")
            {
                protected override bool Use(Humanoid a)
                {
                    if (a.indu().clas() == HCLASSES.NOBLE())
                        return false;
                    HOME h = STATS.HOME().GETTER.get(a, this);
                    if (h == null)
                        return false;
                    return true;
                }
            }.nlSet());

            houseP.Add(new BioLine(descs, json, "HOME_NONE_WORK")
            {
                protected override bool Use(Humanoid a)
                {
                    if (!a.indu().player())
                        return false;
                    return STATS.HOME().GETTER.hasSearched.indu().isMax(a.indu()) && !STATS.HOME().GETTER.has(a) && STATS.WORK().EMPLOYED.get(a) != null;
                }
            }.nlSet());

            houseP.Add(new BioLine(descs, json, "HOME_NONE")
            {
                protected override bool Use(Humanoid a)
                {
                    if (!a.indu().player())
                        return false;
                    return STATS.HOME().GETTER.hasSearched.indu().isMax(a.indu()) && !STATS.HOME().GETTER.has(a) && STATS.WORK().EMPLOYED.get(a) == null;
                }
            }.nlSet());

            houseP.Add(new BioLine(descs, json, "HOME_NONE_SEARCH")
            {
                protected override bool Use(Humanoid a)
                {
                    if (!a.indu().player())
                        return false;
                    return !STATS.HOME().GETTER.hasSearched.indu().isMax(a.indu()) && !STATS.HOME().GETTER.has(a);
                }
            }.nlSet());

            new BioLine(descs, json, "DREAMS");

            new BioLine(descs, json, "DREAMS_CHILD")
            {
                protected override bool Use(Humanoid a)
                {
                    return a.indu().hType() == HTYPES.CHILD();
                }
            };

            new BioLine(descs, json, "DREAMS_CRIMINAL")
            {
                protected override bool Use(Humanoid a)
                {
                    return a.indu().hType() == HTYPES.PRISONER() && STATS.LAW().prisonerType.get(a.indu()).cl.player;
                }
            };

            new BioLine(descs, json, "DREAMS_ENEMY")
            {
                protected override bool Use(Humanoid a)
                {
                    return a.indu().hType() == HTYPES.ENEMY();
                }
            };

            new BioLine(descs, json, "DREAMS_RIOTEER")
            {
                protected override bool Use(Humanoid a)
                {
                    return a.indu().hType() == HTYPES.RIOTER();
                }
            };

            new BioLine(descs, json, "DREAMS_INSANE")
            {
                protected override bool Use(Humanoid a)
                {
                    return a.indu().hType() == HTYPES.DERANGED();
                }
            };

            new BioLine(descs, json, "DREAMS_SLAVE")
            {
                protected override bool Use(Humanoid a)
                {
                    return a.indu().hType() == HTYPES.SLAVE();
                }
            };
        }

        private static class Origin : BioLine
        {
            private readonly CAUSE_ARRIVE ca;

            internal Origin(LISTE<BioLine> all, Json json, string key, CAUSE_ARRIVE ca) : base(all, json, key)
            {
                this.ca = ca;
            }

            protected override bool Use(Humanoid a)
            {
                if (a.indu().clas() != HCLASSES.CITIZEN())
                    return false;
                if (STATS.POP().COUNT.arrive.get(a.indu()) != ca)
                    return false;
                return true;
            }
        }

        private static class Friend : BioLine
        {
            internal Friend(LISTE<BioLine> all, Json json, string key) : base(all, json, key) { }

            protected override bool Use(Humanoid a)
            {
                return base.Use(a) && STATS.POP().FRIEND.get(a.indu()) != null && STATS.POP().FRIEND.get(a.indu()) is Humanoid;
            }
        }
    }
}