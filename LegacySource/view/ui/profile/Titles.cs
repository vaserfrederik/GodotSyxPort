using System;
using System.Collections.Generic;
using snake2d;
using util.colors;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using game;
using game.faction;
using game.faction.player;
using init.race;
using init.settings;
using init.sprite.UI;
using util.gui;

namespace view.ui.profile
{
    final class Titles : GuiSection
    {
        private static readonly CharSequence ¤¤spent = "¤Chosen";
        private static readonly CharSequence ¤¤NotAchiving = "¤Titles can not be unlocked.";
        private static readonly CharSequence ¤¤spentD = "¤Whenever you start a new game, you get to choose 5 unlocked titles.";

        private static readonly CharSequence ¤¤Locked = "¤Title is currently unavailable.";
        private static readonly CharSequence ¤¤Active = "¤Title is currently unlocked and active.";
        private static readonly CharSequence ¤¤Unlocked = "¤Title is currently unlocked. You can select it when starting a new game.";

        static Titles()
        {
            D.ts(typeof(Titles));
        }

        Titles(int HEIGHT)
        {
            AddRelBody(8, DIR.S, new GStat()
            {
                public void update(GText text)
                {
                    GFORMAT.iofk(text, FACTIONS.player().titles.selected(), 5);
                }

                public void hoverInfoGet(GBox b)
                {
                    b.text(¤¤spentD);
                }
            }.hh(¤¤spent));

            if (!GAME.achieving())
            {
                AddRelBody(4, DIR.S, new GStat()
                {
                    public void update(GText text)
                    {
                        if (!GAME.achieving())
                            text.errorify().add(¤¤NotAchiving);
                    }
                });
            }

            StringInputSprite filter = new StringInputSprite(18, UI.FONT().M).placeHolder(Dic.¤¤Search);

            AddRelBody(8, DIR.S, new GInput(filter));

            List<RENDEROBJ> rows = new List<RENDEROBJ>(FACTIONS.player().titles.all().Count);

            foreach (PTitle t in FACTIONS.player().titles.all())
            {
                rows.Add(new Butt(t));
            }

            int hi = HEIGHT - body().height() - 16;
            hi = rows[0].body().height() * (hi / rows[0].body().height());

            AddRelBody(8, DIR.S, new GScrollRows(rows, hi)
            {
                protected override bool passesFilter(int i, RENDEROBJ o)
                {
                    if (filter.text().Length > 0)
                        return Str.containsText(((Butt)o).title.name, filter.text());
                    return true;
                }
            }.view());
        }

        private class Butt : HOVERABLE.HoverableAbs
        {
            private readonly PTitle title;
            private readonly GuiSection sec = new GuiSection();

            private GText t = new GText(UI.FONT().H2, 24);

            Butt(PTitle title)
            {
                body.setDim(800, title.icon.height() * 2 + 16);
                this.title = title;
                int wi = 800 - 16 - title.icon.width() * 2 - 8;
                t.setMaxWidth(wi);
                t.set(title.name);

                sec.add(t, 0, 0);
                sec.addDown(4, new RENDEROBJ.RenderImp(wi, 12)
                {
                    public void render(SPRITE_RENDERER r, float ds)
                    {
                        GMeterCol cc = GMeter.C_ORANGE;
                        if (title.selected())
                            cc = GMeter.C_BLUE;
                        else if (title.unlocked())
                            cc = GMeter.C_GREEN;
                        double d = 0;
                        if (title.unlocked() || title.race(FACTIONS.player().race()))
                            d = 1;
                        else
                            d = title.lockable.progress(FACTIONS.player());
                        GMeter.render(r, cc, d, body());
                    }
                });

                sec.addRelBody(8, DIR.W, new RENDEROBJ.RenderImp(title.icon.width() * 2, title.icon.height() * 2)
                {
                    public void render(SPRITE_RENDERER r, float ds)
                    {
                        if (!title.selected() && !(title.unlocked() && title.race(FACTIONS.player().race())))
                            GCOLOR.T().INACTIVE.bind();
                        title.icon.render(r, body);
                        COLOR.unbind();

                        int x1 = Butt.this.body.x2() - 40;
                        int y1 = Butt.this.body.y1() + 16;

                        foreach (Race ra in RACES.playable())
                        {
                            if (title.race(ra))
                            {
                                ra.appearance().icon.render(r, x1, y1);
                                x1 -= 38;
                            }
                        }
                    }
                });
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                GButt.ButtPanel.renderBG(r, title.unlocked(), title.isNew(), title.selected(), body);
                GButt.ButtPanel.renderFrame(r, body);

                sec.body().centerIn(body);
                sec.render(r, ds);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                title.consumeNew();
                GBox b = (GBox)text;
                b.title(title.name);
                b.text(title.desc);
                b.NL(6);

                if (title.selected())
                {
                    b.text(¤¤Active);
                }
                else if (title.unlocked())
                {
                    b.text(¤¤Unlocked);
                }
                else
                {
                    b.text(¤¤Locked);
                }
                b.NL(6);
                title.lockable.hover(text, FACTIONS.player());

                b.sep();

                title.lockers.hover(text);
                b.NL(8);

                title.boosters.hover(text, Math.Max(0.5, title.boosterValue()), -1);

                if (S.get().developer && MButt.WHEEL.consumeClick())
                {
                    FACTIONS.player().titles.unlock(title);
                }
            }
        }
    }
}