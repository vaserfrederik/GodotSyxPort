using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using game.GAME;
using game.boosting.BOOSTABLES;
using game.events.EVENTS;
using game.time;
using init.constant;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main;
using settlement.stats;
using snake2d.util.MATH;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.rnd;
using util.gui.misc;
using util.text;
using view.main;
using view.sett;
using view.tool;
using view.ui.message;

namespace game.events.disaster
{
    public class EventAccident : EventResource
    {
        private const double RADIUS = C.TILE_SIZE * 20;
        private readonly Rec bounds = new Rec(RADIUS * 2);
        private readonly VectorImp tVec = new VectorImp();

        private readonly ECollision coll = new ECollision();

        private static readonly CharSequence ¤¤Accident = "¤Accident!";
        private static readonly CharSequence ¤¤AccidentD = "¤An accident has occurred. {0} subjects were injured and will seek out a hospital. There were {1} deaths.";
        private static readonly CharSequence ¤¤Go = "¤Go to Site";

        private double[] timers;
        private double timer = 0;
        private readonly double acI;

        static EventAccident()
        {
            D.ts(typeof(EventAccident));
        }

        public EventAccident() : base("ACCIDENTS")
        {
            timers = new double[SETT.ROOMS().all().size()];
            acI = timers.Length / (TIME.secondsPerDay() * 16.0);

            IDebugPanelSett.add(new PlacableSimple("event: accident")
            {
                public override void place(int x, int y)
                {
                    ENTITY e = SETT.ENTITIES().getAtPoint(x, y);
                    if (e != null && e is Humanoid)
                    {
                        create((Humanoid)e);
                    }
                }

                public override CharSequence isPlacable(int x, int y)
                {
                    ENTITY e = SETT.ENTITIES().getAtPoint(x, y);
                    if (e != null && e is Humanoid)
                    {
                        return null;
                    }
                    return E;
                }
            });

            clear();
        }

        protected override void update(double ds)
        {
            int i = (int)timer;
            timer += ds;
            if (i != (int)timer)
            {
                RoomBlueprint b = SETT.ROOMS().all().get(i);

                if (b is RoomBlueprintIns<?> ins && b != null && b.employment() != null)
                {
                    double emp = b.employment().employed() - 150;
                    if (emp < 0)
                        return;
                    emp = Math.Pow(emp, 1.2);

                    double c = acI * b.employment().accidentsPerYear * emp;
                    c /= BOOSTABLES.CIVICS().ACCIDENT.get(HCLASS_RACE.clP());
                    c = CLAMP.d(c, 0, 1);

                    timers[i] -= c;

                    if (timers[i] < -10)
                    {
                        create(ins);
                    }
                }
            }

            if (timer >= timers.Length)
                timer -= timers.Length;
        }

        protected override void save(FilePutter file)
        {
            SETT.ROOMS().collection.saver().save(timers, file);
            file.d(timer);
        }

        protected override void load(FileGetter file) throws IOException
        {
            SETT.ROOMS().collection.loader().load(timers, file, 0);
            timer = file.d();
        }

        protected override void clear()
        {
            Array.Fill(timers, 0);
            timer = 0;
        }

        public bool create(RoomBlueprintIns<?> b)
        {
            if (b.employment().employed() <= 0)
            {
                timers[b.index()] = 0;
                return false;
            }

            if (!MATH.isWithin(TIME.days().bitPartOf(), b.employment().getShiftStart() + 0.1, b.employment().getShiftStart() + 0.6))
            {
                return false;
            }

            int emp = RND.rInt(b.employment().employed());
            for (int i = 0; i < b.instancesSize(); i++)
            {
                RoomInstance ins = b.getInstance(i);
                if (ins.employees().employed() > 0)
                {
                    emp -= ins.employees().employed();

                    if (emp <= 0)
                    {
                        return create(ins);
                    }
                }
            }

            return false;
        }

        public bool create(RoomInstance ins)
        {
            foreach (Humanoid h in ins.employees().employees())
            {
                Room r = STATS.WORK().EMPLOYED.get(h);
                if (r != null && r.blueprint().employment() != null && r == SETT.ROOMS().map.get(h.tc()))
                {
                    int am = create(h);
                    timers[ins.blueprint().index()] += am;
                    return true;
                }
            }
            return false;
        }

        public int create(Humanoid h)
        {
            Room r = STATS.WORK().EMPLOYED.get(h);
            double cx = h.body().cX() + RND.rSign();
            double cy = h.body().cY() + RND.rSign();

            double mom = EPHYSICS.MOM_TRESHOLDI + RND.rFloat() * 2 * EPHYSICS.MOM_TRESHOLDI;

            h.inflictDamage(1.0, CAUSE_LEAVES.getAccident());
            int death = 1;
            int inj = 0;

            bounds.moveC(cx, cy);
            SETT.THINGS().gore.debris((int)cx, (int)cy, 0, 0);

            foreach (ENTITY e in SETT.ENTITIES().fill(bounds))
            {
                if (SETT.ROOMS().map.get(e.tc()) != r)
                    continue;

                double l = tVec.set(cx, cy, e.body().cX(), e.body().cY());
                if (l > RADIUS)
                    continue;
                l = 1.0 - (l / RADIUS);
                e.speed.setRaw(e.speed.x() + tVec.nX() * C.TILE_SIZE * 3 * l, e.speed.y() + tVec.nY() * C.TILE_SIZE * 3 * l);

                coll.dirDot = 1.0;
                coll.tileMomentum = mom * e.physics.getMass();
                coll.damagetileStrength = 0;
                coll.norX = tVec.nX();
                coll.norY = tVec.nY();
                coll.leave = CAUSE_LEAVES.getAccident();
                coll.other = null;
                if (e is Humanoid h2)
                {
                    h2.inflictDamage(l * RND.rFloat() * 2.0, CAUSE_LEAVES.getAccident());
                    if (e.isRemoved())
                    {
                        death++;
                        continue;
                    }
                    else if (!STATS.NEEDS().INJURIES.inDanger(h2.indu()))
                    {
                        HEvent.Handler.alertDanger(h2);
                    }
                    else
                    {
                        inj++;
                    }
                }
                e.collide(coll);
            }

            GAME.count().ACCIDENTS.inc(1);
            new M(¤¤Accident, inj, death, h).send();
            return inj + death;
        }

        private class M : MessageSection
        {
            private readonly int inj;
            private readonly int deaths;
            private int cx, cy;

            public M(CharSequence title, int inj, int deaths, Humanoid h) : base(title)
            {
                this.inj = inj;
                this.deaths = deaths;
                cx = h.tc().x();
                cy = h.tc().y();
            }

            protected override void make(GuiSection section)
            {
                Str s = Str.TMP;
                s.clear();
                s.add(¤¤AccidentD);
                s.insert(0, inj);
                s.insert(1, deaths);
                paragraph(s);

                GButt.ButtPanel p = new GButt.ButtPanel(¤¤Go)
                {
                    protected override void clickA()
                    {
                        VIEW.s().activate();
                        VIEW.s().getWindow().centererTile.set(cx, cy);
                        VIEW.messages().hide();
                    }
                };

                section.addRelBody(8, DIR.S, p);
            }
        }
    }
}