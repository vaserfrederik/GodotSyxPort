using System;
using System.Collections.Generic;
using game;
using init.settings;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid.ai.types.parent;
using settlement.stats;
using settlement.stats.equip;
using settlement.stats.stat;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.main;

namespace view.sett.ui.subject
{
    internal class UISubjectEquip : GuiSection
    {
        private static readonly string ¤¤resource = "¤Carried resource";
        private static readonly string ¤¤infant = "¤Currently caring for offspring. Can not work. Child will grow up in {0} days.";

        static UISubjectEquip()
        {
            D.ts(typeof(UISubjectEquip));
        }

        public UISubjectEquip(AInfo a, HTYPE t)
        {
            StatsEquip pr = STATS.EQUIP();

            int ii = 0;
            const int rrr = 4;
            foreach (Equip pp in pr.allE())
            {
                STAT p = pp.stat();
                CLICKABLE c = new CLICKABLE.ClickableAbs(Icon.L + 8, Icon.L + Icon.M / 2)
                {
                    t = new GText(UI.FONT().S, 8)
                };
                c.render = (SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered) =>
                {
                    if (isHovered)
                    {
                        COLOR.BLUEDARK.render(r, body());
                    }

                    pp.resource().icon().renderC(r, body().cX(), body().y1() + Icon.L / 2);
                    t.clear();
                    int am = p.indu().get(a.a.indu());
                    int max = pp.max(a.a.indu());
                    GFORMAT.iofk(t, am, max);
                    t.lablify();
                    t.adjustWidth();
                    t.renderC(r, body().cX(), body().y1() + Icon.L);
                };

                c.hoverInfoGet = (GUI_BOX text) =>
                {
                    pp.hover(text, a.a.indu());
                };

                c.clickA = () =>
                {
                    if (S.get().developer)
                        DebugInput.activate(p.indu(), a.a);
                    else
                        VIEW.s().ui.standing.openAccess(a.a.race());
                };

                addGrid(c, ii++, rrr, 4, 4);
            }

            {
                GuiSection ss = new GuiSection();
                ss.addRightC(8, new HoverableAbs(Icon.M * 3, Icon.M)
                {
                    t = new GText(UI.FONT().M, 4)
                });
                ss.render = (SPRITE_RENDERER r, float ds, bool isHovered) =>
                {
                    if (a.a.indu().hType() == HTYPES.PARENT() || a.a.indu().hType() == HTYPES.PARENT_SLAVE())
                    {
                        t.clear().add(a.a.race().physics.babyDays - AIModule_Parent.daysOld(a.a));
                        t.renderCY(r, body().x1(), body().cY());
                        UI.icons().m.baby.render(r, body().x1() + Icon.M, body().y1());
                    }
                };

                ss.hoverInfoGet = (GUI_BOX text) =>
                {
                    if (a.a.indu().hType() == HTYPES.PARENT() || a.a.indu().hType() == HTYPES.PARENT_SLAVE())
                    {
                        GBox b = (GBox)text;
                        GText t = b.text();
                        t.add(¤¤infant);
                        t.insert(0, a.a.race().physics.babyDays - AIModule_Parent.daysOld(a.a));
                        b.add(t);
                    }
                };

                GETTER<HCLASS_RACE> g = new GETTER<HCLASS_RACE>()
                {
                    get = () => HCLASS_RACE.clP(a.a.indu())
                };

                ss.addRightC(8, TmpBoostingButt.make(g, GAME.BOOST().popcl));

                ss.addRightC(8, new HoverableAbs(Icon.M * 3, Icon.M)
                {
                    t = new GText(UI.FONT().M, 4)
                });
                ss.render = (SPRITE_RENDERER r, float ds, bool isHovered) =>
                {
                    if (a.a.ai().resourceCarried() != null)
                    {
                        t.clear().add(a.a.ai().resourceA());
                        t.renderCY(r, body().x1(), body().cY());
                        a.a.ai().resourceCarried().icon().render(r, body().x1() + Icon.M, body().y1());
                    }
                };

                ss.hoverInfoGet = (GUI_BOX text) =>
                {
                    text.text(¤¤resource);
                    text.NL();
                    if (a.a.ai().resourceCarried() != null)
                    {
                        text.text(a.a.ai().resourceCarried().name);
                    }
                };

                addRelBody(8, DIR.N, ss);
            }
        }
    }
}