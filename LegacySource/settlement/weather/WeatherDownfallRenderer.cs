using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.time;
using init.constant;
using init.settings;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.rnd;
using util.rendering;

namespace settlement.weather
{
    internal class WeatherDownfallRenderer
    {
        private static int amount = 256;
        private static int squareSize = 512;
        private final static int speedMax = 600;

        private double speedAcc = RND.rFloat() * 10000;
        private final Drops drops = new Drops();

        private final Off[,] offs = new Off[16, 16];

        private final SkyLayer[] layers = new SkyLayer[] { new SkyLayer(), new SkyLayer(), new SkyLayer(), new SkyLayer() };

        public WeatherDownfallRenderer()
        {
            for (int i = 0; i < offs.GetLength(0); i++)
                for (int k = 0; k < offs.GetLength(1); k++)
                    offs[i, k] = new Off();
        }

        public void render(Renderer r, float ds, RenderData data, int zoomout)
        {
            double rain = SETT.WEATHER().rain.getD();

            if (rain <= 0)
                return;

            COLOR[] cs = drops.colors;
            final boolean snow = SETT.WEATHER().snow.rainIsSnow();
            if (snow)
            {
                cs = drops.colorsSnow;
                speedAcc += ds * (1 + speedMax * SETT.WEATHER().wind.getD());
            }
            else
            {
                speedAcc += 1.5 * ds * (speedMax / 2 + speedMax * SETT.WEATHER().wind.getD());
            }
            if (speedAcc < 0)
                speedAcc = 0;

            rain *= rain;

            int am = (int)Math.Ceiling(32 * rain);
            int smask = offs.GetLength(0) - 1;
            int amask = amount - 1;
            int cmask = (amount >> 1) - 1;
            int zs = 1;
            int ze = 2;
            if (S.get().graphics.get() == 0)
            {
                ze = 1;
            }

            if (am < 0)
                return;

            int snowI = (int)(TIME.currentSecond() * amount / 4.0);

            for (int z = zs; z <= ze; z++)
            {
                r.newLayer(false, z);

                ZoomLayer.init(data, zoomout, z);
                TIME.light().apply(ZoomLayer.absX1, ZoomLayer.absX2, ZoomLayer.absY1, ZoomLayer.absY2, RGB.WHITE);

                for (int li = 0; li < layers.Length; li++)
                {
                    layers[li].init(data, zoomout, z, 1.1 + 0.15 * li);
                }

                for (int sy = 0; sy < layers[0].sqIH; sy++)
                {
                    for (int sx = 0; sx < layers[0].sqIW; sx++)
                    {
                        int ranI = 0;
                        foreach (SkyLayer l in layers)
                        {
                            int startX = l.sqStartX + sx * squareSize;
                            int startY = l.sqStartY + sy * squareSize;
                            Off off = offs[(l.sqIX + sx) & smask, (l.sqIY + sy) & smask];
                            for (int i = 0; i < am; i++)
                            {
                                int ra = (i + ranI + off.offR) & amask;
                                int px = (int)(drops.dx[ra] * speedAcc) + off.offX;
                                int py = (int)(drops.dy[ra] * speedAcc) + off.offY;
                                px &= squareSize - 1;
                                px = -px;
                                py &= squareSize - 1;
                                px += drops.sx[ra];
                                py += drops.sy[ra];

                                px += startX;
                                py += startY;

                                if (snow)
                                {
                                    px += drops.snowX[(drops.snowI[ra] + snowI) & amask];
                                    py += drops.snowY[(drops.snowI[(ra + 1) & amask] + snowI) & amask];
                                }

                                if (px < ZoomLayer.absX1 || py < ZoomLayer.absY1 || px >= ZoomLayer.absX2 || py > ZoomLayer.absY2)
                                    continue;
                                int tx = (ZoomLayer.gx1 + ((px << zoomout) >> z)) >> C.T_SCROLL;
                                int ty = (ZoomLayer.gy1 + ((py << zoomout) >> z)) >> C.T_SCROLL;
                                if (SETT.TERRAIN().get(tx, ty).roofIs() || SETT.TERRAIN().get(tx, ty).coversCompletely(tx, ty))
                                    continue;
                                cs[ra & cmask].bind();
                                CORE.renderer().renderParticle(px, py);
                                if (!snow)
                                {
                                    CORE.renderer().renderParticle(px + CORE.renderer().pointsize(), py - CORE.renderer().pointsize());
                                }
                            }
                            ranI += 64;
                        }
                    }
                }
            }

            COLOR.unbind();
        }

        private static class ZoomLayer
        {
            private static int absX1, absY1, absX2, absY2;
            private static int gx1, gy1;

            public static void init(RenderData data, int zoomout, int z)
            {
                absX1 = (data.absBounds().x1() >> zoomout) << z;
                absY1 = (data.absBounds().y1() >> zoomout) << z;
                absX2 = (data.absBounds().x2() >> zoomout) << z;
                absY2 = (data.absBounds().y2() >> zoomout) << z;

                gx1 = data.gBounds().x1() - data.absBounds().x1();
                gy1 = data.gBounds().y1() - data.absBounds().y1();

                if (data.gBounds().x1() < 0)
                {
                    absX1 += -((data.gBounds().x1() >> zoomout) << z);
                }
                if (data.gBounds().y1() < 0)
                {
                    absY1 += -((data.gBounds().y1() >> zoomout) << z);
                }
                if (data.gBounds().x2() > SETT.PWIDTH)
                {
                    absX2 -= (((data.gBounds().x2() - SETT.PWIDTH) >> zoomout) << z);
                }
                if (data.gBounds().y2() > SETT.PHEIGHT)
                {
                    absY2 -= (((data.gBounds().y2() - SETT.PHEIGHT) >> zoomout) << z);
                }
            }
        }

        private static class SkyLayer
        {
            private int sqStartX, sqStartY;
            private int sqIX, sqIY;
            private int sqIW, sqIH;

            public void init(RenderData data, int zoomout, int z, double skyzoom)
            {
                int skyX = (int)((data.gBounds().x1() + SETT.PWIDTH / 2) * skyzoom);
                int skyY = (int)((data.gBounds().y1() + SETT.PHEIGHT / 2) * skyzoom);

                sqIX = skyX / squareSize;
                sqIY = skyY / squareSize;
                sqStartX = skyX % squareSize;
                sqStartY = skyY % squareSize;
                sqStartX = data.absBounds().x1() + (sqIX * squareSize);
                sqStartY = data.absBounds().y1() + (sqIY * squareSize);

                sqIW = data.gBounds().width() / squareSize;
                sqIH = data.gBounds().height() / squareSize;
            }
        }

        internal class Off
        {
            public int offR;
            public int offX;
            public int offY;

            public Off()
            {
                offR = RND.rInt(360);
                offX = RND.rInt(squareSize);
                offY = RND.rInt(squareSize);
            }
        }

        internal class Drops
        {
            public COLOR[] colors = new COLOR[amount / 2];
            public COLOR[] colorsSnow = new COLOR[amount / 2];
            public double[] dx = new double[amount];
            public double[] dy = new double[amount];
            public short[] sx = new short[amount];
            public short[] sy = new short[amount];
            public byte[] snowI = new byte[amount];
            public byte[] snowX = new byte[amount];
            public byte[] snowY = new byte[amount];

            public Drops()
            {
                for (int i = 0; i < amount; i++)
                {
                    dx[i] = 0.5 + RND.rFloat() * 0.5;
                    dy[i] = 0.5 + RND.rFloat() * 0.5;
                    sx[i] = (short)RND.rShort(squareSize);
                    sy[i] = (short)RND.rShort(squareSize);
                }

                for (int i = 0; i < colors.Length; i++)
                {
                    int rg = 40 + RND.rInt(35);
                    rg += 10;
                    colors[i] = new ColorImp(rg, rg, rg + 20 + RND.rInt(30));
                }

                for (int i = 0; i < colorsSnow.Length; i++)
                {
                    int rg = 100 + RND.rInt(28);
                    colorsSnow[i] = new ColorImp(rg, rg, rg);
                }

                for (int i = 0; i < amount; i++)
                {
                    double d = i;
                    d /= amount;
                    d *= Math.PI * 2;
                    snowI[i] = (byte)RND.rInt();
                    snowX[i] = (byte)(64 * Math.Cos(d));
                    snowY[i] = (byte)(64 * Math.Cos(d + Math.PI));
                }
            }
        }
    }
}