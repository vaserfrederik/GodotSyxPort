using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using game;
using init.constant;
using init.resources;
using init.settings;
using settlement.entity.animal;
using settlement.main;
using settlement.thing.DRAGGABLE;
using settlement.thing.THINGS;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.gui.misc;
using util.rendering;
using util.text;
using util.updating;
using view.sett;

namespace settlement.thing
{
    [Serializable]
    public class ThingsCadavers : ThingFactory<ThingsCadavers.Cadaver>, ISerializable
    {
        private const long serialVersionUID = 1L;
        private readonly Cadaver[] cadavers = new Cadaver[1024];
        private readonly IUpdater updater;

        public ThingsCadavers(IList<ThingFactory<?>> all) : base(all, 1024)
        {
            updater = new IUpdater(cadavers.Length, 5.0);

            for (int i = 0; i < cadavers.Length; i++)
            {
                cadavers[i] = new Cadaver(i);
            }
        }

        public readonly DRAGGABLE_HOLDER draggable = new DRAGGABLE_HOLDER
        {
            draggable = (index) => cadavers[index]
        };

        protected override Cadaver[] all()
        {
            return cadavers;
        }

        protected override void save(FilePutter file)
        {
            updater.save(file);
            base.save(file);
        }

        protected override void load(FileGetter file)
        {
            updater.load(file);
            base.load(file);
        }

        public Cadaver gore(int cx, int cy, AnimalSpecies s)
        {
            Cadaver c = nextInLine();
            c.gore(cx, cy, s);
            return c;
        }

        public Cadaver normal(int cx, int cy, double weight, float damage, AnimalSpecies s, int rot)
        {
            Cadaver c = nextInLine();
            c.normal(cx, cy, (float)(1.0 - damage), rot, s, weight);
            return c;
        }

        public Cadaver skelleton(int cx, int cy, AnimalSpecies spec, int rot)
        {
            Cadaver c = nextInLine();
            c.skelleton(cx, cy, rot, spec);
            return c;
        }

        public Cadaver rotten(int cx, int cy, AnimalSpecies spec, int rot)
        {
            Cadaver c = nextInLine();
            c.rotten(cx, cy, rot, spec);
            return c;
        }

        public override void update(double ds)
        {
            updater.update(ds);
        }

        public Cadaver getByIndex(int index)
        {
            return cadavers[index];
        }

        private static readonly string ¤¤name = "Cadaver";
        private static readonly string ¤¤noRes = "No resources";

        static ThingsCadavers()
        {
            D.ts(typeof(ThingsCadavers));
        }

        [Serializable]
        public class Cadaver : Thing, SETT_HOVERABLE, DRAGGABLE
        {
            private const byte stateGore = 0;
            private const byte stateNormal = 1;
            private const byte stateRotten = 2;
            private const byte stateSkelleton = 3;

            private byte state;

            private static readonly ColorImp decayColor = new ColorImp(50, 50, 50);
            private short resExtracted;
            private short resAvailable;
            private short weight;

            private float timer;
            private Rec body = new Rec(AnimalSpecies.SIZE);
            private byte rot;
            private byte ran;
            private float statef;
            private byte spec;

            public Cadaver(int index) : base(index) { }

            protected override void save(FilePutter f)
            {
                f.b(state);
                f.s(resExtracted);
                f.s(resAvailable);
                f.s(weight);
                f.f(timer);
                body.save(f);
                f.b(rot);
                f.b(ran);
                f.f(statef);
                SETT.ANIMALS().map.saver().save(spec(), f);
            }

            protected override void load(FileGetter f)
            {
                state = f.b();
                resExtracted = f.s();
                resAvailable = f.s();
                weight = f.s();
                timer = f.f();
                body.load(f);
                rot = f.b();
                ran = f.b();
                statef = f.f();
                spec = (byte)SETT.ANIMALS().map.loader().loadB(f, null).index();
            }

            public override RECTANGLE body()
            {
                return body;
            }

            public void gore(int cx, int cy, AnimalSpecies spec)
            {
                state = stateGore;
                timer = 120;
                body.moveC(cx, cy);
                rot = (byte)RND.rInt(8);
                this.spec = (byte)spec.index();
                this.resAvailable = 0;
                add();
            }

            public void normal(int cx, int cy, float state, int rot, AnimalSpecies spec, double weight)
            {
                this.state = stateNormal;
                timer = 360 * 8;
                body.setDim(spec.hitBoxSize());
                body.moveC(cx, cy);
                this.rot = (byte)rot;
                this.spec = (byte)spec.index();
                statef = state;
                this.resExtracted = 0;
                this.weight = (short)Math.Ceiling(weight);
                resAvailable = 0;
                for (int i = 0; i < spec().resources().size(); i++)
                {
                    resAvailable += spec().resAmount(i, this.weight);
                }

                add();
            }

            public AnimalSpecies spec()
            {
                return SETT.ANIMALS().species.get(spec);
            }

            public void rotten(int cx, int cy, int rot, AnimalSpecies spec)
            {
                state = stateRotten;
                timer = 260;
                body.moveC(cx, cy);
                rot = (byte)rot;
                this.spec = (byte)spec.index();
                this.resAvailable = 0;
                add();
            }

            public void skelleton(int cx, int cy, int rot, AnimalSpecies spec)
            {
                state = stateSkelleton;
                timer = 360;
                body.moveC(cx, cy);
                rot = (byte)rot;
                this.spec = (byte)spec.index();
                add();
            }

            public override void render(Renderer renderer)
            {
                // Implement rendering logic here
            }

            public override void hover(GBox text)
            {
                text.textL(¤¤name);
                text.NL();
                if (!S.get().developer)
                    return;
                if (!resHas())
                    text.text(¤¤noRes);
                else
                {
                    int am = 0;
                    for (int i = 0; i < spec().resources().size(); i++)
                    {
                        int tot = spec().resAmount(i, weight);
                        am += tot;
                        int a = am - resExtracted;
                        a = CLAMP.i(a, 0, tot);
                        text.setResource(spec().resources().get(i), a, tot);
                    }
                }
                if (S.get().developer)
                {
                    text.add(text.text().add(timer));
                    text.add(text.text().add(state));
                }
            }

            protected override int z()
            {
                return 99;
            }

            public override ThingFactory<?> factory()
            {
                return SETT.THINGS().cadavers;
            }

            public void drag(DIR d, int cx, int cy, int fromDist)
            {
                rot = (byte)d.perpendicular().id();
                body.moveC(cx - fromDist * d.xN(), cy - fromDist * d.yN());
                if (body.cX() < 0)
                    body.moveCX(0);
                if (body.cX() >= SETT.PIXEL_BOUNDS.x2())
                    body.moveCX(SETT.PIXEL_BOUNDS.x2() - 1);
                if (body.cY() < 0)
                    body.moveCY(0);
                if (body.cY() >= SETT.PIXEL_BOUNDS.y2())
                    body.moveCY(SETT.PIXEL_BOUNDS.y2() - 1);

                if (COORDINATE.tileDistance(cx, cy, body().cX(), body().cY()) > 3 * C.TILE_SIZE)
                    LOG.ln(cx + " " + cy + " " + fromDist);

                base.move();
            }

            public void drag(DIR d, int cx, int cy)
            {
                drag(d, cx, cy, body.width());
            }

            public bool canBeDragged()
            {
                return !isRemoved();
            }

            public void makeSkelleton()
            {
                resExtracted = resAvailable;
                state = stateSkelleton;
                timer = 360;
            }
        }
    }
}