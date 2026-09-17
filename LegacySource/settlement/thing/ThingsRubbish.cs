using System;
using System.Collections.Generic;
using System.IO;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.rendering;
using util.updating;

namespace settlement.thing
{
    public class ThingsRubbish
    {
        private readonly RubbishHolder rubbish;
        private readonly RubbishMovingHolder rubbishMoving;

        public ThingsRubbish(LISTE<ThingFactory<?>> all)
        {
            rubbish = new RubbishHolder(all);
            rubbishMoving = new RubbishMovingHolder(all);
        }

        public void Update(float ds)
        {
            rubbish.Update(ds);
            rubbishMoving.Update(ds);
        }

        public void Throw(int sx, int sy, int destx, int desty)
        {
            rubbishMoving.Make(sx, sy, destx, desty);
        }

        private class ThingRubbish : Thing
        {
            private readonly Rec body = new Rec(C.SCALE * 8, C.SCALE * 8);
            private byte hour;
            private byte ran;

            public ThingRubbish(int index) : base(index)
            {
            }

            protected override void Init(int cx, int cy, byte ran)
            {
                body.MoveC(cx, cy);
                this.ran = ran;
                hour = (byte)((TIME.hours().BitsSinceStart() - RND.rInt(TIME.hoursPerDay() / 4)) & 0b01111111);
                Add();
            }

            public override RECTANGLE Body()
            {
                return body;
            }

            public override void Render(Renderer r, ShadowBatch shadows, float ds, int offsetX, int offsetY)
            {
                double t = Age();
                t /= TIME.hoursPerDay() * 0.25;
                t = CLAMP.d(t, 0, 1);
                ColorImp.TMP.Interpolate(COLOR.WHITE100, COLOR.DARK_BROWN, t);
                ColorImp.TMP.Bind();
                THINGS().sprites.rubbish.Render(r, ran & 0x0F, body.X1() + offsetX, body.Y1() + offsetY);
                COLOR.Unbind();
                shadows.SetHeight(1).SetDistance2Ground(0);
                THINGS().sprites.rubbish.Render(shadows, ran & 0x0F, body.X1() + offsetX, body.Y1() + offsetY);
            }

            private int Age()
            {
                int h = TIME.hours().BitsSinceStart() & 0b01111111;
                if (h < hour)
                {
                    return hour - h;
                }
                h = h - hour;
                return h;
            }

            protected override int Z()
            {
                return 0;
            }

            protected override void Save(FilePutter f)
            {
                body.Save(f);
                f.B(hour);
                f.B(ran);
            }

            protected override void Load(FileGetter f)
            {
                body.Load(f);
                hour = f.B();
                ran = f.B();
            }

            public override ThingFactory<?> Factory()
            {
                return SETT.THINGS().rubbish.rubbish;
            }
        }

        private sealed class RubbishHolder : ThingFactory<ThingRubbish>
        {
            private readonly ThingRubbish[] gore = new ThingRubbish[5000];
            private readonly IUpdater up = new IUpdater(5000, TIME.secondsPerDay())
            {
                protected override void Update(int i, double timeSinceLast)
                {
                    ThingRubbish r = gore[i];

                    if (!r.IsRemoved())
                    {
                        int hour = r.Age();
                        if (hour >= TIME.hoursPerDay())
                            r.Remove();
                    }
                }
            };

            public RubbishHolder(LISTE<ThingFactory<?>> all) : base(all, 5000)
            {
                for (int i = 0; i < gore.Length; i++)
                {
                    gore[i] = new ThingRubbish(i);
                }
            }

            public void Make(int cx, int cy)
            {
                ThingRubbish f = NextInLine();
                f.Init(cx, cy, (byte)(RND.rInt() & 0xFF));
                f.Add();
            }

            public void Make(int cx, int cy, byte ran)
            {
                ThingRubbish f = NextInLine();
                f.Init(cx, cy, ran);
            }

            protected override void Update(double ds)
            {
                up.Update(ds);
            }

            protected override void Save(FilePutter file)
            {
                up.Save(file);
                base.Save(file);
            }

            protected override void Load(FileGetter file)
            {
                up.Load(file);
                base.Load(file);
            }

            protected override void Clear()
            {
                up.Clear();
                base.Clear();
            }

            protected override ThingRubbish[] All()
            {
                return gore;
            }
        }

        private class ThingRubbishMoving : Thing
        {
            private static readonly VectorImp vec = new VectorImp();
            private byte ran;
            private double z;
            private double dx;
            private double dy;

            private double x;
            private double y;

            public ThingRubbishMoving(int index) : base(index)
            {
            }

            protected override void Init(int cx, int cy, int destx, int desty)
            {
                ran = (byte)RND.rInt();
                x = cx;
                y = cy;

                destx += RND.rInt0(C.TILE_SIZE);
                desty += RND.rInt0(C.TILE_SIZE);
                if (cx == destx && cy == desty)
                    return;

                z = vec.Set(cx, cy, destx, desty);

                dx = vec.NX();
                dy = vec.NY();
                double speed = 10 * C.TILE_SIZE + RND.rFloat(4 * C.TILE_SIZE);
                dx *= speed;
                dy *= speed;
                z = z / speed;

                Add();
            }

            public bool Update(double ds)
            {
                z -= ds;

                if (z <= 0)
                {
                    Remove();
                    THINGS().rubbish.rubbish.Make(body.CX(), body.CY(), ran);
                    return false;
                }
                x += ds * dx;
                y += ds * dy;

                if (z < 50)
                {
                    if (SETT.ENTITIES().GetAtPoint((int)x, (int)y) != null)
                    {
                        Remove();
                        THINGS().rubbish.rubbish.Make(body.CX(), body.CY(), ran);
                        return false;
                    }
                }
                return true;
            }

            public override RECTANGLE Body()
            {
                return rec;
            }

            public override void Render(Renderer r, ShadowBatch shadows, float ds, int offsetX, int offsetY)
            {
                THINGS().sprites.rubbish.Render(r, ran & 0x0F, body.X1() + offsetX, body.Y1() + offsetY);
                shadows.SetHeight(1).SetDistance2Ground(z / 25);
                THINGS().sprites.rubbish.Render(shadows, ran & 0x0F, body.X1() + offsetX, body.Y1() + offsetY);
            }

            protected override int Z()
            {
                return (int)z;
            }

            protected override void Save(FilePutter f)
            {
                f.D(x);
                f.D(y);
                f.D(z);
                f.D(dx);
                f.D(dy);
                f.B(ran);
            }

            protected override void Load(FileGetter f)
            {
                x = f.D();
                y = f.D();
                z = f.D();
                dx = f.D();
                dy = f.D();
                ran = f.B();
            }

            private readonly Rec rec = new Rec
            {
                X1 = () => (int)y - 4 * C.SCALE,
                Y1 = () => (int)x - 4 * C.SCALE,
                Width = () => 8 * C.SCALE,
                Height = () => 8 * C.SCALE
            };
        }

        private sealed class RubbishMovingHolder : ThingFactory<ThingRubbishMoving>
        {
            private readonly ThingRubbishMoving[] gore;

            public RubbishMovingHolder(LISTE<ThingFactory<?>> all) : base(all, 1024)
            {
                this.gore = new ThingRubbishMoving[1024];
                for (int i = 0; i < gore.Length; i++)
                {
                    gore[i] = new ThingRubbishMoving(i);
                }
            }

            public void Make(int cx, int cy, int sx, int sy)
            {
                ThingRubbishMoving f = NextInLine();
                f.Init(cx, cy, sx, sy);
            }

            protected override void Update(double ds)
            {
                ThingRubbishMoving g = First();
                ThingRubbishMoving drop = null;
                while (g != null)
                {
                    if (drop == g)
                        break;
                    ThingRubbishMoving next = Next(g);
                    if (g.Update(ds))
                    {
                        if (drop != null)
                            drop = g;
                    }
                    g = next;
                }
            }

            protected override ThingRubbishMoving[] All()
            {
                return gore;
            }
        }
    }
}