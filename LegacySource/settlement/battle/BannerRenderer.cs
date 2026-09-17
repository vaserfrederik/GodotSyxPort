using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.rendering;

namespace settlement.battle
{
    public class BannerRenderer
    {
        private readonly ArrayList<Humanoid> bannerMen = new ArrayList<Humanoid>(512);

        int period = 64;
        int periods = 16;
        int totL = period * periods;
        int totM = totL - 1;

        private readonly VectorImp vec = new VectorImp();

        private readonly int[] divSwayOff = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY * 2);
        private readonly int[] divLength = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY * 2);
        private readonly double[] winDir = new double[totL];

        private readonly byte[] offX = Alloc.bb(totL);
        private readonly double[] width = new double[totL];

        private readonly byte[] nx = Alloc.bb(totL);
        private readonly byte[] ny = Alloc.bb(totL);
        private readonly byte[] nz = Alloc.bb(totL);
        private readonly byte na = (byte)0xFF;


        public BannerRenderer()
        {
            for (int pi = 0; pi < periods; pi++)
            {
                double h = 16 + RND.rFloat() * 24;
                for (int i = 0; i < period; i++)
                {
                    double d = i * 4.0 * Math.PI / period;
                    offX[pi * period + i] = (byte)(h * Math.Sin(d));

                    double dn = (1 + Math.Cos(d));

                    vec.setAngle(dn);
                    double nx = vec.nX();
                    double ny = vec.nY();
                    double nz = 1;
                    double l = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    nx /= l;
                    ny /= l;
                    nz /= l;
                    this.nx[pi * period + i] = (byte)(128 + 127 * nx);
                    this.ny[pi * period + i] = (byte)(128 + 127 * ny);
                    this.nz[pi * period + i] = (byte)(128 + 127 * nz);
                }
            }

            for (int i = 0; i < totL; i++)
            {
                int l = 16 + RND.rInt(period - 16);
                double h = RND.rFloat(C.SCALE);
                for (int k = 0; k < l && i < totL; k++)
                {
                    double d = (double)k / l;
                    d = (Math.Sin(d * Math.PI * 2 - Math.PI * 0.5) + 1) / 2.0;
                    width[i] = (h * d);

                    i++;
                }
            }

            for (int i = 0; i < period / 2; i++)
            {
                double d = i / (period * 0.5);

                width[totL - 1 - i] = (byte)(width[totL - 1 - i] * d + width[0] * (1 - d));
            }

            for (int i = 0; i < divSwayOff.Length; i++)
            {
                divSwayOff[i] = RND.rInt();
                divLength[i] = period / 4 + RND.rInt(period / 2);
            }

            winDir[0] = 0;
            double max = 0.1 + RND.rFloat();
            double sp = 0.1 + RND.rFloat() * 0.1;
            for (int i = 1; i < winDir.Length; i++)
            {
                double n = winDir[i - 1] + sp;

                if (n <= 0)
                {
                    n = -n;
                    max = 0.1 + RND.rFloat();
                    sp = 0.05 + RND.rFloat() * 0.05;
                }
                else if (n >= max)
                {
                    n = max - (n - max);
                    sp = -(0.05 + RND.rFloat() * 0.05);
                }
                winDir[i] = n;
            }

            for (int i = 0; i < nx.Length; i++)
            {
            }
        }

        private readonly ColorImp col = new ColorImp();

        public void render(Renderer r, ShadowBatch s, int x, int y, Div div, VECTOR speed, int rran, Rec bounds)
        {
            {
                x += speed.dir().xN() * 12;
                y -= speed.dir().yN() * 12;
                COLOR.BROWN.bind();
                SPRITES.cons().BIG.line.render(r, 0, x - C.TILE_SIZEH, y - C.TILE_SIZEH);
                SPRITES.cons().BIG.line.render(s, 0, x - C.TILE_SIZEH, y - C.TILE_SIZEH);
            }

            int ranoff = divSwayOff[div.index()] + rran;
            col.set(div.info.banner().col);
            col.shadeSelf(0.75 + (double)0.5 * ((rran >> 8) & 0x0FF) / 0x0FF);
            col.bind();

            double w = SETT.WEATHER().wind.getD();
            double wi = 1.0 - w;

            {
                int si = ranoff + (int)(10 * TIME.currentSecond());
                double wd = winDir[si & totM];
                double xx = 0.5 - wi * (wd * 0.4 - 0.2);

                double dx = -xx;
                double dy = 1.0 - xx;
                dx *= C.TILE_SIZE * w * 6;
                dy *= C.TILE_SIZE * w * 6;
                dx -= speed.x();
                dy -= speed.y();

                w = vec.set(dx, dy) / (6 * C.TILE_SIZE);
                w = CLAMP.d(w, 0, 1);
                w = 0.4 + 0.6 * w;
                wi = 1.0 - w;
            }

            int startI = ranoff + (int)(80 * TIME.currentSecond());

            int length = divLength[div.index()];

            int lll = 0;

            for (int k = -2; k <= 2; k++)
            {
                int ll = length - lll;
                lll++;
                for (int i = 0; i < ll; i++)
                {
                    double d = (double)i / length;
                    int oi = (int)(startI - i * wi * 2);
                    oi = oi & totM;

                    int rx = (int)(x + w * vec.nX() * i * C.SCALE);
                    int ry = (int)(y + w * vec.nY() * i * C.SCALE);

                    rx += -vec.nY() * wi * d * offX[oi];
                    ry += vec.nX() * wi * d * offX[oi];

                    double dh = width[oi];

                    int ni = (i * period + k + (int)(vec.nX() * 16)) & totM;
                    int dx = (int)(dh * (1) * k * vec.nX());
                    int dy = (int)(dh * (1) * k * vec.nY());
                    int px = rx - dy;
                    int py = ry + dx;
                    if (bounds.holdsPoint(px, py))
                    {
                        r.renderParticle(px, py, nx[ni], ny[ni], nz[ni], na);
                        if (k == 0)
                            SPRITES.icons().s.dot.renderC(s, rx - dy, ry + dx);
                    }
                }
            }
        }

        public void regBannerman(Humanoid h)
        {
            if (bannerMen.hasRoom())
            {
                bannerMen.add(h);
            }
        }

        public void renderAll(Renderer r, ShadowBatch s, int offX, int offY, RECT gamePixels)
        {
            s.setDistance2Ground(12);
            s.setHeight(2);

            int offXs = (int)(offX - gamePixels.x1());
            int offYs = (int)(offY - gamePixels.y1());

            bounds.set(gamePixels);
            bounds.incr(offXs, offYs);

            foreach (Humanoid a in bannerMen)
            {
                if (a.isRemoved())
                    continue;

                Div d = a.division();

                if (d == null)
                    continue;

                int x = a.body().cX() + offXs;
                int y = a.body().cY() + offYs;

                render(r, s, x, y, d, a.speed, STATS.RAN().get(a.indu(), 8, 16), bounds);
            }

            COLOR.unbind();
            bannerMen.clearSloppy();
        }
    }
}