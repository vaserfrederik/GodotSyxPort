using System.Collections.Generic;
using Util.Gui.Common;
using Init.Race;
using Init.Sprite.UI;
using Snake2D;
using Snake2D.Util.MATH;
using Snake2D.Util.Color;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Hoverable;
using Snake2D.Util.Sets;
using Util.Colors;
using Util.Gui.Misc;

public class UIPickerRace
{
    public readonly GuiSection section = new GuiSection();
    private readonly LIST<Race> all;
    private int current = 0;

    public UIPickerRace()
        : this(RACES.All())
    {
    }

    public UIPickerRace(LIST<Race> races)
    {
        this.all = races;

        GButt.ButtPanel b = new GButt.ButtPanel(UI.Icons().M.ArrowLeft)
        {
            protected override void ClickA()
            {
                Set(current - 1);
                base.ClickA();
            }
        };
        b.Body.SetHeight(46);
        b.Pad(1, 0);
        section.Add(b);

        section.AddRightC(0, new HoverableAbs(80, 46)
        {
            protected override void Render(SPRITE_RENDERER r, float ds, bool isHovered)
            {
                GCOLOR.UI().Border().Render(r, Body);
                GCOLOR.UI().Bg().Render(r, Body, -1);
                COLOR.White35.Bind();

                all.GetC(current - 1).Appearance().Icon.RenderC(r, Body.CX() - 18, Body.CY());
                all.GetC(current + 1).Appearance().Icon.RenderC(r, Body.CX() + 18, Body.CY());
                COLOR.Unbind();
                all.GetC(current).Appearance().IconBig.RenderC(r, Body.CX(), Body.CY());
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                Hover((GBox)text, all.GetC(current));
            }
        });

        b = new GButt.ButtPanel(UI.Icons().M.ArrowRight)
        {
            protected override void ClickA()
            {
                Set(current + 1);
                base.ClickA();
            }
        };
        b.Body.SetHeight(46);
        b.Pad(1, 0);
        section.AddRightC(0, b);
    }

    public void Set(Race race)
    {
        for (int i = 0; i < all.Size(); i++)
        {
            if (all.Get(i) == race)
            {
                current = i;
                break;
            }
        }
    }

    public void Set(int ri)
    {
        current = ri;
        current = MATH.Mod(current, all.Size());
    }

    public void Hover(GBox b, Race race)
    {
        b.Title(race.Info.Names);
        b.Text(race.Info.Desc);
        b.NL();
    }

    public Race Race()
    {
        return all.GetC(current);
    }
}