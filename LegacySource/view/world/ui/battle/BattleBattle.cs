using System;
using System.Collections.Generic;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;

namespace view.world.ui.battle
{
    final class BattleBattle : Battle
    {
        public static string ¤¤RetreatCant = "¤Your forces are trapped, they can't retreat.";
        private static string ¤¤RetreatD = "¤Make a tactical retreat. Your allies will not engage, but your commanding force will move out of harms way. In doing so you will lose {0} men and some equipment in the process.";
        private static string ¤¤desc = "Our mighty forces are about to engage an enemy host. What are your orders?";
        private static string ¤¤commandDD = "¤Take personal command and fight this battle on the field. The outcome will depend on your skill of leading men into battle. Your allies might resent you if you waste their men.";
        private static string ¤¤Retreat = "¤Retreat";

        static BattleBattle()
        {
            D.ts(typeof(BattleBattle));
        }

        private readonly ACTION close;

        public BattleBattle(ACTION close) : base(¤¤desc)
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
            bb.hoverInfoSet(¤¤commandDD);
            ss.addRightC(0, bb);

            bb = new Butt(UI.icons().s.cog, ¤¤AutoResolve)
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    Text t = text.text();
                    t.add(¤¤autoD);
                    t.insert(0, g.victory ? Dic.¤¤Victory
                            : (g.player.losses() >= g.player.men() ? ¤¤Annihilation : Dic.¤¤Defeat));
                    t.insert(1, g.player.losses());
                    t.insert(2, g.enemy.losses());
                    text.add(t);
                }

                public override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
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

            bb = new Butt(UI.icons().s.arrow_left, ¤¤Retreat)
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    if (g.player.lossesRetreat() >= g.player.men())
                        text.text(¤¤RetreatCant);
                    else
                    {
                        Text t = text.text();
                        t.add(¤¤RetreatD);
                        t.insert(0, g.player.lossesRetreat());
                        text.add(t);
                    }
                }

                public override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
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
            ss.addRightC(0, bb);

            return ss;
        }
    }
}