using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Events.Citizen
{
    public class EventCitizenRiot : ISavable
    {
        private static readonly string ¤¤riot = "¤Riot!";
        private static readonly string ¤¤riotD = "¤Your subjects have had enough of you and your rule. They have risen up, determined to show their displeasure by murdering and vandalizing. The only way to quell these rebels is by calling in the military, set them to mopping up. Guards will also do their fair share. Riots will also subdue naturally without action in time.";
        private static readonly string ¤¤amount = "¤We have reports of {0} rioters. The following species have joined: ";
        private static readonly string ¤¤success = "¤The rioters have had a taste of your might and have laid down their arms. The ring leaders will be processed in your justice system and be made an example of.";
        private static readonly string ¤¤Over = "¤Riot Over!";
        private static readonly string ¤¤OverD = "¤The rioters have had enough of murdering and pillaging for now. They have returned to be law abiding citizens, until they feel it's time again.";

        private double timer = 0;

        private double secondsToRiot;
        private int currentRioteers = 0;

        static EventCitizenRiot()
        {
            D.ts(typeof(EventCitizenRiot));
        }

        public EventCitizenRiot()
        {
            IDebugPanelSett.Add("Event: Riot", new Action
            {
                Exe = () =>
                {
                    int[] races = new int[RACES.all().Count];
                    for (int i = 0; i < races.Length; i++)
                    {
                        races[i] = (int)Math.Ceiling(STATS.POP().POP.data(HCLASSES.CITIZEN()).get(RACES.all()[i]) * RND.rExpo());
                    }
                    riot(races);
                }
            });

            clear();
        }

        public void save(BinaryWriter file)
        {
            file.Write(timer);
            file.Write(secondsToRiot);
            file.Write(currentRioteers);
        }

        public void load(BinaryReader file)
        {
            timer = file.ReadDouble();
            secondsToRiot = file.ReadDouble();
            currentRioteers = file.ReadInt32();
        }

        public void clear()
        {
            timer = 0;
            secondsToRiot = 0;
            currentRioteers = 0;
        }

        public void update(double ds)
        {
            timer += ds;
            if (timer < 10)
                return;
            timer -= 10;

            if (STATS.POP().pop(HTYPES.RIOTER()) > 0)
            {
                if ((double)STATS.POP().pop(HTYPES.RIOTER()) / currentRioteers < 0.3)
                {
                    foreach (var e in SETT.ENTITIES().getAllEnts())
                    {
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            if (a.indu().hType() == HTYPES.RIOTER())
                            {
                                a.HTypeSet(RND.oneIn(5) ? HTYPES.PRISONER() : HTYPES.SUBJECT(), CAUSE_LEAVES.PUNISHED(), null);
                            }
                        }
                    }
                    new MessageText(¤¤Over, ¤¤success).send();
                }

                secondsToRiot -= 10;
                if (secondsToRiot <= 0)
                {
                    foreach (var e in SETT.ENTITIES().getAllEnts())
                    {
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            if (a.indu().hType() == HTYPES.RIOTER())
                            {
                                a.HTypeSet(HTYPES.SUBJECT(), null, null);
                            }
                        }
                    }
                    new MessageText(¤¤Over, ¤¤OverD).send();
                }
            }
        }

        public void riot(int[] races)
        {
            currentRioteers = 0;

            secondsToRiot = TIME.secondsPerDay() * 0.25 + RND.rFloat() * TIME.secondsPerDay() * 0.75;
            int[] tot = new int[RACES.all().Count];

            for (int i = 0; i < races.Length; i++)
            {
                tot[i] = races[i];
            }

            Humanoid first = null;

            foreach (var e in SETT.ENTITIES().getAllEnts())
            {
                if (e is Humanoid)
                {
                    Humanoid a = (Humanoid)e;
                    if (a.indu().hType() == HTYPES.SUBJECT() && races[a.race().index] > 0)
                    {
                        first = a;
                        break;
                    }
                }
            }

            if (first == null)
                return;

            GUTIL.flooder().init(this);
            GUTIL.flooder().pushSloppy(first.tc(), 0);
            while (GUTIL.flooder().hasMore())
            {
                PathTile c = GUTIL.flooder().pollSmallest();
                foreach (var e in SETT.ENTITIES().getAtTile(c.x(), c.y()))
                {
                    if (e is Humanoid)
                    {
                        Humanoid a = (Humanoid)e;
                        if (a.indu().hType() == HTYPES.SUBJECT() && races[a.race().index] > 0)
                        {
                            races[a.race().index]--;
                            currentRioteers++;
                            a.HTypeSet(HTYPES.RIOTER(), null, null);
                        }
                    }
                }
                foreach (DIR d in DIR.ALL)
                {
                    if (!SETT.PATH().solidity.is(c, d) && SETT.IN_BOUNDS(c, d))
                        GUTIL.flooder().pushSmaller(c, d, c.getValue() + d.tileDistance());
                }
            }
            GUTIL.flooder().done();

            if (currentRioteers > 0)
            {
                STANDINGS.emergency(HCLASSES.CITIZEN(), TIME.secondsPerDay() * 4);
                Str t = Str.TMP;
                t.clear();

                t.add(¤¤amount).insert(0, currentRioteers);
                t.NL();
                foreach (Race r in RACES.all())
                {
                    if (tot[r.index] - races[r.index] > 0)
                    {
                        t.NL();
                        t.add(r.info.names);
                    }
                }

                new MessageText(¤¤riot, ¤¤riotD).paragraph(t).send();

                GAME.count().RIOTS.inc(1);
            }
        }
    }
}