using System;
using System.Collections.Generic;
using game;
using init.constant;
using init.paths;
using init.race;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sprite.text;
using util.colors;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;

public abstract class MenuScreenLoad : ClickableAbs
{
    private readonly GuiSection main;
    private readonly GuiSection prompt = new GuiSection();
    private readonly GuiSection deleteOld = new GuiSection();
    private GuiSection current;

    private SaveFile[] saves = new SaveFile[0];
    private readonly RENDEROBJ info;
    private int selectedSave = -1;

    public static CharSequence ¤¤name = "¤saved game";
    static CharSequence ¤¤delete = "¤Delete Save?";
    static CharSequence ¤¤deleteAll = "¤Delete Old";
    static CharSequence ¤¤deleteAllD = "¤Delete {0} outdated saves forever? You might still be able to load them if you revert for an earlier version of the game.";

    static CharSequence ¤¤prob = "Loading it may crash the game!";
    static CharSequence ¤¤prob2 = "¤This save may be incompatible.";

    private readonly COLOR color;
    private readonly Str version = new Str(16);
    private readonly GText tmp = new GText(UI.FONT().H2, 64);

    public MenuScreenLoad(COLOR color)
    {
        this.color = color;
        main = new GuiSection();
        info = new RENDEROBJ(new RECTANGLE(0, 0, 0, 0));
    }

    public void Initialize()
    {
        ScrollableArea scrollableArea = new ScrollableArea();
        for (int i = 0; i < saves.Length; i++)
        {
            Savebutt savebutt = new Savebutt(i, this);
            scrollableArea.Add(savebutt);
        }
        main.Add(scrollableArea);

        current = main;
    }

    public override bool Hover(COORDINATE mCoo)
    {
        if (current.Hover(mCoo))
            return true;
        return false;
    }

    public override bool Click()
    {
        current.Click();
        return false;
    }

    protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
    {
        if (current != main)
        {
            file = null;
            return;
        }

        current.Render(r, ds);

        if (file == null || file != open)
        {
            this.hovDS = 0;
            open = file;
        }

        if (open == null)
            return;

        this.hovDS += ds;
        if (this.hovDS < 0.25 && !file.SpecReady())
            return;

        if (file != null)
        {
            RenderInfo(r, file, info.Body(), ds);
        }
        file = null;
    }

    protected void RenderName(SPRITE_RENDERER r, SaveFile s, RECTANGLE body)
    {
        version.Clear();
        version.Add(VERSION.VersionMajor(s.version));
        version.Add('.');
        version.Add(VERSION.VersionMinor(s.version));

        UI.FONT().M.Render(r, version, body.X1(), body.Y1());

        UI.FONT().H2.Render(r, s.name, body.X1() + 90, body.Y1());

        tmp.Clear().Add('p').S();
        GFORMAT.I(tmp, s.Pop);
        UI.FONT().M.Render(r, tmp, body.X1() + 830, body.Y1());

        UI.FONT().M.Render(r, s.Ago, body.X2() - 180, body.Y1());
    }

    protected void RenderInfo(SPRITE_RENDERER r, SaveFile file, RECTANGLE body, double ds)
    {
        int y1 = RenderInfoGen(r, file, body);
        y1 = RenderInfoMod(r, file, body.X1(), y1, ds);
        RenderInfoProb(r, file, body.X1(), y1);
    }

    protected int RenderInfoGen(SPRITE_RENDERER r, SaveFile file, RECTANGLE body)
    {
        int ii = 0;
        RenderPair(r, ii++, body, Dic.¤¤Capitol, file.Spec().city);
        RenderPair(r, ii++, body, Dic.¤¤Ruler, file.Spec().ruler);
        RenderPair(r, ii++, body, RACES.name(), file.Spec().race);
        RenderPair(r, ii++, body, Dic.¤¤Population, GFORMAT.I(tmp.Clear(), file.Spec().population));
        RenderPair(r, ii++, body, Dic.¤¤Regions, GFORMAT.I(tmp.Clear(), file.Spec().regions));
        RenderPair(r, ii++, body, Dic.¤¤Subjects, GFORMAT.I(tmp.Clear(), file.Spec().regPop));
        return RenderPair(r, ii++, body, Dic.¤¤PlayTime, DicTime.SetYears(tmp.Clear(), file.Spec().playSeconds / (Config.sett().secondsPerHour * Config.sett().hoursPerDay * 16.0)));
    }

    private double mi = 0;

    protected int RenderInfoMod(SPRITE_RENDERER r, SaveFile file, int x1, int y1, double ds)
    {
        mi += ds;

        if (mi >= file.Spec().mods.Length)
            mi -= (int)mi;

        if (mi >= file.Spec().mods.Length)
            return y1 + UI.FONT().H2.Height() + 6;

        color.Bind();
        UI.FONT().H2.Render(r, Dic.¤¤Mods, x1, y1);
        tmp.Clear();
        tmp.Add(1 + (int)mi).Add('/').Add(file.Spec().mods.Length).S();
        tmp.Add(file.Spec().mods[(int)mi]);

        UI.FONT().M.Render(r, tmp, x1 + UI.FONT().H2.Width(Dic.¤¤Mods) + 8, y1);
        COLOR.Unbind();
        return y1 + UI.FONT().H2.Height() + 6;
    }

    protected void RenderInfoProb(SPRITE_RENDERER r, SaveFile file, int x1, int y1)
    {
        CharSequence p = file.Spec().Warning();
        if (p != null)
        {
            tmp.Clear();
            tmp.Color(file.Spec().fubar ? COLOR.REDISH : COLOR.YELLOW100).Add(p);
            tmp.S();
            tmp.Add(file.Spec().fubar ? ¤¤prob2 : ¤¤prob);
            tmp.SetMultipleLines(true);
            tmp.SetMaxWidth(C.MIN_WIDTH);
            (file.Spec().fubar ? COLOR.REDISH : COLOR.YELLOW100).Bind();
            UI.FONT().M.Render(r, tmp, x1, y1);
            COLOR.Unbind();
        }
    }

    private int RenderPair(SPRITE_RENDERER r, int ii, RECTANGLE body, CharSequence title, CharSequence value)
    {
        int x1 = body.X1() + (ii % 3) * 350;
        int y1 = body.Y1() + (ii / 3) * (UI.FONT().M.Height() + 6);
        color.Bind();
        UI.FONT().H2.Render(r, title, x1, y1);
        x1 += UI.FONT().H2.Width(title) + 8;
        COLOR.Unbind();
        UI.FONT().M.Render(r, value, x1, y1);
        return y1 + (UI.FONT().M.Height() + 6);
    }

    public bool HasSaves()
    {
        return saves.Length != 0;
    }

    public SaveFile[] Saves()
    {
        return saves;
    }
}

public class Savebutt : CLICKABLE.ClickableAbs, ScrollRow
{
    private readonly int index;
    private readonly MenuScreenLoad parent;

    public Savebutt(int index, MenuScreenLoad parent)
    {
        this.index = index;
        this.parent = parent;
        Body.SetWidth(MenuScreenLoad.Inner.Width());
        Body.SetHeight(28);
    }

    public void Init(int index)
    {
        this.index = index;
    }

    protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
    {
        SaveFile s = parent.Saves()[index];

        if (index == parent.SelectedSave)
        {
            GCOLOR.T().SELECTED.Bind();
        }
        else if (isHovered)
        {
            GCOLOR.T().HOVERED.Bind();
        }
        else if (s.SpecReady() && s.Spec().fubar)
        {
            COLOR.REDISH.Bind();
        }
        else if (s.Problem() != null || (s.SpecReady() && s.Spec().Warning() != null))
        {
            COLOR.YELLOW100.Bind();
        }

        parent.RenderName(r, s, Body);
        COLOR.WHITE50.Render(r, Body.X1(), Body.X2(), Body.Y2(), Body.Y2() + 1);
        COLOR.Unbind();
    }

    public override bool Hover(COORDINATE mCoo)
    {
        if (base.Hover(mCoo))
        {
            SaveFile s = parent.Saves()[index];
            parent.File = s;
            return true;
        }
        return false;
    }

    protected override void ClickA()
    {
        parent.SelectedSave = index;

        if (MButt.LEFT.IsDouble())
        {
            SaveFile f = parent.Saves()[index];
            if (!f.Spec().fubar)
                parent.Load(f);
        }
    }

    public void HoverInfoGet(GUI_BOX text)
    {
        SaveFile s = parent.Saves()[index];
        parent.File = s;
    }
}