using System;
using System.IO;
using System.Collections.Generic;
using game;
using game.boosting;
using game.debug;
using game.faction;
using init.race;
using init.sprite.UI;
using init.type;
using init.value;
using settlement.main;
using snake2d;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sprite;
using util.data;

namespace settlement.stats.standing
{
    public class STANDINGS : SettResource
    {
        private static STANDINGS s;

        public static void Create()
        {
            s = new STANDINGS();
        }

        private readonly StandingCitizen happiness = new StandingCitizen(HCLASSES.CITIZEN(), BOOSTABLES.BEHAVIOUR().HAPPI, BOOSTABLES.BEHAVIOUR().LOYALTY);
        private readonly StandingCitizen submission = new StandingCitizen(HCLASSES.SLAVE(), BOOSTABLES.BEHAVIOUR().HAPPI_SLAVES, BOOSTABLES.BEHAVIOUR().SUBMISSION);
        private readonly StandingBuff buff = new StandingBuff();

        private STANDINGS() : base("STANDINGS", false)
        {
            if (false)
            {
                //buff for war
            }
            foreach (Race r in RACES.All())
            {
                GVALUES.FACTION.Push("LOYALTY_" + r.Key, happiness.Loyalty.Info().Name + ": " + r.Info.Names, new SPRITE.Imp(Icon.M)
                {
                    public override void Render(SPRITE_RENDERER re, int X1, int X2, int Y1, int Y2)
                    {
                        r.Appearance().Icon.Render(re, X1, X2, Y1, Y2);
                    }
                }, new DOUBLE_O<Faction>()
                {
                    public override double GetD(Faction t)
                    {
                        return happiness.Loyalty.GetD(r);
                    }
                });
            }

            GVALUES.FACTION.Push("LOYALTY", happiness.Loyalty.Info().Name, UI.Icons().S.Heart, new DOUBLE_O<Faction>()
            {
                public override double GetD(Faction t)
                {
                    return happiness.Current();
                }
            });

            GVALUES.FACTION.Push("SUBMISSION_SLAVES", submission.Info().Name, UI.Icons().S.Slave, new DOUBLE_O<Faction>()
            {
                public override double GetD(Faction t)
                {
                    return submission.Current();
                }
            });

            GAME.AddBeforeGameStarts(new ACTION()
            {
                public override void Exe()
                {
                    s.Happiness.Init();
                    s.Submission.Init();
                }
            });
        }

        public static StandingCitizen Get(HCLASS c)
        {
            if (c == HCLASSES.CITIZEN())
                return s.Happiness;
            else if (c == HCLASSES.SLAVE())
                return s.Submission;
            return s.Happiness;
        }

        public static StandingCitizen CITIZEN()
        {
            return s.Happiness;
        }

        public static StandingCitizen SLAVE()
        {
            return s.Submission;
        }

        public static void Emergency(HCLASS cl, double time)
        {
            s.buff.Execute(cl, time);
        }

        protected override void Save(FilePutter file)
        {
            happiness.Save(file);
            submission.Save(file);
            buff.Saver.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            happiness.Load(file);
            submission.Load(file);
            if (!VERSION.VersionIsBefore(71, 20))
                buff.Saver.Load(file);
        }

        protected override void Clear()
        {
            happiness.Clear();
            submission.Clear();
            buff.Saver.Clear();
        }

        protected override void Update(double ds, Profiler profiler)
        {
            happiness.Update(ds);
            submission.Update(ds);
            buff.Update(ds);
        }
    }
}