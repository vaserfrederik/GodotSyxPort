using System;
using init.sprite;
using init.type;
using settlement.stats;
using settlement.stats.standing;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;

namespace view.sett.ui.subject
{
    final class UISubjectType : GuiSection
    {
        public readonly HTYPE type;
        private readonly GuiSection currentS = new GuiSection();
        private GuiSection current;

        private static readonly CharSequence ¤¤race = "¤Read up about current race in the tome of knowledge.";
        private static readonly CharSequence ¤¤favourite = "¤Mark as favourite";
        private static readonly CharSequence ¤¤follow = "¤Center screen at subject.";

        static UISubjectType()
        {
            D.ts(typeof(UISubjectType));
        }

        UISubjectType(AInfo a, HTYPE type)
        {
            GuiSection top = new GuiSection();
            int w = 0;
            this.type = type;
            top.addRightC(0, new GButt.ButtPanel(SPRITES.icons().m.arrow_right)
            {
                protected override void clickA()
                {
                    a.follow = 20;
                }

                protected override void renAction()
                {
                    selectedSet(a.follow > 0);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.text(¤¤follow);
                }
            }.pad(4, 1));

            top.addRightC(0, new GButt.ButtPanel(SPRITES.icons().m.heart)
            {
                protected override void clickA()
                {
                    STATS.APPEARANCE().favo.set(a.a.indu(), (STATS.APPEARANCE().favo.get(a.a.indu()) + 1) & 1);
                }

                protected override void renAction()
                {
                    selectedSet(STATS.APPEARANCE().favo.get(a.a.indu()) == 1);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.text(¤¤favourite);
                }
            }.pad(4, 1));

            UISubjectInfo info = new UISubjectInfo(a, ISidePanel.HEIGHT - 40, type);
            top.addRightC(0, new GButt.ButtPanel(Dic.¤¤Info)
            {
                protected override void clickA()
                {
                    set(info);
                    base.clickA();
                }

                protected override void renAction()
                {
                    selectedSet(current == info);
                }
            }.pad(4, 1));
            w = Math.Max(w, info.body().width());

            UISubjectProperties prop = new UISubjectProperties(a, ISidePanel.HEIGHT - 40, type);
            top.addRightC(0, new GButt.ButtPanel(Dic.¤¤Properites)
            {
                protected override void clickA()
                {
                    set(prop);
                    base.clickA();
                }

                protected override void renAction()
                {
                    selectedSet(current == prop);
                }
            }.pad(4, 1));
            w = Math.Max(w, prop.body().width());

            if (type.CLASS.player)
            {
                UISubjectStats stats = new UISubjectStats(a, ISidePanel.HEIGHT - 40);
                top.addRightC(0, new GButt.ButtPanel(STANDINGS.CITIZEN().fullfillment.info().name)
                {
                    protected override void clickA()
                    {
                        set(stats);
                        base.clickA();
                    }

                    protected override void renAction()
                    {
                        selectedSet(current == stats);
                    }
                }.pad(4, 1));
                w = Math.Max(w, stats.body().width());
            }

            top.addRightC(0, new GButt.ButtPanel(SPRITES.icons().m.questionmark)
            {
                protected override void clickA()
                {
                    VIEW.UI().wiki.showRace(a.a.race());
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.text(¤¤race);
                    base.hoverInfoGet(text);
                }
            }.pad(4, 1));
            w = Math.Max(w, top.body().width());

            body().setDim(w, 1);

            addRelBody(0, DIR.S, top);
            currentS.body().setDim(w, ISidePanel.HEIGHT - body().height());
            addRelBody(8, DIR.S, currentS);

            set(info);
        }

        private void set(GuiSection s)
        {
            current = s;
            int y1 = currentS.body().y1();
            currentS.clear();
            currentS.add(s);
            currentS.body().moveY1(y1);
            currentS.body().centerX(body());
        }
    }
}