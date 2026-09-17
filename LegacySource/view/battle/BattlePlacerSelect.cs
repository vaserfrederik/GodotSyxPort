using System;
using System.Collections.Generic;
using static settlement.main.SETT;
using game;
using game.battle.div;
using init.constant;
using init.settings;
using init.sprite;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main;
using settlement.room.military.artillery;
using snake2d;
using snake2d.util.datatypes;
using util.colors;
using util.gui.misc;
using util.rendering;
using view.battle;
using view.main;
using view.subview;

namespace view.battle
{
    internal class BattlePlacerSelect : Mode
    {
        private readonly GameWindow w;
        private readonly DivSelection s;
        private readonly bool debug;
        private readonly Action a;

        public BattlePlacerSelect(GameWindow w, DivSelection s, Action action)
        {
            this.w = w;
            this.s = s;
            this.a = action;
            debug = S.get().developer;
        }

        private readonly Rec fill = new Rec();

        public override void update(bool hovered)
        {
            if (!hovered)
            {
                return;
            }

            if (a.clicked || a.clickReleased)
            {
                int x1 = (Math.Min(a.start.x(), w.pixel().x())) - C.TILE_SIZEH;
                int y1 = (Math.Min(a.start.y(), w.pixel().y())) - C.TILE_SIZEH;
                int x2 = (Math.Max(a.start.x(), w.pixel().x())) + C.TILE_SIZEH;
                int y2 = (Math.Max(a.start.y(), w.pixel().y())) + C.TILE_SIZEH;
                fill.set(x1, x2, y1, y2);
                x1 = x1 >> C.T_SCROLL;
                y1 = y1 >> C.T_SCROLL;
                x2 = x2 >> C.T_SCROLL;
                y2 = y2 >> C.T_SCROLL;
                bool include = false;
                foreach (ENTITY e in ENTITIES().fill(fill))
                {
                    if (e is Humanoid)
                    {
                        Div d = ((Humanoid)e).division();

                        if (d != null)
                            if (debug || d.army() == GAME.ARMIES().player())
                            {
                                s.hover(d);
                                include |= !s.selected(d);
                            }
                    }
                }
                for (int y = y1; y <= y2; y++)
                {
                    for (int x = x1; x <= x2; x++)
                    {

                        Room r = SETT.ROOMS().map.get(x, y);
                        if (r != null && r is ArtilleryInstance)
                        {
                            ArtilleryInstance ca = (ArtilleryInstance)r;

                            if (ca.army() == GAME.ARMIES().player())
                            {
                                ca.hovered = true;
                                include |= !ca.selected;
                            }
                        }
                    }

                }

                if (a.clickReleased)
                {
                    foreach (ENTITY e in ENTITIES().fill(fill))
                    {
                        if (e is Humanoid)
                        {
                            Div d = ((Humanoid)e).division();

                            if (d != null)
                                if (debug || d.army() == GAME.ARMIES().player())
                                {
                                    if (include)
                                        s.select(d);
                                    else
                                        s.deSelect(d);
                                }
                        }
                    }

                    for (int y = y1; y <= y2; y++)
                    {
                        for (int x = x1; x <= x2; x++)
                        {
                            foreach (ENTITY e in ENTITIES().getAtTile(x, y))
                            {
                                if (e is Humanoid)
                                {
                                    Div d = ((Humanoid)e).division();

                                    if (d != null)
                                        if (debug || d.army() == GAME.ARMIES().player())
                                        {
                                            if (include)
                                                s.select(d);
                                            else
                                                s.deSelect(d);
                                        }
                                }

                            }
                            Room r = SETT.ROOMS().map.get(x, y);
                            if (r != null && r is ArtilleryInstance)
                            {
                                ArtilleryInstance ca = (ArtilleryInstance)r;
                                if (ca.army() == GAME.ARMIES().player())
                                {
                                    if (include)
                                        s.artillery.select(ca);
                                    else
                                        s.artillery.deSelect(ca);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ENTITY e = ENTITIES().getArroundPoint(w.pixel().x(), w.pixel().y());
                if (e is Humanoid)
                {
                    Div d = ((Humanoid)e).division();
                    if (d != null)
                    {
                        s.hover(d);
                        return;
                    }
                }
                Room r = SETT.ROOMS().map.get(w.tile());
                if (r != null && r is ArtilleryInstance)
                {
                    ArtilleryInstance ca = (ArtilleryInstance)r;
                    ca.hovered = true;

                }

                return;
            }
        }

        public override void render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
        {
            VIEW.mouse().setReplacement(SPRITES.icons().m.expand);
            if (!a.clicked)
            {
                return;
            }
            int x1 = Math.Min(a.start.x(), w.pixel().x());
            int y1 = Math.Min(a.start.y(), w.pixel().y());
            int x2 = Math.Max(a.start.x(), w.pixel().x());
            int y2 = Math.Max(a.start.y(), w.pixel().y());

            final int dim = (2) << w.zoomout();
            final int mX = C.WIDTH() << w.zoomout();
            final int mY = C.HEIGHT() << w.zoomout();

            if (x2 - x1 < dim)
                return;
            if (y2 - y1 < dim)
                return;

            //select

            //render

            x1 -= data.offX1();
            x2 -= data.offX1();
            y1 -= data.offY1();
            y2 -= data.offY1();

            //left
            if (x1 + dim > 0)
            {
                int ry1 = Math.Max(y1, 0);
                int ry2 = Math.Min(y2, mY);
                GCOLOR.MAP().BATTLE_OK.render(r, x1, x1 + dim, ry1, ry2);
            }
            //right
            if (x2 < mX)
            {
                int ry1 = Math.Max(y1, 0);
                int ry2 = Math.Min(y2, mY);
                GCOLOR.MAP().BATTLE_OK.render(r, x2, x2 + dim, ry1, ry2);
            }

            //up
            if (y1 + dim > 0)
            {
                int rx1 = Math.Max(x1, 0);
                int rx2 = Math.Min(x2, mX);
                GCOLOR.MAP().BATTLE_OK.render(r, rx1, rx2, y1, y1 + dim);
            }
            //down
            if (y2 < mY)
            {
                int rx1 = Math.Max(x1, 0);
                int rx2 = Math.Min(x2, mX);
                GCOLOR.MAP().BATTLE_OK.render(r, rx1, rx2, y2, y2 + dim);
            }
        }

        public void renderCurrent(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
        {
        }

        private readonly string sSelect = "Click to select. Click and hold to select area";

        double hovDelay = 0;

        public override void hoverTimer(GBox text)
        {
            if (!a.clicked)
            {
                ENTITY e = ENTITIES().getArroundPoint(w.pixel().x(), w.pixel().y());
                if (e is Humanoid)
                {
                    Div d = ((Humanoid)e).division();
                    if (d != null)
                    {
                        s.hover(d);
                        if (GAME.SPEED.isPaused() || VIEW.renderSecond() - hovDelay > 1.5)
                        {
                            d.hoverInfo(text);
                            if (d.army() == GAME.ARMIES().player())
                            {
                                text.NL(5);
                                text.text(sSelect);

                            }

                        }
                        return;
                    }
                }
                hovDelay = VIEW.renderSecond();

                Room r = SETT.ROOMS().map.get(w.tile());
                if (r != null && r is ArtilleryInstance)
                {
                    ((ArtilleryInstance)r).hover(text);
                }


            }
            else
            {
                hovDelay = VIEW.renderSecond();
            }
        }
    }
}