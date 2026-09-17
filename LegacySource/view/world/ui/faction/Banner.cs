using System;
using game.faction.diplomacy;
using game.faction.npc;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.text;
using world.region;

namespace view.world.ui.faction
{
    sealed class Banner : GuiSection
    {
        public Banner(GETTER<FactionNPC> f, int width)
        {
            body().setWidth(700);

            addRelBody(2, DIR.S, new GStat(new GText(UI.FONT().M, 32))
            {
                public override void update(GText text)
                {
                    text.add(f.get().nameIntro);
                }
            }.r(DIR.N));

            addRelBody(6, DIR.S, new GStat(new GText(UI.FONT().H1, 32))
            {
                public override void update(GText text)
                {
                    text.add(f.get().name);
                    text.lablify();
                }
            }.r(DIR.N));

            addRelBody(4, DIR.S, new GStat()
            {
                public override void update(GText text)
                {
                    if (!RD.DIST().reachable(f.get()))
                    {
                        if (RD.DIST().factionCanAttackPlayerAllies(f.get()))
                            text.add(Dic.¤¤FactionBorder);
                        else
                            text.add(Dic.¤¤Distant);
                    }
                    else
                        text.add(DIP.get(f.get()).name);
                }

                public override void hoverInfoGet(GBox b)
                {
                    DIP.get(f.get()).hover(b);
                }
            }.r(DIR.N));

            RENDEROBJ rr = ban(f);
            rr.body().moveX1(0).moveCY(body().cY());
            add(rr);

            rr = ban(f);
            rr.body().moveX2(700).moveCY(body().cY());
            add(rr);

            RENDEROBJ info = Hoverer.facts(f, 1000, 90);
            addRelBody(4, DIR.S, info);
        }

        private RENDEROBJ ban(GETTER<FactionNPC> f)
        {
            return new RENDEROBJ.RenderImp(Icon.L * 2)
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    f.get().banner().HUGE.render(r, body());
                }
            };
        }
    }
}