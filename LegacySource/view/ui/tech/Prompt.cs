using System;
using System.Linq;
using System.Collections.Generic;

namespace View.UI.Tech
{
    public static class Prompt
    {
        private static string ¤¤Unlock = "¤Do you wish to unlock the following:";
        private static string ¤¤Unlock2 = "¤For the cost of:";
        private static string ¤¤Forget = "¤Do you wish to forget the following technologies? 50% of the points deallocated will be frozen for some time.";

        private static double renderS = VIEW.RenderSecond() - 60;
        private static bool[] checks = new bool[TECHS.ALL().Count];
        private static double checkFoget = VIEW.RenderSecond();

        static Prompt()
        {
            D.ts(typeof(Prompt));
        }

        private static int[] costs = Alloc.II(TECHS.COSTS().Count);

        public static void Unlock(TECH tech)
        {
            if (FACTIONS.Player().Tech().Level(tech) == tech.LevelMax)
                return;
            if (!FACTIONS.Player().Tech().CanUnlockNext(tech) && !S.Get().Developer)
                return;

            foreach (TechCurrency c in TECHS.COSTS())
            {
                costs[c.Index] = FACTIONS.Player().Tech().CostOfRequired(c, tech);
            }

            foreach (TechCost c in tech.Costs)
            {
                costs[c.Cu.Index] += FACTIONS.Player().Tech.CostLevelNext(c.Amount, tech);
            }

            if (VIEW.RenderSecond() > renderS)
            {
                Str s = Str.TMP;
                s.Clear();
                s.Add(¤¤Unlock);
                s.NL();
                checks.Fill(false);
                AddUnlocks(tech, s);
                s.Add(¤¤Unlock2);
                s.NL();
                foreach (TechCurrency c2 in TECHS.COSTS())
                {
                    if (costs[c2.Index] > 0)
                        s.Add(c2.Bo.Name).S().Add(costs[c2.Index]);
                    s.NL();
                }

                tech = tech;
                VIEW.Inters().YesNo.Activate(s, AskUnlock, AskNo, true);
                renderS = VIEW.RenderSecond() + 120;
                return;
            }

            PUnlock(tech, FACTIONS.Player().Tech().Level(tech) + 1);
        }

        private static void AddUnlocks(TECH tech, Str s)
        {
            if (checks[tech.Index()])
                return;
            checks[tech.Index()] = true;
            s.Add(tech.Name());
            s.NL();
            for (int i = 0; i < tech.Requires().Count; i++)
            {
                TechRequirement r = tech.Requires()[i];
                if (FACTIONS.Player().Tech().Level(r.Tech) < r.Level)
                {
                    s.Add(r.Tech.Name());
                    if (r.Tech.LevelMax > 1)
                        s.S().Add(r.Level);
                    s.NL();
                }
            }
        }

        private static void PUnlock(TECH tech, int level)
        {
            for (int i = 0; i < tech.Requires().Count; i++)
            {
                TechRequirement r = tech.Requires()[i];
                if (FACTIONS.Player().Tech().Level(r.Tech) < r.Level)
                    PUnlock(r.Tech, r.Level);
            }
            FACTIONS.Player().Tech().LevelSet(tech, level);
        }

        public static void Forget(TECH tech)
        {
            int l = FACTIONS.Player().Tech().Level(tech);
            if (l == 0)
                return;

            int am = 1;
            Str s = Str.TMP;
            s.Clear();
            s.Add(¤¤Forget);
            s.NL();
            s.Add(tech.Name());
            s.NL();
            for (int ti = 0; ti < TECHS.ALL().Count; ti++)
            {
                TECH t = TECHS.ALL()[ti];
                if (t != tech && t.Requires(tech, l - 1) && FACTIONS.Player().Tech().Level(t) > 0)
                {
                    s.Add(t.Name());
                    s.NL();
                    am++;
                }
            }
            tech = tech;

            if (am > 2 || VIEW.RenderSecond() - checkFoget > 60)
            {
                VIEW.Inters().YesNo.Activate(s, AskForget, AskNo, true);
                checkFoget = VIEW.RenderSecond();
            }
            else
            {
                AskForget.Exe();
            }
        }

        private static TECH tech;

        private static ACTION askForget = new ACTION
        {
            Exe = () =>
            {
                int l = FACTIONS.Player().Tech().Level(tech);
                for (int ti = 0; ti < TECHS.ALL().Count; ti++)
                {
                    TECH t = TECHS.ALL()[ti];
                    if (t != tech && t.Requires(tech, l - 1))
                    {
                        FACTIONS.Player().Tech().LevelSet(t, 0);
                    }
                }
                FACTIONS.Player().Tech().LevelSet(tech, l - 1);
            }
        };

        private static ACTION askUnlock = new ACTION
        {
            Exe = () =>
            {
                PUnlock(tech, FACTIONS.Player().Tech().Level(tech) + 1);
            }
        };

        private static ACTION askNo = new ACTION
        {
            Exe = () =>
            {

            }
        };
    }
}