using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;

namespace game.events.killer
{
    public sealed class EventKiller : EVENTS.EventResource
    {
        private readonly Messenger messages = new Messenger();
        private readonly Messenger.Data mData = new Messenger.Data();
        private static readonly double interval = TIME.secondsPerDay() * 16 * 24;
        private readonly LIST<KillerType> killerTypes;
        private int[] typeShuffle;
        private DoubleImp timer = new DoubleImp();
        private int type = 0;
        private int day;
        private bool dormant;
        private int killerID = -1;
        private int suspect = -1;
        private int victimRace = -1;
        private int victims;
        private double rate;
        private static double rateSpeed = 1.0 / (TIME.secondsPerDay() * 8);

        public EventKiller() : base("KILLER")
        {
            LinkedList<KillerType> ks = new LinkedList<KillerType>();
            PATH pp = PATHS.TEXT_MISC().getFolder("serialKiller");
            foreach (string k in pp.getFiles())
            {
                ks.add(new KillerType(JsonDocument.Parse(pp.gets(k)).RootElement));
            }
            this.killerTypes = new ArrayList<KillerType>(ks);
            this.typeShuffle = Alloc.ii(killerTypes.size());
            shuffle();

            clear();

            IDebugPanelSett.add("Event Serial killer", new ACTION()
            {
                public void exe()
                {
                    init();
                    if (theKiller() == null)
                    {
                        LOG.ln("nay");
                    }
                }
            });
        }

        private void shuffle()
        {
            for (int i = 0; i < typeShuffle.Length; i++)
            {
                typeShuffle[i] = i;
            }
            for (int i = 0; i < typeShuffle.Length; i++)
            {
                int i2 = RND.rInt(typeShuffle.Length);
                int v = typeShuffle[i];
                typeShuffle[i] = typeShuffle[i2];
                typeShuffle[i2] = v;
            }
        }

        protected override void save(FilePutter file)
        {
            timer.save(file);
            file.i(type);
            file.i(day);
            file.bool(dormant);
            file.i(killerID);
            file.i(suspect);
            file.i(victimRace);
            file.i(victims);
            file.d(rate);
            file.isE(typeShuffle);
        }

        protected override void load(FileGetter file) throws IOException
        {
            timer.load(file);
            type = file.i();
            day = file.i();
            dormant = file.bool();
            killerID = file.i();
            suspect = file.i();
            victimRace = file.i();
            victims = file.i();
            rate = file.d();
            if (!file.isE(typeShuffle))
                shuffle();
            type = CLAMP.i(type, 0, killerTypes.size());
        }

        protected override void clear()
        {
            reset();
            rate = 0;
        }

        private void reset()
        {
            dormant = true;
            suspect = -1;
            victims = 0;
            timer.setD(interval);
            timer.incD(interval * 0.5 * RND.rFloat());
        }

        private void init()
        {
            killerID = Util.pickKiller();
            victimRace = Util.pickRace();
            victims = 0;
            rate = 0;
            suspect = -1;

            type++;
            type %= typeShuffle.Length;
            timer.setD(-1);
            if (theKiller() == null)
            {
                reset();
            }
        }

        protected override void update(double ds)
        {
            if (timer.getD() > 0)
            {
                if (ds > 0)
                    timer.incD(-ds);
                if (timer.getD() < 0)
                {
                    if (SETT.ROOMS().PRISON.instancesSize() > 0)
                    {
                        init();
                    }
                    else
                        reset();

                }

                if (rate > 0)
                {
                    rate -= ds * rateSpeed;
                    rate = CLAMP.d(rate, 0, 1.0);
                    if (rate <= 0)
                    {
                        setData(null);
                        messages.over(mData);
                    }
                }
                return;
            }

            if (suspect != -1)
            {
                if (day != TIME.days().bitsSinceStart())
                {
                    ENTITY e = SETT.ENTITIES().getByID(suspect);
                    if (e != null && e is Humanoid)
                    {
                        if (theKiller() == e)
                        {
                            setData(null);
                            STATS.LAW().prisonerType.set(mData.suspect.indu(), CRIMES.MURDER());
                            mData.suspect.HTypeSet(HTYPES.PRISONER(), CAUSE_LEAVES.PUNISHED(), null);
                            messages.caught(mData);

                            AIModule_Prisoner.DATA().punishmentSet.set(((Humanoid)e).ai(), CRIME_PUNISHMENTS.EXECUTE());
                            clear();
                        }
                        else
                        {
                            rate += 0.2;
                            rate = CLAMP.d(rate, 0, 1);
                            setData(null);
                            messages.fail(mData);
                            suspect = -1;
                        }
                    }
                }
            }

            if (theKiller() == null)
            {
                reset();
                return;
            }
            else if (theKiller().indu().hType() == HTYPES.PRISONER())
            {
                setData(null);
                messages.caught(mData);
                AIModule_Prisoner.DATA().punishmentSet.set(theKiller().ai(), CRIME_PUNISHMENTS.EXECUTE());
                clear();
            }

            if (day != TIME.days().bitsSinceStart())
            {
                dormant = !RND.oneIn(3);
                day = TIME.days().bitsSinceStart();
            }

            rate = (double)victims / type().messages.Length;
        }

        public KillerType type()
        {
            return killerTypes.get(typeShuffle[type]);
        }

        public Humanoid theKiller()
        {
            if (killerID == -1)
                return null;
            ENTITY e = SETT.ENTITIES().getByID(killerID);
            if (e is Humanoid)
            {
                Humanoid a = (Humanoid)e;
                if (a.indu().clas() == HCLASSES.CITIZEN() || a.indu().hType() == HTYPES.PRISONER())
                    return a;
            }
            return null;
        }

        public bool theKillerShouldKill()
        {
            return !dormant;
        }

        public Race victimRace()
        {
            if (victimRace == -1)
                return null;
            return RACES.all().get(victimRace);
        }

        public int murders()
        {
            return victims;
        }

        public void setSuspect(int suspect)
        {
            this.suspect = suspect;
            day = TIME.days().bitsSinceStart();
        }

        int suspect()
        {
            return suspect;
        }

        public void reportKill(Corpse corpse)
        {
            victims++;
            dormant = true;
            if (victims > type().messages.Length)
            {
                reset();
                return;
            }

            if (victims == type().messages.Length / 2)
            {
                int suspect = killerID;
                if (RND.rBoolean())
                    suspect = Util.pickKiller();
                if (suspect != -1)
                {
                    setData(corpse);
                    mData.suspect = (Humanoid)SETT.ENTITIES().getByID(suspect);
                    messages.murderSuspect(mData);
                    return;
                }
            }

            setData(corpse);
            messages.murder(mData);

            rate = (double)(victims - 1) / (type().messages.Length - 1);
            if (victims >= type().messages.Length)
            {
                reset();
            }
        }

        void setData(Corpse corpse)
        {
            mData.killer = theKiller();
            mData.type = type();
            mData.murders = victims;
            mData.race = victimRace();
            mData.suspect = mData.killer;
            mData.victim = corpse;
        }

        public double rate()
        {
            return rate;
        }
    }
}