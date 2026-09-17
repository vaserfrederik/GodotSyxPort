using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Thing.Pointlight
{
    using static Settlement.Main.SETT;

    using Game.Time;
    using Init.Constant;
    using Settlement.Main;
    using Snake2D;
    using Snake2D.Util.Color;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Light;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sets;
    using Snake2D.Util.Sprite;

    abstract class LightModel
    {
        static readonly LightModel candle = new LightModel(10, 0)
        {
            Flicker = (l, radius) =>
            {
                l.OffX = (int)(RND.rExpo() * RND.rInt0(7));
                l.OffY = (int)(RND.rExpo() * RND.rInt0(7));
                double intense = 5f + Math.Pow(RND.rFloat0(0.8), 3);
                l.R = intense * 1.7;
                l.G = intense * 0.8;
                l.B = intense * 0.4;
                l.Radius = radius - RND.rInt(8);
                l.Falloff = 1;
                l.Z = 30 + RND.rInt0(5);
                return 0.030f + RND.rFloat(0.05f);
            },

            RenderSprite = (x1, y1, ran) =>
            {
                SETT.LIGHTS().sprites.candle.RenderC(CORE.renderer(), ran & 0x07, x1, y1);
            },

            RenderFlame = (tx, ty, x1, y1, ran) =>
            {
                Flame(tx, ty, x1, y1, ran, SETT.LIGHTS().sprites.flame_small, 2);
            },

            Intensity = (x, y) =>
            {
                return base.Intensity(x, y);
            }
        };

        static readonly LightModel torch = new LightModel(10, 1)
        {
            Flicker = (l, radius) =>
            {
                l.OffX = (int)(RND.rExpo() * RND.rInt0(7));
                l.OffY = (int)(RND.rExpo() * RND.rInt0(7));
                double intense = 4f + RND.rExpo() * RND.rFloat0(0.8f);
                l.R = intense * 1.8;
                l.G = intense * 0.9;
                l.B = intense * 0.4;
                l.Radius = radius - RND.rInt(8);
                l.Falloff = 1;
                l.Z = 30 + RND.rInt0(5);
                return 0.025f + RND.rFloat(0.05f);
            },

            RenderSprite = (x1, y1, ran) =>
            {
            },

            RenderFlame = (tx, ty, x1, y1, ran) =>
            {
                Flame(tx, ty, x1, y1, ran, SETT.LIGHTS().sprites.flame_medium, 24);
            }
        };

        static readonly LightModel torch_big = new LightModel(12, 2)
        {
            Flicker = (l, radius) =>
            {
                l.OffX = (int)(RND.rExpo() * RND.rInt0(7));
                l.OffY = (int)(RND.rExpo() * RND.rInt0(7));
                double intense = 4f + RND.rExpo() * RND.rFloat0(0.8f);
                l.R = intense * 1.8;
                l.G = intense * 0.9;
                l.B = intense * 0.3;
                l.Radius = radius;
                l.Falloff = 1;
                l.Z = 30 + RND.rInt0(5);
                return 0.025f + RND.rFloat(0.05f);
            },

            RenderSprite = (x1, y1, ran) =>
            {
            },

            RenderFlame = (tx, ty, x1, y1, ran) =>
            {
                Flame(tx, ty, x1, y1, ran, SETT.LIGHTS().sprites.flame_big, 24);
            }
        };

        static readonly LightModel cave_light = new LightModel(10, 3)
        {
            Flicker = (l, radius) =>
            {
                l.OffX = 0;
                l.OffY = 0;
                l.R = 1.0;
                l.G = 1.0;
                l.B = 1.0;
                l.Radius = radius;
                l.Falloff = 1;
                l.Z = 50;
                return 0.1f;
            },

            RenderSprite = (x1, y1, ran) =>
            {
            },

            RenderFlame = (tx, ty, x1, y1, ran) =>
            {
                Flame(tx, ty, x1, y1, ran, SETT.LIGHTS().sprites.cave_light, 1);
            },

            Intensity = (x, y) =>
            {
                if (TERRAIN().get(x >> C.T_SCROLL, y >> C.T_SCROLL) == SETT.TERRAIN().CAVE)
                {
                    return 1.0;
                }
                return 0.0;
            }
        };

        static readonly List<LightModel> allModels = new List<LightModel> { candle, torch, torch_big, cave_light };

        readonly Func<PointLight, int, float> Flicker;
        readonly Action<int, int, int> RenderSprite;
        readonly Action<int, int, int, int, int> RenderFlame;

        readonly int tileDiameter;
        readonly PointLight[] lights;
        readonly PointTracer tracer;

        public LightModel(int tileDiameter, int index)
        {
            this.tileDiameter = tileDiameter;
            lights = new PointLight[32];
            for (int i = 0; i < 32; i++)
            {
                lights[i] = new PointLight();
            }
            tracer = new PointTracer(tileDiameter);
        }

        public static void RegisterAll(Renderer r, int ran, int x, int y, int offx, int offy)
        {
            foreach (var model in allModels)
            {
                model.Register(r, ran, x, y, offx, offy);
            }
        }

        public void Register(Renderer r, int ran, int x, int y, int offx, int offy)
        {
            double i = Intensity(x, y);

            if (i == 0)
                return;

            RenderFlame(x >> C.T_SCROLL, y >> C.T_SCROLL, x + offx, y + offy, ran);

            tracer.Init(x, y);

            PointLight light = lights[ran & 0x2F];

            double lr = light.R;
            double lg = light.G;
            double lb = light.B;

            light.R *= i;
            light.G *= i;
            light.B *= i;

            light.X = x + offx;
            light.Y = y + offy;

            int sx = ((x) & ~C.T_MASK) + offx - tileDiameter / 2 * C.TILE_SIZE;
            int sy = ((y) & ~C.T_MASK) + offy - tileDiameter / 2 * C.TILE_SIZE;

            for (int ty = 0; ty < tileDiameter; ty++)
            {
                for (int tx = 0; tx < tileDiameter; tx++)
                {
                    x = sx + tx * C.TILE_SIZE;
                    y = sy + ty * C.TILE_SIZE;
                    if (tracer.LitIs(tx, ty))
                        r.RegisterLight(
                            light,
                            x, x + C.TILE_SIZE,
                            y, y + C.TILE_SIZE,
                            tracer.GetSide(tx, ty, DIR.NE),
                            tracer.GetSide(tx, ty, DIR.SE),
                            tracer.GetSide(tx, ty, DIR.SW),
                            tracer.GetSide(tx, ty, DIR.NW));
                }
            }

            light.R = lr;
            light.G = lg;
            light.B = lb;
        }

        protected virtual double Intensity(int x, int y)
        {
            double i = 1.0;

            if (TERRAIN().get(x >> C.T_SCROLL, y >> C.T_SCROLL) == SETT.TERRAIN().CAVE)
            {
                if (TIME.light().dayIs())
                    i *= 0.75;
                else if (TIME.light().partOfCircular() < 0.2)
                {
                    i *= 0.75 + 0.75 * (TIME.light().partOfCircular() / 0.2);
                }
                else
                {
                    i *= 1.5;
                }
            }
            else if (TERRAIN().get(x >> C.T_SCROLL, y >> C.T_SCROLL).roofIs())
            {
                if (TIME.light().dayIs())
                    i *= 0.25;
                else if (TIME.light().partOfCircular() < 0.2)
                {
                    i *= 0.25 + (TIME.light().partOfCircular() / 0.2);
                }
            }
            else if (TIME.light().partOfCircular() < 0.2)
            {
                if (TIME.light().dayIs())
                    return 0;
                i *= ((TIME.light().partOfCircular()) / 0.2);
            }
            else if (TIME.light().dayIs())
            {
                return 0;
            }
            return i;
        }

        void Flame(int tx, int ty, int x1, int y1, int ran, TILESprite sprite, int sparks)
        {
            double i = Intensity(x1, y1);

            if (i == 0)
                return;

            RenderSprite(x1, y1, ran);

            double wi = 0;
            if (TERRAIN().get(tx, ty).roofIs())
                wi = 0;
            FireSparks.Render(x1, y1, sparks, ran, wi);

            int w = sprite.size() / 2;

            sprite.Render(CORE.renderer(), ran & 0x07, x1 - w, y1 - w);

            x1 -= C.TILE_SIZEH;
            y1 -= C.TILE_SIZEH;

            OPACITY.O75.Bind();
            TextureCoords d, c;
            c = SETT.LIGHTS().sprites.texture.get((ran >> 4) & 0x07, (ran >> 5) & 0x07);
            d = SETT.LIGHTS().sprites.displacement.get((ran >> 4) & 0x07, (ran >> 5) & 0x07);
            CORE.renderer().RenderDisplaced(x1, x1 + C.TILE_SIZE, y1, y1 + C.TILE_SIZE, d, c);
            OPACITY.Unbind();
        }

        class PointLight : LIGHT_POINT
        {
            public double R, G, B;
            public double Falloff;
            public int Radius;
            public int OffX, OffY;
            public double X, Y, Z;
            public double Timer;

            public float getRed() => (float)R;
            public float getGreen() => (float)G;
            public float getBlue() => (float)B;
            public float getFalloff() => (float)Falloff;
            public int getRadius() => Radius;
            public float cx() => (float)(X + OffX);
            public float cy() => (float)(Y + OffY);
            public float cz() => (float)Z;
        }
    }
}