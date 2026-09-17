using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.info;
using world.map.regions;
using world.region;
using world.region.pop;
using world.region.updating;

namespace game.faction.royalty
{
    public class King
    {
        public Str name = new Str(64);
        private readonly Str intro = new Str(64);
        private static readonly StrInserter<Faction> iins = new StrInserter<Faction>("TITLE")
        {
            protected override void Set(Faction t, Str str)
            {
                double d = t.Realm().Regions() / 20.0;
                int i = (int)(d * FACTIONS.Player().Level().All().Size());
                i = CLAMP.i(i, 0, FACTIONS.Player().Level().All().Size() - 1);
                str.Add(FACTIONS.Player().Level().All()[i].Male);
            }
        };
        private readonly NPCCourt court;

        public King(NPCCourt court)
        {
            this.court = court;
        }

        public void Init()
        {
            Royalty roy = court.All()[0];
            RDNames nn = RD.RACES().Get(roy.Induvidual.Race()).Names;
            name.Clear().Add(nn.RNames.Next());
            name.S();
            GFORMAT.ToNumeral(name, RND.rInt(1 + 15));
            intro.Clear().Add(nn.RIntro.Next());
        }

        public void Save(FilePutter file)
        {
            name.Save(file);
            intro.Save(file);
        }

        public void Load(NPCCourt c, FileGetter file)
        {
            name.Load(file);
            intro.Load(file);
        }

        public Royalty Roy()
        {
            return court.All()[0];
        }

        public Str Intro(Str str)
        {
            str.Add(intro);
            iins.Insert(court.Faction, str);
            return str;
        }

        public double Garrison()
        {
            return 0.5 + BOOSTABLES.NOBLE().AGRESSION.Get(Roy().Induvidual) * 0.5;
        }

        public double Size()
        {
            double c = 0.75 * BOOSTABLES.NOBLE().COMPETANCE.Get(Roy().Induvidual);
            return CLAMP.d(c, 0, 1);
        }

        public readonly RealmBuilder Builder = new RealmBuilder()
        {
            public override double Priority(Religion religion, Region reg)
            {
                if (STATS.RELIGION().Getter.Get(Roy().Induvidual).Religion == religion)
                    return 1.0;
                return BOOSTABLES.NOBLE().TOLERANCE.Get(Roy().Induvidual);
            }

            public override double Priority(TRADABLE res, Region reg)
            {
                return 1.0;
            }

            public override double Policy(Race race, Region reg)
            {
                double add = BOOSTABLES.NOBLE().TOLERANCE.Get(Roy().Induvidual) - 1;
                if (race == Roy().Induvidual.Race())
                    return 4 * RD.RACES().All.Size() - add * RD.RACES().All.Size();
                return -1 + Roy().Induvidual.Race().Pref().Race(race) + add;
            }

            public override double Military(Region reg)
            {
                double ran = RD.RAN().Get(reg, 9, 8) / (double)0x0FF;
                double v = 0.75 + BOOSTABLES.NOBLE().AGRESSION.Get(Roy().Induvidual) * 0.25;
                ran = 0.5 + ran * 0.5;
                v *= ran;
                return v;
            }

            public override double Size()
            {
                double c = 0.25 * 0.5 * BOOSTABLES.NOBLE().COMPETANCE.Get(Roy().Induvidual);
                return CLAMP.d(c, 0, 1);
            }
        };
    }
}