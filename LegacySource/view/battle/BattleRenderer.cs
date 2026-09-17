using game;
using game.battle.div;
using game.battle.formation;
using game.battle.thread.order;
using init.constant;
using init.settings;
using init.sprite;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.military.artillery;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.light;
using util.rendering;
using view.keyboard;
using view.main;

namespace view.battle
{
    public class BattleRenderer : ON_TOP_RENDERABLE
    {
        private readonly DivSelection s;
        private readonly COLOR cHover = new ColorImp(0, 127, 0);
        private readonly COLOR cHoverEnemy = new ColorImp(127, 0, 0);
        private readonly BattleOrderTask task = new BattleOrderTask();

        public static ColorImp colAttack = new ColorImp(127, 40, 40);

        //private readonly DivStatus status = new DivStatus();

        public BattleRenderer(DivSelection s)
        {
            this.s = s;
        }

        public override void render(ShadowBatch shadowBatch, RenderData data, int zoomout, double ds)
        {
            CORE.renderer().newLayer(false, 0);
            AmbientLight.full.register(0, C.WIDTH(), 0, C.HEIGHT());
            remove();
            Renderer r = CORE.renderer();

            if (r.getZoomout() < 4)
            {
                ENTITY[] es = SETT.ENTITIES().getAllEnts();
                int m = SETT.ENTITIES().Imax();
                for (int ei = 0; ei < m; ei++)
                {
                    ENTITY e = es[ei];
                    if (e == null || !data.gBounds().holdsPoint(e.body().cX(), e.body().cY()))
                        continue;
                    if (e is Humanoid)
                    {
                        Div d = ((Humanoid)e).division();
                        if (d != null && (s.hovered(d) || s.selected(d)))
                        {
                            if (d.army() == GAME.ARMIES().enemy())
                                cHoverEnemy.bind();
                            else
                                cHover.bind();
                            int rx = (data.absBounds().x1() + (e.body().cX() - data.gBounds().x1()) >> zoomout);
                            int ry = (data.absBounds().y1() + (e.body().cY() - data.gBounds().y1()) >> zoomout);
                            DIR dir = d.position().dir(d.reporter.positionSpot((Humanoid)e));
                            if (dir == null)
                                dir = e.speed.dir();

                            if (zoomout == 3)
                            {
                                SPRITE s = SPRITES.cons().TINY.dots.get(0);
                                int dd = 8;
                                s.render(r, rx - dd / 2, rx + dd / 2, ry - dd / 2, ry + dd / 2);
                            }
                            else
                            {
                                SPRITE s = SPRITES.cons().ICO.arrows2.get(dir.id());

                                int dd = s.width() >> Math.Min(zoomout + 1, 2);
                                int x1 = rx - dd / 2;
                                int y1 = ry - dd / 2;
                                s.render(r, x1, x1 + dd, y1, y1 + dd);
                            }
                        }
                    }
                }
            }

            if (KEYS.BATTLE().SHOW_DIVISIONS.isPressed())
            {
                for (int di = 0; di < GAME.ARMIES().divisions().size(); di++)
                {
                    Div d = GAME.ARMIES().divisions().getC(di);

                    if (s.hovered(d) || s.selected(d))
                        continue;

                    int x = d.centre().cX();
                    int y = d.centre().cY();

                    if (data.gBounds().holdsPoint(x, y))
                    {
                        x = (data.absBounds().x1() + (x - data.gBounds().x1()) >> zoomout);
                        y = (data.absBounds().y1() + (y - data.gBounds().y1()) >> zoomout);
                        SPRITE icon = VIEW.UI().div.battle.miniDiv(d, false, false);
                        icon.renderC(r, x, y);
                    }
                }

                for (int di = 0; di < GAME.ARMIES().divisions().size(); di++)
                {

                    Div d = GAME.ARMIES().divisions().getC(di);
                    if (s.hovered(d) || s.selected(d))
                    {
                        SPRITE icon = VIEW.UI().div.battle.miniDiv(d, false, false);
                        icon.renderC(r, x, y);
                    }
                }
            }

            if (d != null && d.reporter.body().touches(data.gBounds()))
            {
                int x = d.centre().cX();
                int y = d.centre().cY();
                if (data.gBounds().holdsPoint(x, y))
                {
                    x = (data.absBounds().x1() + (x - data.gBounds().x1()) >> zoomout);
                    y = (data.absBounds().y1() + (y - data.gBounds().y1()) >> zoomout);
                    colAttack.bind();
                    UI.icons().l.crossheir.renderCScaled(r, x, y, 2);
                }
            }
            COORDINATE coo = ins.targetCooGet();
            if (coo != null)
            {
                int x = coo.x();
                int y = coo.y();
                if (data.gBounds().holdsPoint(x, y))
                {
                    x = (data.absBounds().x1() + (x - data.gBounds().x1()) >> zoomout);
                    y = (data.absBounds().y1() + (y - data.gBounds().y1()) >> zoomout);
                    colAttack.bind();
                    UI.icons().l.crossheir.renderCScaled(r, x, y, 2);
                }
            }
        }

        public override void render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
        {

        }

        private readonly DivFormationImp tmp = new DivFormationImp();
        private readonly BattleOrderPath pathDiv = new BattleOrderPath();
        private static FormationBody body = new FormationBody();

        public void renderBelow(SPRITE_RENDERER ren, RenderData data)
        {
            //OPACITY.O50.bind();

            SPRITE s = SPRITES.icons().s.circle;
            foreach (Div d in GAME.ARMIES().divisions())
            {
                COLOR.GREEN40.bind();
                if ((d.army() == GAME.ARMIES().player() || S.get().developer) && (this.s.hovered(d) || this.s.selected(d) || KEYS.BATTLE().SHOW_DIVISIONS.isPressed()))
                {

                    if (S.get().developer)
                        DivRenderer.render(ren, d.position(), data);

                    if (body.init(d.current()))
                    {

                        if (body.width() > d.position().width() * 2 || body.height() > d.position().width() * 2)
                            body.init(d.position());

                        int x1 = body.x1() - data.offX1();
                        int y1 = body.y1() - data.offY1();
                        int y2 = body.y2() - data.offY1();
                        int x2 = body.x2() - data.offX1();

                        (d.player() ? cHover : cHoverEnemy).renderFrame(ren, x1, x2, y1, y2, 1, 8);
                    }

                    if (this.s.hovered(d))
                    {
                        COLOR.WHITE100.bind();
                    }
                    else
                        COLOR.WHITE50.bind();
                    d.order().task.get(task);
                    if (task.task().showDest || S.get().developer)
                    {
                        d.order().dest.get(tmp);
                        DivRenderer.render(ren, tmp, data);
                    }
                    if (task.task().showPath || S.get().developer)
                    {
                        d.order().path.get(pathDiv);
                        if (pathDiv.length() > 0)
                        {
                            COLOR.ORANGE100.bind();
                            int curr = pathDiv.currentI();
                            int k = curr > 0 ? curr - 1 : curr;
                            for (int i = k; i < pathDiv.length(); i++)
                            {
                                pathDiv.setCurrentI(i);
                                int rx = pathDiv.x() - s.width() / 2;
                                int ry = pathDiv.y() - s.width() / 2;
                                rx -= data.offX1();
                                ry -= data.offY1();

                                s.renderScaled(ren, rx, ry, C.SCALE);
                            }
                            pathDiv.setCurrentI(curr);

                        }
                    }

                    COLOR.unbind();
                }
            }
            COLOR.unbind();
        }
    }
}