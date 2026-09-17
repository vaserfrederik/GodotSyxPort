using System;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Misc;
using Snake2D.Util.Sprite.Text;
using Util.Gui.Misc;
using Util.Text;
using World.Battle.Spec;
using World.Map.Regions;

namespace View.World.Ui.Battle
{
    internal sealed class BattleSally : Battle
    {
        private static readonly CharSequence ¤¤name = "The Sally of {0}";
        private static readonly CharSequence ¤¤desc = "Our garrison is about to sally out to break the siege.";
        private static readonly CharSequence ¤¤CommandD = "¤Take personal command and fight this battle on the field. The outcome will depend on your skill of leading men into battle.";

        private static readonly CharSequence ¤¤Retire = "¤Fall Back";
        private static readonly CharSequence ¤¤RetireD = "¤Abort the whole operation. No one will be harmed.";

        static BattleSally()
        {
            D.ts(typeof(BattleSally));
        }

        private readonly Action close;

        public BattleSally(Action close) : base(¤¤desc)
        {
            this.close = close;
        }

        protected override CharSequence Title(WBattleSpec g)
        {
            Region reg = WORLD.REGIONS().map.Get(g.Player.Coo());
            Str.TMP.Clear().Add(¤¤name);
            Str.TMP.Insert(0, reg.Info.Name());
            return Str.TMP;
        }

        protected override RenderObj Buttons()
        {
            GuiSection ss = new GuiSection();
            GButt.ButtPanel bb;

            bb = new Butt(UI.Icons().S.Sword, ¤¤Command)
            {
                protected override void ClickA()
                {
                    close?.Invoke();
                    g.Engage();
                }
            };
            bb.HoverInfoSet(¤¤CommandD);
            ss.AddRightC(0, bb);

            bb = new Butt(UI.Icons().S.Cog, ¤¤AutoResolve)
            {
                public override void HoverInfoGet(GUI_BOX text)
                {
                    Text t = text.Text();
                    t.Add(¤¤autoD);
                    t.Insert(0, g.Victory ? Dic.¤¤Victory
                        : (g.Player.Losses() >= g.Player.Men() ? ¤¤Annihilation : Dic.¤¤Defeat));
                    t.Insert(1, g.Player.Losses());
                    t.Insert(2, g.Enemy.Losses());
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
                    close?.Invoke();
                    g.Auto();
                }
            };
            ss.AddRightC(0, bb);

            bb = new Butt(UI.Icons().S.ArrowLeft, ¤¤Retire)
            {
                protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                {
                    base.Render(r, ds, isActive, isSelected, isHovered);
                    if (g.Player.LossesRetreat() >= g.Player.Men())
                    {
                        OPACITY.O25.Bind();
                        COLOR.RED100.Render(r, body, -4);
                        OPACITY.Unbind();
                    }
                }

                protected override void ClickA()
                {
                    if (g.Player.LossesRetreat() >= g.Player.Men())
                    {
                        return;
                    }
                    close?.Invoke();
                    g.Retreat();
                }

                public override bool Hover(COORDINATE mCoo)
                {
                    if (base.Hover(mCoo))
                    {
                        SetCas(true, false);
                        return true;
                    }
                    return false;
                }
            };
            bb.HoverInfoSet(¤¤RetireD);
            ss.AddRightC(0, bb);

            return ss;
        }
    }
}