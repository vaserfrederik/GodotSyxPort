using System;
using util.gui.misc;
using snake2d;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.data.INT;
using util.gui.slider;
using view.keyboard;

namespace util.gui.misc
{
    public class GInputInt : GuiSection
    {
        private readonly INTE inValue;

        private readonly StringInputSprite sp = new StringInputSprite(10, UI.FONT().S)
        {
            protected override void Change()
            {
                int num = 0;
                int sign = 1;
                for (int i = 0; i < Text.Length; i++)
                {
                    if (i == 0 && Text[i] == '-')
                    {
                        sign = -1;
                        continue;
                    }

                    int n = Text[i] - '0';
                    if (n >= 0 && n < 10)
                    {
                        if (num * 10 + n > inValue.Max())
                            break;
                        num *= 10;
                        num += n;
                    }
                    else
                    {
                        Unfuck();
                        return;
                    }
                }
                if (num == 0 && sign == -1 && inValue.Min() < 0)
                {
                    inValue.Set(0);
                    Text.Clear().Add('-');
                }
                else
                {
                    inValue.Set(CLAMP.i(num * sign, inValue.Min(), inValue.Max()));
                    Text.Clear().Add(inValue.Get());
                }
            }
        }.PlaceHolder("0");

        public GInputInt(INTE inValue) : this(inValue, false, false)
        {
        }

        public GInputInt(INTE inValue, bool butts, bool doublebutts)
        {
            this.inValue = inValue;

            GInput inn = new GInput(sp);

            if (doublebutts)
            {
                GButt.ButtPanel pp = new GButt.ButtPanel(UI.icons().s.minifierBig)
                {
                    protected override void ClickA()
                    {
                        inValue.Inc(-Math.Max(1, (inValue.Max() - inValue.Min()) / 5));
                        if (KEYS.MAIN().MOD.IsPressed())
                            inValue.Set(inValue.Min());
                    }

                    protected override void RenAction()
                    {
                        ActiveSet(inValue.Get() > inValue.Min());
                    }

                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        GAllocator.Hov(text);
                        base.HoverInfoGet(text);
                    }
                };
                pp.RepetativeSet(true);
                pp.Body().SetHeight(inn.Body.Height());
                AddRightC(0, pp);
            }

            if (butts)
            {
                GButt.ButtPanel pp = new GButt.ButtPanel(UI.icons().s.minifier)
                {
                    protected override void ClickA()
                    {
                        inValue.Inc(-1);
                        if (KEYS.MAIN().MOD.IsPressed())
                            inValue.Set(inValue.Min());
                    }

                    protected override void RenAction()
                    {
                        ActiveSet(inValue.Get() > inValue.Min());
                    }

                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        GAllocator.Hov(text);
                        base.HoverInfoGet(text);
                    }
                };
                pp.RepetativeSet(true);
                pp.Body().SetHeight(inn.Body.Height());
                AddRightC(0, pp);
            }

            AddRightC(0, inn);
            if (butts)
            {
                GButt.ButtPanel pp = new GButt.ButtPanel(UI.icons().s.magnifier)
                {
                    protected override void ClickA()
                    {
                        inValue.Inc(1);
                        if (KEYS.MAIN().MOD.IsPressed())
                            inValue.Set(inValue.Max());
                    }

                    protected override void RenAction()
                    {
                        ActiveSet(inValue.Get() < inValue.Max());
                    }

                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        GAllocator.Hov(text);
                        base.HoverInfoGet(text);
                    }
                };
                pp.RepetativeSet(true);
                pp.Body().SetHeight(inn.Body.Height());
                AddRightC(0, pp);
            }

            if (doublebutts)
            {
                GButt.ButtPanel pp = new GButt.ButtPanel(UI.icons().s.magnifierBig)
                {
                    protected override void ClickA()
                    {
                        inValue.Inc(Math.Max(1, (inValue.Max() - inValue.Min()) / 5));
                        if (KEYS.MAIN().MOD.IsPressed())
                            inValue.Set(inValue.Max());
                    }

                    protected override void RenAction()
                    {
                        ActiveSet(inValue.Get() < inValue.Max());
                    }

                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        GAllocator.Hov(text);
                        base.HoverInfoGet(text);
                    }
                };
                pp.RepetativeSet(true);
                pp.Body().SetHeight(inn.Body.Height());
                AddRightC(0, pp);
            }
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            Unfuck();
            base.Render(r, ds);
        }

        private void Unfuck()
        {
            int am = inValue.Get();
            if (am == 0 && Text.Length == 1 && Text[0] == '-' && inValue.Min() < 0)
            {
                return;
            }
            Text.Clear();
            Text.Add(am);
        }
    }
}