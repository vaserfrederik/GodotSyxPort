using System;
using System.Text;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;

namespace view.world.ui.battle
{
    final class BattleAssist : Battle
    {
        private static CharSequence ¤¤desc = "Our allies are about to engage an enemy host. Do we help them, or do we stand idly by and watch?";
        private static CharSequence ¤¤CommandD = "¤Take personal command and fight this battle on the field. The outcome will depend on your skill of leading men into battle. Your allies might resent you if you waste their men.";
        private static CharSequence ¤¤Assist = "¤Assist";
        private static CharSequence ¤¤assistD = "¤Partake in this battle but let your allies command it. The result will be {0}. You will lose about {1} men and inflict about {2} casualties on the enemy.";
        private static CharSequence ¤¤Annihilation = "¤your annihilation";
        private static CharSequence ¤¤Decline = "¤Decline";
        private static CharSequence ¤¤DeclineD = "¤Decline to partake in this conflict and spare your men and the enemy at the expense of your allies.";

        static
        {
            D.ts(typeof(BattleAssist));
        }

        private readonly ACTION close;

        BattleAssist(ACTION close) : base(¤¤desc)
        {
            this.close = close;
        }

        protected override RENDEROBJ buttons()
        {
            GuiSection ss = new GuiSection();
            GButt.ButtPanel bb;

            bb = new Butt(UI.icons().s.sword, ¤¤Command)
            {
                protected override void clickA()
                {
                    close.exe();
                    g.engage();
                }
            };
            bb.hoverInfoSet(¤¤CommandD);
            ss.addRightC(0, bb);

            bb = new Butt(UI.icons().s.cog, ¤¤Assist)
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    Text t = text.text();
                    t.add(¤¤assistD);
                    t.insert(0, g.victory ? Dic.¤¤Victory
                            : (g.player.losses() >= g.player.men() ? ¤¤Annihilation : Dic.¤¤Defeat));
                    t.insert(1, g.player.losses());
                    t.insert(2, g.enemy.losses());
                    text.add(t);
                }

                protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                {
                    base.render(r, ds, isActive, isSelected, isHovered);
                    if (g.victory)
                    {
                        OPACITY.O25.bind();
                        COLOR.ORANGE100.render(r, body, -4);
                        OPACITY.unbind();
                    }
                }

                public override bool hover(COORDINATE mCoo)
                {
                    if (base.hover(mCoo))
                    {
                        setCas(false, true);
                        return true;
                    }
                    return false;
                }

                protected override void clickA()
                {
                    close.exe();
                    g.auto();
                }
            };
            ss.addRightC(0, bb);

            bb = new Butt(UI.icons().s.arrow_left, ¤¤Decline)
            {
                protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                {
                    base.render(r, ds, isActive, isSelected, isHovered);
                    if (g.player.lossesRetreat() >= g.player.men())
                    {
                        OPACITY.O25.bind();
                        COLOR.RED100.render(r, body, -4);
                        OPACITY.unbind();
                    }
                }

                protected override void clickA()
                {
                    if (g.player.lossesRetreat() >= g.player.men())
                    {
                        return;
                    }
                    close.exe();
                    g.retreat();
                }

                public override bool hover(COORDINATE mCoo)
                {
                    if (base.hover(mCoo))
                    {
                        setCas(true, false);
                        return true;
                    }
                    return false;
                }
            };
            bb.hoverInfoSet(¤¤DeclineD);
            ss.addRightC(0, bb);

            return ss;
        }
    }
}