using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Settlement.Room.Main
{
    public class BonusExperience : SAVABLE
    {
        private static readonly string ¤¤name = "¤Experience";
        private static readonly string ¤¤mGainedTitle = "¤Experience Gained";
        private static readonly string ¤¤mGainedBody = "¤We now employ over {0} in our {1}, and as a result, this combined experience is boosting performance. Boosting will continue to increase up until {2} employees.";
        private static readonly string ¤¤mLostTitle = "¤Experience Lost";
        private static readonly string ¤¤mLostBody = "¤Since the employees of our {0} have plummeted, performance boosts from experience has been lost.";

        static BonusExperience()
        {
            D.ts(typeof(BonusExperience));
        }

        private readonly int[] currents;
        private readonly byte[] sent;

        public BonusExperience()
        {
            currents = Alloc.ii(all.Count);
            sent = Alloc.bb(all.Count);
        }

        private readonly IUpdater up = new IUpdater(all.Count, TIME.secondsPerDay())
        {
            protected override void update(int index, double timeSinceLast)
            {
                RoomExperienceBonus bo = all[index];

                int am = bo.blue.employment().employed();

                if (currents[index] < bo.minEmployed && am >= bo.minEmployed && (sent[index] & 1) == 0)
                {
                    MessageText m = new MessageText(¤¤mGainedTitle);
                    Str s = Str.TMP;
                    s.clear();
                    s.add(¤¤mGainedBody);
                    s.insert(0, bo.minEmployed);
                    s.insert(1, bo.blue.info.names);
                    s.insert(2, bo.maxEmployed);
                    m.paragraph(s);
                    m.send();
                    sent[index] |= 1;
                }
                else if (currents[index] >= bo.minEmployed && am < bo.minEmployed && (sent[index] & 2) == 0)
                {
                    MessageText m = new MessageText(¤¤mLostTitle);

                    Str s = Str.TMP;
                    s.clear();
                    s.add(¤¤mLostBody);
                    s.insert(0, bo.blue.info.names);

                    m.paragraph(s);
                    m.send();
                    sent[index] |= 2;
                }
                currents[index] = am;
            }
        };

        public void update(double ds)
        {
            up.update(ds);
        }

        public override void save(FilePutter file)
        {
            file.isE(currents);
            file.bsE(sent);
            up.save(file);
        }

        public override void load(FileGetter file) throws IOException
        {
            file.isE(currents);
            file.bsE(sent);
            up.load(file);
        }

        public override void clear()
        {
            Array.Fill(currents, 0);
            Array.Fill(sent, (byte)0);
        }

        private static readonly ArrayListGrower<RoomExperienceBonus> all = new ArrayListGrower<>();
        static BonusExperience()
        {
            new GameDisposable()
            {
                protected override void dispose()
                {
                    all.clear();
                }
            };
        }

        public LIST<RoomExperienceBonus> ALL()
        {
            return all;
        }

        public class RoomExperienceBonus
        {
            public readonly double bonus;
            public readonly int maxEmployed;
            public readonly int minEmployed;
            private readonly double ie;
            public readonly RoomBlueprintImp blue;
            public readonly Boostable boostable;

            public RoomExperienceBonus(RoomBlueprintImp blue, Json data, Boostable boostable)
            {
                all.add(this);
                this.blue = blue;
                this.boostable = boostable;

                int ma = 1000;
                double bo = 1.0;

                if (data.has("EXPERIENCE_BONUS"))
                {
                    data = data.json("EXPERIENCE_BONUS");
                    ma = data.i("MAX_EMPLOYEES", 50, int.MaxValue);
                    bo = data.d("BONUS");
                }

                maxEmployed = ma;
                minEmployed = 50;
                bonus = bo;
                ie = 1.0 / (maxEmployed - minEmployed);
                BValue v = new BValue.BValueFaction(boostable)
                {
                    public override double vGet(Player f)
                    {
                        return CLAMP.d((blue.employment().employed() - minEmployed) * ie, 0, 1.0);
                    }

                    public override double vGet(FactionNPC f)
                    {
                        return 0;
                    }
                };

                BoosterValue bos = new BoosterValue(v, new BSourceInfo(¤¤name, UI.icons().s.clock), 0, bonus, false);

                bos.add(boostable);
            }
        }
    }
}