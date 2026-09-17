using System;
using System.Collections.Generic;
using game.faction;
using game.faction.player;
using init.race;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;

class StagePickTitles : GuiSection
{
    static readonly string ¤¤title = "¤Select Titles";
    static readonly string ¤¤spent = "¤Pick 5 unlocked titles to boost your name.";
    static readonly string ¤¤YouSure = "¤You may still pick some unlocked titles. Start anyway?";

    static
    {
        D.ts(typeof(StagePickTitles));
    }

    StagePickTitles(WorldViewGenerator stage)
    {
        addRelBody(4, DIR.S, new GText(UI.FONT().M, ¤¤spent));

        addRelBody(8, DIR.S, new GStat(UI.FONT().M)
        {
            public override void update(GText text)
            {
                GFORMAT.iofkInv(text, FACTIONS.player().titles.selected(), 5);
            }
        }.r(DIR.N));

        List<RENDEROBJ> rows = new List<RENDEROBJ>(FACTIONS.player().titles.all().Count);

        foreach (PTitle t in FACTIONS.player().titles.all())
        {
            rows.Add(new Butt(t));
        }

        addRelBody(8, DIR.S, new GScrollRows(rows, rows[0].body().height() * 6).view());

        addRelBody(16, DIR.S, new GButt.ButtPanel(Dic.¤¤confirm)
        {
            protected override void clickA()
            {
                ACTION no = new ACTION()
                {
                    public override void exe()
                    {
                        // TODO Auto-generated method stub
                    }
                };
                ACTION next = new ACTION()
                {
                    public override void exe()
                    {
                        stage.hasSelectedTitles = true;
                        stage.set();
                    }
                };

                if (FACTIONS.player().titles.selected() < 5 && FACTIONS.player().titles.unlocked() > FACTIONS.player().titles.selected())
                {
                    VIEW.inters().yesNo.activate(¤¤YouSure, next, no, true);
                }
                else
                {
                    next.exe();
                }
            }
        });

        stage.dummy.add(this, ¤¤title);
    }

    private class Butt : GButt.ButtPanel
    {
        private readonly PTitle title;

        Butt(PTitle title) : base(title.name)
        {
            icon(title.icon.scaled(2));
            body.setDim(600, FACTIONS.player().titles.all()[0].icon.height() * 2 + 8);
            this.title = title;
        }

        protected override void renAction()
        {
            activeSet(title.unlocked());
            selectedSet(title.selected());
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            GBox b = (GBox)text;
            if (!title.unlocked())
            {
                b.error(Dic.¤¤Unavailable);
            }
            else
            {
                b.title(title.name);
                b.text(title.desc);
                b.sep();

                b.textLL(PTitles.¤¤racesUnlocked);
                b.NL();
                foreach (Race r in RACES.playable())
                {
                    if (title.race(r))
                        b.add(r.appearance().iconBig);
                }
                b.NL();
                GText t = b.text();
                t.add(PTitles.¤¤racesUnlockedD);
                t.insert(0, 50.0 / RACES.playable().Count, 1);
                b.text(t);
                b.NL(4);

                b.textLL(PTitles.¤¤currentBoost);
                b.NL();
                title.boosters.hover(text, title.boosterValue(), null, -1);

                b.NL();
            }
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
        {
            base.render(r, ds, isActive, isSelected, isHovered);

            if (title.unlocked())
            {
                int x1 = body.x1() + 120;
                int y1 = body.y2() - 25;
                foreach (Race ra in RACES.playable())
                {
                    if (title.race(ra))
                    {
                        ra.appearance().icon.render(r, x1, y1);
                        x1 += 26;
                    }
                }
            }
        }

        protected override void clickA()
        {
            if (title.selected())
                title.select(!title.selected());
            else if (!title.unlocked())
                ;
            else if (FACTIONS.player().titles.selected() >= 5)
                ;
            else
            {
                title.select(!title.selected());
            }
        }
    }
}