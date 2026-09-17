using System;
using System.Collections.Generic;
using System.Linq;
using game.faction;
using init.constant;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.data;
using util.rendering;
using world;
using world.map.regions;
using world.map.regions.centre;
using world.overlay;

namespace world.overlay
{
    public class OverlayRegnames : Overlay
    {
        private readonly IntChecker check = new IntChecker(WREGIONS.MAX);
        private readonly Text text = new Text(UI.FONT().H1, RegionInfo.nameSize);

        public readonly BOOLEANImp active = new BOOLEANImp(true);

        public OverlayRegnames()
        {
            text.setMultipleLines(false);
        }

        public void exclude(Region r)
        {
            check.isSetAndSet(r.index());
        }

        public override void renderAbove(Renderer r, ShadowBatch s, RenderData data)
        {
            if (!active.is())
                return;

            foreach (var reg in WORLD.REGIONS().active())
            {
                if (!check.isSet(reg.index()))
                {
                    foreach (var d in DIR.NORTHO)
                    {
                        if (data.tBounds().holdsPoint(reg.cx() + d.x() * 5, reg.cy() + d.y() * 5))
                        {
                            render(reg, r, s, data);
                            break;
                        }
                    }
                }
            }
            check.init();
        }

        public void render(Region reg, Renderer r, ShadowBatch s, RenderData data)
        {
            if (r.getZoomout() >= 2)
            {
                text.setFont(UI.FONT().S);
                text.setScale(4.0);
            }
            else
            {
                text.setFont(UI.FONT().H1);
                text.setScale(1.0);
            }

            text.clear().add(reg.info.name());
            text.adjustWidth();
            text.setMaxWidth(C.T_PIXELS * 24);

            int cx = reg.info.cx() * C.TILE_SIZE + C.TILE_SIZE / 2;
            int cy = reg.info.cy() * C.TILE_SIZE + C.TILE_SIZE / 2;

            int dy = (int)(C.TILE_SIZE * (WCentre.TILE_DIM / 2.5 + (reg.capitol() ? 0.5 : 0)));

            int x1 = cx - text.width() / 2;
            int y1 = (int)(8 + cy + dy);

            if (canRender(reg, text, x1, y1))
            {
                render(reg, x1, y1, r, s, data);
            }
            else
            {
                int yy1 = (int)(cy - text.height() - dy);
                if (canRender(reg, text, x1, yy1))
                {
                    render(reg, x1, yy1, r, s, data);
                }
                else
                {
                    render(reg, x1, y1, r, s, data);
                }
            }

            COLOR.unbind();
        }

        private bool canRender(Region reg, DIMENSION dim, int x1, int y1)
        {
            int y2 = y1 + dim.height();
            int x2 = x1 + dim.width();
            if (is(reg, x1, y1) && is(reg, x2, y1) && is(reg, x2, y2) && is(reg, x1, y2))
            {
                return true;
            }
            return false;
        }

        private void render(Region reg, int x1, int y1, Renderer r, ShadowBatch s, RenderData data)
        {
            s.setHard().setDistance2Ground(0).setHeight(0);
            x1 = data.transformGX(x1);
            y1 = data.transformGY(y1);

            int ox = x1;
            int oy = y1;

            int M = 8;
            int H = text.height();

            if (reg.faction() == null)
            {
                COLOR.WHITE25.render(s, x1 - M, x1 + text.width() + M + H + M, y1 - M, y1 + H + M);
                COLOR.WHITE65.bind();
                renderText(r, x1, y1);
                COLOR.unbind();
                DIP.WAR().icon.render(r, x1 + text.width() + M, x1 + text.width() + M + H, y1, y1 + H);
            }
            else if (reg.faction() == FACTIONS.player())
            {
                COLOR.WHITE25.render(s, x1 - M, x1 + text.width() + M + H + M, y1 - M, y1 + H + M);
                if (reg.capitol())
                {
                    UI.icons().s.crown.render(r, x1, x1 + H, y1, y1 + H);
                    x1 += H + M;
                }
                reg.faction().banner().colorBGBright().bind();
                renderText(r, x1, y1);
                if (!reg.capitol())
                {
                    double loy = RD.RACES().loyaltyAll.getD(reg);

                    SPRITE sp = UI.icons().s.faces[(int)Math.Round(loy * (UI.icons().s.faces.Length - 1))];

                    GCOLOR.UI().badToGood(ColorImp.TMP, loy);
                    ColorImp.TMP.bind();
                    sp.render(r, x1 + text.width(), x1 + text.width() + H, y1, y1 + H);
                    COLOR.unbind();
                }
            }
            else
            {
                COLOR.WHITE25.render(s, x1 - M, x1 + text.width() + M + H + M + H, y1 - M, y1 + text.height() + M);
                if (reg.capitol())
                {
                    UI.icons().s.crown.render(r, x1, x1 + H, y1, y1 + H);
                }
                else
                {
                    FBanner.render(r, reg.faction(), x1, x1 + H, y1, y1 + H);
                }
                x1 += H + M;
                reg.faction().banner().colorBGBright().bind();
                renderText(r, x1, y1);

                x1 += text.width() + M;
                COLOR.unbind();
                DIP.get(FACTIONS.player(), reg.faction()).icon.render(r, x1, x1 + H, y1, y1 + H);
            }

            if (reg.besieged())
            {
                GCOLOR.MAP().SOSO.bind();
                UI.icons().s.degrade.render(r, ox - H / 2, ox - H / 2 + H, oy - H / 2, oy - H / 2 + H);
                COLOR.unbind();
            }
        }

        private void renderText(Renderer r, int x1, int y1)
        {
            text.render(r, x1, y1);
        }

        private bool is(Region reg, int x, int y)
        {
            x = x >> C.T_SCROLL;
            y = y >> C.T_SCROLL;
            return WORLD.REGIONS().map.is(x, y, reg);
        }

        public override bool renderBelow(Renderer r, ShadowBatch s, RenderData data)
        {
            // TODO Auto-generated method stub
            return false;
        }
    }
}