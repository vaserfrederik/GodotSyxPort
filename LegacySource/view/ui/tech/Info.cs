using System;
using System.Collections.Generic;
using System.Linq;
using game.faction.FACTIONS;
using game.faction.player.PTech;
using init.settings.S;
using settlement.main.SETT;
using settlement.room.infra.admin.AdminData;
using settlement.room.main.RoomBlueprint;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.renderable.RENDEROBJ;
using snake2d.util.sets;
using util.colors.GCOLOR;
using util.gui.misc.GBox;
using util.gui.misc.GButt;
using util.gui.misc.GHeader;
using util.gui.misc.GStat;
using util.gui.misc.GText;
using util.gui.table.GStaples;
using util.info.GFORMAT;
using util.text.DicTime;
using view.main.VIEW;

namespace view.ui.tech
{
    class Info : GuiSection
    {
        public Info(UITechTree tree, int width)
        {
            int wi = width;

            List<TechCurr> aa = new List<TechCurr>(FACTIONS.player().tech().currs());
            aa.Sort((o1, o2) => o1.cu.bo.key.CompareTo(o2.cu.bo.key));

            foreach (TechCurr c in FACTIONS.player().tech().currs())
            {
                RENDEROBJ cc = curr(c);
                if (getLastX2() + cc.body().width() > wi)
                {
                    add(cc, 0, body().y2());
                }
                else
                {
                    addRight(0, cc);
                }
            }

            if (S.get().developer)
            {
                addRight(0, new GButt.ButtPanel(UI.icons().m.questionmark)
                {
                    protected override void clickA()
                    {
                        VIEW.inters().popup.show(TechTest.get(), this);
                    }
                });
            }
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            base.render(r, ds);
        }

        private static GuiSection curr(TechCurr c)
        {
            GuiSection res = new GuiSection();
            GuiSection s = new GuiSection
            {
                protected override void hoverInfoSelf(GUI_BOX box)
                {
                    c.hover(box);
                }
            };

            s.add(UI.icons().s.dot.createColored(Node.cols.get(c.cu.index)), 0, 0);
            s.addRightC(4, c.cu.bo.icon);
            s.addRightC(4, new GHeader(c.cu.bo.name));
            s.addRightC(8, new GStat(UI.FONT().S)
            {
                public override void update(GText text)
                {
                    GFORMAT.iIncr(text, c.available());
                }
            }.r(DIR.NW));
            s.body().incrW(48);
            GStaples st = new GStaples(c.produced().historyRecords())
            {
                protected override void hover(GBox box, int stapleI)
                {
                    int i = c.produced().historyRecords() - 1 - stapleI;
                    GText t = box.text();
                    DicTime.setDaysAgo(t, i);
                    box.add(t);
                    box.NL();
                    box.add(GFORMAT.i(box.text(), c.produced().get(i)));
                }

                protected override double getValue(int stapleI)
                {
                    int i = c.produced().historyRecords() - 1 - stapleI;
                    return c.produced().get(i);
                }
            };
            st.body().setDim(8 * c.produced().historyRecords(), 32);
            s.add(st, s.body().x1(), s.body().y2() + 4);

            res.add(s);

            if (S.get().developer)
            {
                res.addRightC(8, new GButt.ButtPanel(UI.icons().s.plus)
                {
                    protected override void clickA()
                    {
                        foreach (RoomBlueprint b in SETT.ROOMS().all())
                        {
                            if (b is RoomBlueprintIns && b is ROOM_ADMIN_HOLDER)
                            {
                                AdminData d = ((ROOM_ADMIN_HOLDER)b).admin();
                                if (d.target == c.cu.bo)
                                    d.cheatAdd(500);
                            }
                        }
                    }
                });
            }

            res.addRightC(8, GCOLOR.UI().border().makeSprite(1, res.body().height()));
            res.body().incrW(8);

            return res;
        }
    }
}