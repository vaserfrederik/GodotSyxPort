using System;
using System.Collections.Generic;

namespace Settlement.Battle
{
    using Game;
    using VERSION;
    using Game.Battle.Div;
    using Init.Constant;
    using Init.Race;
    using Init.Type;
    using Settlement.Entity.Humanoid;
    using Settlement.Main;
    using Settlement.Room.Military.Training;
    using Settlement.Stats;
    using Settlement.Stats.Colls.StatsBattle;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;
    using World.Army;

    public sealed class ArmyTrainingInfo
    {
        private bool dirty = false;
        private readonly int[] targets = new int[RACES.All().Count];

        private readonly RaceDiv[] perRace = new RaceDiv[RACES.All().Count];
        private int raceU = 0;
        private bool sendOutWithoutTraining = true;

        public ArmyTrainingInfo()
        {
            for (int i = 0; i < perRace.Length; i++)
                perRace[i] = new RaceDiv(RACES.All()[i]);
        }

        public ROOM_M_TRAINER<?> UpdateAndGetEmployment(Humanoid a, ROOM_M_TRAINER<?> current)
        {
            Div div = UpdateExisting(a);

            if (div == null)
                return null;

            DivInfo inInfo = div.info;

            if (current != null && current.Employable() >= 0 && current.Training().ShouldTrain(a.Indu(), inInfo.Training(current.Training()), true))
            {
                return current;
            }

            if (false)
            {
                //have some kind of employment swapper. Matching best man for best div
                //also age limit
            }

            current = EmploymentTarget(a, div, current != null);
            if (current != null)
                return current;

            STATS.BATTLE().RECRUIT.Set(a, null);
            return null;
        }

        private ROOM_M_TRAINER<?> EmploymentTarget(Humanoid a, Div div, bool training)
        {
            double bestV = 0;
            ROOM_M_TRAINER<?> best = null;
            DivInfo inInfo = div.info;
            foreach (StatTraining tra in STATS.BATTLE().TRAINING_ALL)
            {
                double emp = tra.Room.Employable();

                if (emp >= 1)
                {
                    if (tra.ShouldTrain(a.Indu(), inInfo.Training(tra), training))
                    {
                        return tra.Room;
                    }
                    else if (emp > bestV)
                    {
                        bestV = emp;
                        best = tra.Room;
                    }
                }
            }

            if (!STATS.BATTLE().BasicTraining.IsMax(a.Indu()))
                return best;
            return null;
        }

        private Div UpdateExisting(Humanoid a)
        {
            Div div = STATS.BATTLE().DIV.Get(a);

            if (div != null)
            {
                if (!CanStayInDiv(a, div, false))
                {
                    STATS.BATTLE().DIV.Set(a, null);
                    return SetNew(a);
                }

                if (perRace[a.Race().Index].TryBetter(a.Indu(), div))
                {
                    Div match = perRace[a.Race().Index].GetMatch(a.Indu());
                    if (match != null && match != div)
                    {
                        STATS.BATTLE().DIV.Set(a, match);
                        div = match;
                    }
                }
                return div;
            }

            div = STATS.BATTLE().RECRUIT.Get(a);
            if (div != null)
            {
                if (!CanStayInDiv(a, div, true))
                {
                    STATS.BATTLE().RECRUIT.Set(a, null);
                    return SetNew(a);
                }

                if (STATS.BATTLE().BasicTraining.IsMax(a.Indu()))
                {
                    STATS.BATTLE().RECRUIT.Set(a, null);
                    STATS.BATTLE().DIV.Set(a, div);
                    return div;
                }
                return div;
            }

            return SetNew(a);
        }

        public bool ShouldJoinArmy(Humanoid a)
        {
            Div div = STATS.BATTLE().DIV.Get(a);
            if (div == null)
                return false;
            if (AD.CityDivs().AttachedArmy(STATS.BATTLE().DIV.Get(a)) == null)
                return false;
            if (!SETT.ENTRY().Points.HasAny())
                return false;
            if (sendOutWithoutTraining)
                return true;

            DivInfo inInfo = div.info;
            bool training = a.Indu().HType() == HTYPES.RECRUIT();
            foreach (StatTraining tra in STATS.BATTLE().TRAINING_ALL)
            {
                if (tra.ShouldTrain(a.Indu(), inInfo.Training(tra), training))
                {
                    return false;
                }
            }

            return EmploymentTarget(a, div, false) == null;
        }

        public bool SendOutWithoutTraining()
        {
            return sendOutWithoutTraining;
        }

        public void SendOutWithoutTraining(bool s)
        {
            sendOutWithoutTraining = s;
        }

        public void ClearTargets()
        {
            dirty = true;
        }

        private void Cache()
        {
            if (dirty)
            {
                Array.Fill(targets, 0);
                for (int di = 0; di < GAME.ARMIES().Player().Divisions().Size; di++)
                {
                    Div d = GAME.ARMIES().Player().Divisions()[di];
                    if (d.info.Race() != null)
                    {
                        targets[d.info.Race().Index()] += d.info.Men();
                    }
                }
                dirty = false;
            }
        }

        public int TargetMen(Race race)
        {
            Cache();
            if (race == null)
            {
                int am = 0;
                foreach (int i in targets)
                    am += i;
                return am;
            }

            return targets[race.Index];
        }

        public int TargetMen()
        {
            return TargetMen(null);
        }

        private bool ThereAreDivsToSignUpTo(Race race)
        {
            int am = (int)AD.CityDivs().Total(race) + (STATS.BATTLE().DIV.Stat().Data(null).Get(race, 0) + STATS.BATTLE().RECRUIT.Stat().Data(null).Get(race, 0));
            return (am < TargetMen(race));
        }

        private Div SetNew(Humanoid a)
        {
            if (!ThereAreDivsToSignUpTo(a.Race()))
            {
                return null;
            }

            if (perRace[a.Race().Index].Has(a.Indu()))
            {
                Div match = perRace[a.Race().Index].GetMatch(a.Indu());
                if (match != null)
                {
                    if (STATS.BATTLE().BasicTraining.IsMax(a.Indu()))
                    {
                        STATS.BATTLE().DIV.Set(a, match);
                        return match;
                    }
                    else
                    {
                        STATS.BATTLE().RECRUIT.Set(a, match);
                        return match;
                    }
                }
            }
            return null;
        }

        private bool CanStayInDiv(Humanoid a, Div div, bool recruit)
        {
            DivInfo inInfo = div.info;
            if (a.Race() != inInfo.Race())
                return false;
            if (recruit && AD.CityDivs().Get(div).Men() + STATS.BATTLE().DIV.Stat().Div().Get(div) + STATS.BATTLE().RECRUIT.InDiv(div) > inInfo.Men())
                return false;
            if (!recruit && !STATS.BATTLE().BasicTraining.IsMax(a.Indu()))
                return false;
            return true;
        }

        public void Update()
        {
            if (raceU == Config.Battle().DIVISIONS_PER_ARMY)
            {
                foreach (RaceDiv d in perRace)
                    d.Update();
                raceU = 0;
                return;
            }

            Div d = GAME.ARMIES().Player().Divisions()[raceU];
            DivInfo inInfo = d.info;
            perRace[inInfo.Race().Index].Update(d, inInfo);

            raceU++;
        }

        private class RaceDiv
        {
            private readonly Race race;
            private readonly ArrayList<Div> divs = new ArrayList<Div>(16);

            public RaceDiv(Race race)
            {
                this.race = race;
            }

            public Div GetMatch(Induvidual inInfo)
            {
                return GetNext();
            }

            private Div GetNext()
            {
                while (!divs.IsEmpty())
                {
                    Div div = divs[divs.Count - 1];
                    if (div.info.Race() == race)
                    {
                        int am = div.info.Men() - (AD.CityDivs().Get(div).Men() + STATS.BATTLE().DIV.Stat().Div().Get(div) + STATS.BATTLE().RECRUIT.InDiv(div));
                        if (am > 0)
                            return div;
                    }
                    divs.RemoveLast();
                }
                return null;
            }

            public bool Has(Induvidual i)
            {
                return GetNext() != null;
            }

            public bool TryBetter(Induvidual i, Div div)
            {
                return false;
            }

            public void Update()
            {
                divs.ClearSloppy();
            }

            public void Update(Div div, DivInfo inInfo)
            {
                if (!divs.HasRoom())
                    return;
                int am = inInfo.Men() - (AD.CityDivs().Get(div).Men() + STATS.BATTLE().DIV.Stat().Div().Get(div) + STATS.BATTLE().RECRUIT.InDiv(div));
                if (am > 0)
                {
                    divs.Add(div);
                }
            }
        }
    }
}