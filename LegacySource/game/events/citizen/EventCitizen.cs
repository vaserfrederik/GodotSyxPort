using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Game.Events.Citizen
{
    public sealed class EventCitizen : EventResource
    {
        public static readonly double BreakPoint = 0.85;
        private static readonly string ¤¤riotWarning = "¤Ungrateful plebs!";
        private static readonly string ¤¤riotWarningD = "¤Rumour has it that your citizens are grinding their teeth in frustration over what they claim is your incompetent rule. If nothing is done in time, a riot might follow! Try to increase their loyalty immediately.";

        private static readonly string ¤¤emigration = "¤Low loyalty!";
        private static readonly string ¤¤emigrationD = "¤Some plebeians are displeased with your rule. As a result, they are packing their bags to leave. We must try to improve the loyalty of our plebeians, else we risk ever greater displays of dissatisfaction.";

        static EventCitizen()
        {
            D.ts(typeof(EventCitizen));
        }

        private bool hasSentWarning = false;
        private readonly double timerD = 15;
        private double timer = 15;
        private double count = 5.0;
        private int warmup = 3;
        private readonly double countD = timerD / TIME.SecondsPerDay;
        private bool emigrate;

        private readonly double emmiCountD = 100.0 / (TIME.SecondsPerDay * TIME.Years.BitConversion(TIME.Days));
        private readonly double[] emmiCount;

        private readonly EventCitizenEmmigrate emmi = new EventCitizenEmmigrate();
        private readonly EventCitizenStrike strike = new EventCitizenStrike();
        private readonly EventCitizenRiot riot = new EventCitizenRiot();
        private readonly EventCitizenRace brawl = new EventCitizenRace();
        private readonly EventCitizenRel rel = new EventCitizenRel();

        private readonly SMALL_EVENT[] all = new SMALL_EVENT[]
        {
            emmi, strike, brawl, rel
        };

        private readonly SMALL_EVENT[] tmp = new SMALL_EVENT[all.Length];

        private readonly int[] amounts;

        public EventCitizen() : base("CITIZEN")
        {
            Clear();

            IDebugPanelSett.Add("Event: Emmigration2", new ACTION
            {
                Exe = () =>
                {
                    double total = 0;
                    foreach (var r in RACES.All())
                    {
                        int a = GetAmount(r);
                        total += a;
                        amounts[r.Index] = a;
                    }

                    double c = total / STATS.POP.POP.Data(null);

                    if (c == 0)
                    {
                        LOG.Ln("nay!");
                    }
                    else
                    {
                        var r = GetRace(total);
                        emmi.Event(amounts[r.Index], r);
                    }
                }
            });
        }

        protected override void Save(FilePutter file)
        {
            file.D(timer);
            file.D(count);
            file.Bool(emigrate);
            file.Bool(hasSentWarning);
            file.I(warmup);
            foreach (var e in all)
            {
                e.Save(file);
            }
            riot.Save(file);
            RACES.Map.Saver.Save(emmiCount, file);
        }

        protected override void Load(FileGetter file)
        {
            timer = file.D();
            count = file.D();
            emigrate = file.Bool();
            hasSentWarning = file.Bool();
            warmup = file.I();
            foreach (var e in all)
            {
                e.Load(file);
            }
            riot.Load(file);
            if (!VERSION.VersionIsBefore(71, 10))
                RACES.Map.Loader.Load(emmiCount, file, 0);
        }

        protected override void Clear()
        {
            emigrate = true;
            hasSentWarning = false;
            timer = timerD;
            count = 2.0;
            foreach (var e in all)
            {
                e.Clear();
            }
            warmup = 3;
            emmiCount.Fill(0);
            riot.Clear();
            ShuffleSmall();
        }

        private void ShuffleSmall()
        {
            Array.Copy(all, tmp, all.Length);

            for (int i = 0; i < tmp.Length; i++)
            {
                int ri = RND.RInt(tmp.Length);
                var o = tmp[i];
                tmp[i] = tmp[ri];
                tmp[ri] = o;
            }
        }

        public bool ShouldEmigrate(Humanoid h)
        {
            return ShouldEmigrate(h.Race);
        }

        public bool ShouldEmigrate(Race r)
        {
            return emmi.ShouldEmigrate(r);
        }

        public void Emigrate(Humanoid h)
        {
            emmi.Emigrate(h);
        }

        public bool OnStrike(Humanoid h)
        {
            return strike.IsStriking(h);
        }

        public bool ShouldBrawl(Humanoid a, Humanoid b)
        {
            if (a.Indu.Clas == HCLASSES.CITIZEN && b.Indu.Clas == HCLASSES.CITIZEN)
                return brawl.IsAtOdds(a, b) || rel.IsAtOdds(a, b);
            return false;
        }

        protected override void Update(double ds)
        {
            foreach (var e in all)
            {
                e.Update(ds);
            }
            riot.Update(ds);

            timer -= ds;
            if (timer > 0)
                return;

            timer += timerD;

            if (STATS.POP.POP.Data(null) < 15)
                return;

            double total = 0;
            double biggest = 0;
            foreach (var r in RACES.All())
            {
                int a = GetAmount(r);
                if (a > 0 && !SETT.ENTRY.IsClosed)
                {
                    emmiCount[r.Index] += timerD * emmiCountD;
                    int ee = (int)emmiCount[r.Index];

                    if (ee > 0)
                    {
                        emmi.Inc(ee, r);
                        emmiCount[r.Index] -= ee;

                        if (!hasSentWarning)
                        {
                            new MessageText(¤¤emigration).Paragraph(¤¤emigrationD).Send();
                            hasSentWarning = true;
                        }
                        a -= ee;
                        if (a < 0)
                            a = 0;
                    }
                }
                else
                {
                    emmiCount[r.Index] = 0;
                }

                total += a;
                amounts[r.Index] = a;
                int pop = STATS.POP.POP.Data(HCLASSES.CITIZEN).Get(r);
                if (pop > 0)
                    biggest = Math.Max(biggest, (double)a / pop);
            }

            if (total == 0 || biggest <= 0)
            {
                if (count < 1.0)
                {
                    count += countD;
                    count = Math.Clamp(count, 0, 1.0);
                }
            }
            else
            {
                double c = biggest;

                double old = count;
                biggest = Math.Pow(c, 0.8);
                count -= c * countD;
                if (count >= 0.25)
                    return;

                if (emigrate)
                {
                    if (old > 0.25)
                    {
                        var r = GetRace(total);
                        ShuffleSmall();
                        foreach (var e in tmp)
                        {
                            if (e.Event(amounts[r.Index], r))
                            {
                                AddCount(0.2 + 0.5 * total / (1 + STATS.POP.POP.Data(HCLASSES.CITIZEN).Get(null)));
                                STANDINGS.Emergency(HCLASSES.CITIZEN, 2 * TIME.SecondsPerDay);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void AddCount(double am)
        {
            warmup--;
            if (warmup < 1)
                warmup = 1;
            count += warmup * am;
        }

        private Race GetRace(double total)
        {
            total = total * RND.RFloat();
            for (int i = 0; i < RACES.All().Count; i++)
            {
                total -= amounts[i];
                if (total <= 0 && amounts[i] > 0)
                    return RACES.All()[i];
            }
            for (int i = 0; i < RACES.All().Count; i++)
            {
                if (amounts[i] > 0)
                    return RACES.All()[i];
            }
            return RACES.All()[0];
        }

        private int GetAmount(Race r)
        {
            double m = Math.Max(STANDINGS.CITIZEN.Loyalty.Get(r), STANDINGS.CITIZEN.LoyaltyTarget.Get(r));
            if (m >= BreakPoint)
            {
                return 0;
            }

            m = 1.0 - m / BreakPoint;

            double dPop = STATS.POP.POP.Data(null);
            dPop = 0.1 + 0.9 * Math.Clamp(dPop / 600.0, 0, 1);

            m *= dPop;
            int pop = STATS.POP.POP.Data(HCLASSES.CITIZEN).Get(r);

            int rebels = (int)(pop * m);

            return Math.Clamp(rebels, 0, pop);
        }

        public interface SMALL_EVENT : SAVABLE
        {
            bool Event(int am, Race race);
            void Update(double ds);
        }
    }
}