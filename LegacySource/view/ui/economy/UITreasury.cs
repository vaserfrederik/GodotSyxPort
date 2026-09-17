using System;
using System.Collections.Generic;
using game.faction;
using game.faction.npc;
using init.settings;
using init.sprite.UI;
using init.trade;
using init.type;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.common;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using view.ui.goods;
using view.ui.manage;

public class UITreasury : IFullView
{
    private static readonly string ¤¤unused = "Show resources not actively traded";
    private static readonly string ¤¤import = "Show resources that are imported.";
    private static readonly string ¤¤export = "Show resources that are exported.";
    private static readonly string ¤¤economy = "Economy & Trade";
    private static readonly string ¤¤priceDesc = "The average global price, the total production rate without boosts, and the price multiplied with the production rate. This might give you a sense of what industries are profitable for you. The last two columns shows your current average bonus of selected race.";

    private GScrollRows ta;

    static
    {
        D.ts(typeof(UITreasury));
    }

    public UITreasury() : base(¤¤economy, UI.icons().l.coin)
    {
        section.body().setWidth(WIDTH).setHeight(1);
        final IntImp ii = new IntImp();
        GETTER_IMP<TRADABLE> gres = new GETTER_IMP<>();

        GuiSection s = new GuiSection
        {
            hover = mCoo =>
            {
                ii.set(-1);
                gres.set(null);
                return base.hover(mCoo);
            }
        };

        s.addDownC(0, new MainChart(HEIGHT, ii, 10));
        s.addRight(32, new MainDetails(ii));

        GuiSection f = new GuiSection();

        GButt.ButtPanel oo = new GButt.ButtPanel(UI.icons().m.coins.resized(Icon.L))
        {
            GuiSection s = new Prices();

            protected override void clickA()
            {
                VIEW.inters().popup.show(s, this);
            }
        }.pad(2, 4);
        f.addDown(0, oo);
        GButt.ButtPanel unused = new GButt.ButtPanel(UI.icons().m.questionmark.resized(Icon.L))
        {
            protected override void clickA()
            {
                selectedToggle();
            }
        }.pad(2, 4);
        unused.hoverInfoSet(¤¤unused);
        unused.selectedSet(true);
        f.addDown(0, unused);
        GButt.ButtPanel impot = new GButt.ButtPanel(SETT.ROOMS().IMPORT.icon)
        {
            protected override void clickA()
            {
                selectedToggle();
            }
        }.pad(2, 4);
        impot.hoverInfoSet(¤¤import);
        impot.selectedSet(true);
        f.addDown(0, impot);
        GButt.ButtPanel export = new GButt.ButtPanel(SETT.ROOMS().EXPORT.icon)
        {
            protected override void clickA()
            {
                selectedToggle();
            }
        }.pad(2, 4);
        export.hoverInfoSet(¤¤export);
        export.selectedSet(true);
        f.addDown(0, export);

        s.add(f, s.body().x2() + 32, s.body().y2() - f.body().height());

        UIGoodsImport im = new UIGoodsImport();
        UIGoodsExport ex = new UIGoodsExport(true);
        List<RENDEROBJ> rows = new List<RENDEROBJ>(TR.ALL().Count);
        foreach (TRADABLE res in TR.ALL())
            rows.Add(new RRow(res, ii, gres, 12, im, ex));

        int height = HEIGHT - s.body().height() - 16;
        height = height / rows[0].body().height();
        height *= rows[0].body().height();
        ta = new GScrollRows(rows, height)
        {
            passesFilter = (i, o) =>
            {
                if (unused.selectedIs())
                    return true;
                TRADABLE res = TR.ALL()[i];
                if (impot.selectedIs() && res.pb().importing())
                    return true;
                if (export.selectedIs() && res.ps().exporting() == null)
                    return true;
                return false;
            }
        };
        s.add(ta.view(), s.body().x1() - 58, s.body().y2() + 8);

        s.add(new Factions(HEIGHT), s.body().x2() + 16, s.body().y1());
        section.addRelBody(16, DIR.S, s);
    }

    public override void hoverInfoGet(GUI_BOX box)
    {
        GBox b = (GBox)box;
        b.title(¤¤economy);

        b.textLL(Dic.¤¤Treasury);
        b.tab(6);
        b.add(GFORMAT.i(b.text(), (long)FACTIONS.player().credits().getD()));
        b.NL();

        foreach (TRADABLE res in TR.ALL())
        {
            if (res.pb().importing())
            {
                GText t = b.text();
                CharSequence p = FACTIONS.player().buyer(res).problem();
                if (p != null)
                {
                    b.add(res.icon());
                    b.add(t.errorify().add(p));
                    b.NL();
                }
                else
                {
                    p = FACTIONS.player().buyer(res).warning();
                    if (p != null)
                    {
                        b.add(res.icon());
                        b.add(t.warnify().add(p));
                        b.NL();
                    }
                }
            }

            if (res.ps().exporting() == null)
            {
                GText t = b.text();
                CharSequence p = FACTIONS.player().seller(res).problem();

                if (p != null)
                {
                    b.add(res.icon());
                    b.add(t.errorify().add(p));
                    b.NL();
                }
                else
                {
                    p = FACTIONS.player().seller(res).warning();
                    if (p != null)
                    {
                        b.add(res.icon());
                        b.add(t.warnify().add(p));
                        b.NL();
                    }
                }
            }
        }
    }

    private class Prices : GuiSection
    {
        public Prices()
        {
            UIPanel panel = new UIPanel(new UIBox(new Vec2i(100, 100), 200, 300));
            add(panel);

            UIPanel headerPanel = new UIPanel(new UIBox(new Vec2i(0, 0), panel.width(), 30));
            headerPanel.add(new GText(UI.FONT().B, "Prices and Rates"));
            panel.add(headerPanel, 0, 0);

            int y = 40;
            add(new GText(UI.FONT().S, "Resource"), 10, y);
            add(new GText(UI.FONT().S, "Global Price"), 120, y);
            add(new GText(UI.FONT().S, "Base Rate"), 220, y);
            add(new GText(UI.FONT().S, "Rate *"), 320, y);
            add(new GText(UI.FONT().S, "Rate * x Price"), 420, y);
            y += 20;

            foreach (TRADABLE res in TR.ALL())
            {
                add(new GText(UI.FONT().S, res.name()), 10, y);
                add(new GText(UI.FONT().S, FACTIONS.PRICE().get(res).ToString()), 120, y);
                add(new GText(UI.FONT().S, SETT.RECIPES().ratesV.vanillaRate(res).ToString()), 220, y);
                add(new GText(UI.FONT().S, SETT.RECIPES().rates.rateTotal(HCLASS_RACE.clP(pick.race()), res).ToString()), 320, y);
                add(new GText(UI.FONT().S, (FACTIONS.PRICE().get(res) / SETT.RECIPES().rates.rateTotal(HCLASS_RACE.clP(pick.race()), res)).ToString()), 420, y);
                y += 20;
            }

            GText t = new GText(UI.FONT().S, ¤¤priceDesc);
            t.setMaxWidth(400);
            t.setMultipleLines(true);
            add(t, 10, y + 10);
        }
    }
}