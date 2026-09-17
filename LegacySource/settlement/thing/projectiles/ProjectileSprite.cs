using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.rendering;
using util.spritecomposer;
using util.spritecomposer.ComposerDests;
using util.spritecomposer.ComposerSources;
using util.spritecomposer.ComposerThings;
using util.spritecomposer.ComposerUtil;

namespace settlement.thing.projectiles
{
    public abstract class ProjectileSprite
    {
        private static readonly KeyMap<TILE_SHEET> map = new KeyMap<TILE_SHEET>();

        static ProjectileSprite()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    map.Clear();
                }
            };
        }

        public static readonly ProjectileSprite DUMMY = new ProjectileSprite
        {
            public override void RenderProj(Projectile p, double ref, Renderer r, ShadowBatch s, double x, double y, int h, int ran,
                double dx, double dy, double dz, float ds, int zoomout)
            {
                if (zoomout < 2)
                {
                    double l = Math.Sqrt(dx * dx + dy * dy + dz * dz * 4);
                    dx /= l;
                    dy /= l;
                    dx *= C.SCALE;
                    dy *= C.SCALE;
                    for (int k = 0; k < 8; k++)
                    {
                        r.RenderParticle((int)x, (int)y);
                        x += dx;
                        y += dy;
                    }
                }

                s.SetDistance2Ground(h / 4);
                SPRITES.icons().s.dot.RenderC(s, (int)x, (int)y);
            }
        };

        public static ProjectileSprite Get(Json json)
        {
            COLOR col = new ColorImp(json, "COLOR");
            if (json.Has("SPRITE_FILE"))
                return Get(col, json.Value("SPRITE_FILE"));
            else
            {
                COLOR[] cols = Cols(col);
                return new ProjectileSprite
                {
                    public override void RenderProj(Projectile p, double ref, Renderer r, ShadowBatch s, double x, double y, int h, int ran,
                        double dx, double dy, double dz, float ds, int zoomout)
                    {
                        if (zoomout < 2)
                        {
                            cols[ran & 0b0111111].Bind();
                            double l = Math.Sqrt(dx * dx + dy * dy + dz * dz * 4);
                            dx /= l;
                            dy /= l;
                            dx *= C.SCALE;
                            dy *= C.SCALE;
                            for (int k = 0; k < 8; k++)
                            {
                                r.RenderParticle((int)x, (int)y);
                                x += dx;
                                y += dy;
                            }
                            COLOR.Unbind();
                        }

                        s.SetHeight(0).SetDistance2Ground(h / 4);
                        SPRITES.icons().s.dot.RenderC(s, (int)x, (int)y);
                    }
                };
            }
        }

        public static ProjectileSprite Get(COLOR col, string key)
        {
            if (!map.ContainsKey(key))
            {
                TILE_SHEET sheet = new ITileSheet(PATHS.SPRITE_SETTLEMENT().GetFolder("projectile").Get(key), 112, 28)
                {
                    protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.singles.Init(0, 0, 2, 1, 1, 1, d.s16);
                        for (int i = 0; i < 4; i++)
                        {
                            s.singles.SetVar(0).PasteRotated(i, true);
                            s.singles.SetVar(1).PasteRotated(i, true);
                        }

                        return d.s16.SaveGame();
                    }
                }.Get();
                map.Put(key, sheet);
            }
            COLOR[] cols = Cols(col);
            TILE_SHEET sheet = map.Get(key);
            return new ProjectileSprite
            {
                public override void RenderProj(Projectile p, double ref, Renderer r, ShadowBatch s, double x, double y, int h,
                    int ran, double dx, double dy, double dz, float ds, int zoomout)
                {
                    int i = DIR.Get(dx, dy).Id();
                    double sc = 1 + h * C.ITILE_SIZE * 0.125;

                    int w = (int)(sheet.Size() * sc);
                    int x1 = (int)(x - w / 2);
                    int y1 = (int)(y - w / 2);
                    int x2 = x1 + w;
                    int y2 = y1 + w;

                    cols[ran & 0b0111111].Bind();
                    sheet.Render(r, i, x1, x2, y1, y2);
                    s.SetHeight(0).SetDistance2Ground(h / 4);
                    sheet.RenderC(s, i, (int)x, (int)y);
                    COLOR.Unbind();
                }
            };
        }

        public abstract void RenderProj(Projectile p, double ref, Renderer r, ShadowBatch s, double x, double y, int h, int ran, double dx, double dy, double dz,
            float ds, int zoomout);

        private ProjectileSprite()
        {
        }

        private static COLOR[] Cols(COLOR col)
        {
            COLOR[] cols = new COLOR[64];
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i] = new ColorImp(
                    CLAMP.i(col.Red() + RND.rInt(5), 0, 128),
                    CLAMP.i(col.Green() + RND.rInt(5), 0, 127),
                    CLAMP.i(col.Blue() + RND.rInt(5), 0, 127))
                    .ShadeSelf(RND.rFloat1(0.5));
            }
            return cols;
        }
    }
}