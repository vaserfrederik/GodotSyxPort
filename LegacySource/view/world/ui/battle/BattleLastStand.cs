using System;
using System.Text;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;
using world;
using world.battle.spec;
using world.map.regions;

namespace view.world.ui.battle
{
    internal sealed class BattleLastStand : Battle
    {
        private static readonly CharSequence ¤¤name = "The Last Stand of {0}";
        private static readonly CharSequence ¤¤desc = "The enemy is about to take the city! Some of the defenders implore you to lead them in a sally, either to disperse the besiegers or to die with honour.";
        private static readonly CharSequence ¤¤CommandD = "¤Take personal command and fight this battle on the field. The outcome will depend on your skill of leading men into battle.";

        private static readonly CharSequence ¤¤Retire = "¤Decline";
        private static readonly CharSequence ¤¤RetireD = "¤Let the garrison fend for themselves and die in the defence of the city.";

        static BattleLastStand()
        {
            D.ts(typeof(BattleLastStand));
        }

        private readonly ACTION close;

        public BattleLastStand(ACTION close) : base(¤¤desc)
        {
            this.close = close;
        }

        protected override CharSequence title(WBattleSpec g)
        {
            Region reg = WORLD.REGIONS().map.Get(g.player.Coo());
            Str.TMP.Clear().Add(¤¤name);
            Str.TMP.Insert(0, reg.Info.Name());
            return Str.TMP;
        }

        protected override RENDEROBJ Buttons()
        {
            GuiSection ss = new GuiSection();
            GButt.ButtPanel bb;

            bb = new Butt(UI.icons().s.sword, ¤¤Command)
            {
                protected override void ClickA()
                {
                    close.Exe();
                    g.Engage();
                }
            };
            bb.HoverInfoSet(¤¤CommandD);
            ss.AddRightC(0, bb);

            bb = new Butt(UI.icons().s.cog, ¤¤Retire)
            {
                public override void HoverInfoGet(GUI_BOX text)
                {
                    Text t = text.Text();
                    t.Add(¤¤RetireD);
                    text.Add(t);
                }

                protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                {
                    base.Render(r, ds, isActive, isSelected, isHovered);
                    if (g.Victory)
                    {
                        OPACITY.O25.Bind();
                        COLOR.ORANGE100.Render(r, body, -4);
                        OPACITY.Unbind();
                    }
                }

                public override bool Hover(COORDINATE mCoo)
                {
                    if (base.Hover(mCoo))
                    {
                        SetCas(false, true);
                        return true;
                    }
                    return false;
                }

                protected override void ClickA()
                {
                    close.Exe();
                    g.Auto();
                }
            };
            ss.AddRightC(0, bb);

            return ss;
        }
    }
}