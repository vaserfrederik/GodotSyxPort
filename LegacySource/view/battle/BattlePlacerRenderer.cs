using System;
using System.Collections.Generic;
using settlement.main;
using game.battle.div;
using init.constant;
using init.sprite;
using settlement.main;
using settlement.room.military.artillery;
using settlement.thing.projectiles;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.colors;
using util.rendering;
using view.main;

namespace view.battle
{
    sealed class BattlePlacerRenderer : ON_TOP_RENDERABLE
    {
        private readonly BattlePlacer b;
        private readonly double[] xs = new double[Config.battle().DIVISIONS_PER_ARMY];
        private readonly double[] ys = new double[Config.battle().DIVISIONS_PER_ARMY];
        private readonly double[] ranges = new double[Config.battle().DIVISIONS_PER_ARMY];
        private int ri;
        private bool hovered;

        public BattlePlacerRenderer(BattlePlacer b)
        {
            this.b = b;
        }

        void add(bool hovered)
        {
            this.hovered = hovered;
            this.add();
        }

        public override void render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
        {
            remove();

            //deployment bounds
            if (VIEW.b().state() != null && VIEW.b().state().deploying())
            {
                RenderIterator it = data.onScreenTiles();
                GCOLOR.MAP().OK.bind();
                while (it.has())
                {
                    if (VIEW.b().state().deploymentBounds().isOnEdge(it.tx(), it.ty()))
                    {
                        int m = 0;

                        foreach (DIR d in DIR.ORTHO)
                            if (VIEW.b().state().deploymentBounds().holdsPoint(it.tx(), it.ty(), d))
                            {
                                m |= d.mask();
                            }

                        if (m != 0x0F)
                        {
                            SPRITES.cons().BIG.outline.render(r, m, it.x(), it.y());
                        }
                    }

                    it.next();

                }
                COLOR.unbind();
            }

            //trajectory
            {
                ri = 0;
                foreach (Div d in b.s.selection())
                {
                    if (ri >= ranges.Length)
                        break;
                    if (d.settings().ammo() != null)
                    {
                        xs[ri] = d.centre().cUnitX() >> C.T_SCROLL;
                        ys[ri] = d.centre().cUnitY() >> C.T_SCROLL;
                        double refValue = d.settings().ammo().ref(d);
                        ranges[ri] = Trajectory.range(TERRAIN().get((int)xs[ri], (int)ys[ri]).heightEnd((int)xs[ri], (int)ys[ri]), d.settings().ammo().projectile.maxAngle(refValue), d.settings().ammo().projectile.velocity(refValue));
                        ranges[ri] = (int)ranges[ri] / C.TILE_SIZE;
                        //ranges[ri] *= ranges[ri];

                        ri++;
                    }
                }

                if (ri > 0)
                {
                    GCOLOR.MAP().OK.bind();
                    RenderIterator it = data.onScreenTiles();
                    while (it.has())
                    {
                        renderRange(it, r);

                        it.next();

                    }
                    COLOR.unbind();
                }
            }

            renderArtillery(r, shadowBatch, data);

            if (hovered)
                b.current.render(r, shadowBatch, data, ds);
        }

        private void renderRange(RenderIterator it, Renderer r)
        {
            for (int i = 0; i < ri; i++)
            {
                double x = Math.Abs(xs[i] - it.tx());
                double y = Math.Abs(ys[i] - it.ty());
                int l = (int)Math.Sqrt(x * x + y * y);
                if (l == ranges[i])
                {
                    SPRITES.cons().BIG.dots.render(r, 0, it.x(), it.y());
                }
            }
        }

        private void renderArtillery(Renderer r, ShadowBatch shadowBatch, RenderData data)
        {
            foreach (ArtilleryInstance ins in b.s.artillery.selection())
            {
                COORDINATE c = ins.centre();
                int sx = c.x();
                int sy = c.y();
                int rangeMin = ins.rangeMin();
                int rangeMax = ins.rangeMax();

                for (int di = -1; di <= 1; di++)
                {
                    DIR d = ins.dir().next(di);
                    bool inRange = false;
                    int min = (int)(rangeMin / d.tileDistance());
                    int max = (int)(rangeMax / d.tileDistance());
                    for (int i = min; i <= max; i += C.TILE_SIZE)
                    {
                        int tx = (sx + i * d.x()) >> C.T_SCROLL;
                        int ty = (sy + i * d.y()) >> C.T_SCROLL;
                        if (data.tBounds().holdsPoint(tx, ty))
                        {
                            inRange = true;
                            SPRITES.cons().ICO.arrows2.get(d.id()).render(r, tx * C.TILE_SIZE - data.offX1(), ty * C.TILE_SIZE - data.offY1());
                        }
                        else if (inRange)
                            break;
                    }
                }

            }
        }
    }
}