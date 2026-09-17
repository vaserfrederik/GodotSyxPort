using System;
using System.IO;

namespace Settlement.Stats.Disease
{
    public class Epidemic : StatsInit.StatUpdatable, SAVABLE
    {
        public DISEASE Current { get; private set; }
        public double Duration { get; private set; }

        public Epidemic(StatsInit init)
        {
            init.Updaters.Add(this);
            init.Savers.Put("EPIDEMIC_UPDATER", this);

            IDebugPanelSett.Add("disease: epidemic", new ACTION()
            {
                public void Exe()
                {
                    Outbreak(0.1 + RND.rFloat() * 0.5, DISEASES.RandomEpidemic(RND.rFloat()));
                }
            });

            IDebugPanelSett.Add("disease: CURE ALL", new ACTION()
            {
                public void Exe()
                {
                    new EntityIterator.Humans()
                    {
                        protected bool ProcessAndShouldBreakH(Humanoid h, int ie)
                        {
                            STATS.DISEASE().Cure(h.Indu(), false);
                            return false;
                        }
                    }.Iterate();
                    Current = null;
                    Duration = 0;
                }
            });
        }

        public void Update(double ds)
        {
            Duration -= ds;
            if (Duration < 0)
            {
                Current = null;
            }
        }

        public bool Outbreak(double spread, DISEASE strain)
        {
            int am = 0;
            double aveHealth = BOOSTABLES.PHYSICS().HEALTH.Get(HCLASS_RACE.ClP(null, null));
            Humanoid patientZero = null;

            ENTITY[] ee = SETT.ENTITIES().GetAllEnts();
            for (int i = 0; i < ee.Length; i++)
            {
                if (ee[i] != null && ee[i] is Humanoid)
                {
                    Humanoid a = (Humanoid)ee[i];
                    if (a.Indu().Player())
                    {
                        double c = spread * (BOOSTABLES.PHYSICS().HEALTH.Get(a.Indu()) / aveHealth);
                        if (RND.rFloat() < c)
                        {
                            STATS.DISEASE().Incubate(a.Indu(), strain);
                            am++;
                            if (RND.OneIn(am))
                                patientZero = a;
                        }
                    }
                }
            }

            if (am > 1)
            {
                Current = strain;
                Duration = TIME.SecondsPerDay() * (strain.IncubationDays + strain.Length);
                STATS.DISEASE().Infect(patientZero.Indu(), strain);
                return true;
            }
            return false;
        }

        public void Save(FilePutter file)
        {
            file.PutD(Duration);
            file.PutI(Current == null ? -1 : Current.Index());
        }

        public void Load(FileGetter file)
        {
            Duration = file.GetD();
            int ci = file.GetI();
            Current = ci < 0 ? null : DISEASES.All().GetC(ci);
        }

        public void Clear()
        {
            Duration = 0;
            Current = null;
        }
    }
}