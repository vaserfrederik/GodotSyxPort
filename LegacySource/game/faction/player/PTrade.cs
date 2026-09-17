using System;
using System.Collections.Generic;
using System.IO;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.time;
using init.trade;
using snake2d.util.file;
using util.info;
using util.statistics;
using util.text;
using world.region;

public class PTrade
{
    public readonly HistoryTradable pricesBuy;
    public readonly HistoryTradable pricesSell;
    public readonly HistoryTradable pricesAve;

    public readonly HistoryTradable unitsImported;
    public readonly HistoryTradable unitsExported;
    public readonly HistoryTradable priceImported;
    public readonly HistoryTradable priceExported;

    public readonly HistoryTradable outImported;
    public readonly HistoryTradable inExported;

    private static readonly CharSequence ¤¤InExported = "¤Exported";
    private static readonly CharSequence ¤¤InExportedD = "¤Moneys earned from exports.";

    private static readonly CharSequence ¤¤OutImported = "¤Imported";
    private static readonly CharSequence ¤¤OutImportedD = "¤Moneys spent on imports.";

    private static readonly CharSequence ¤¤units = "¤units";
    private static readonly CharSequence ¤¤unitsD = "¤The amount of units traded.";
    private static readonly CharSequence ¤¤price = "¤Price";
    private static readonly CharSequence ¤¤priceD = "¤Price per unit.";

    static PTrade()
    {
        D.ts(typeof(PTrade));
    }

    public PTrade()
    {
        pricesBuy = new HistoryTradable(
            new INFO(Dic.¤¤buyPrice, ""),
            PCredits.history,
            TIME.days(),
            true);
        pricesSell = new HistoryTradable(
            new INFO(Dic.¤¤sellPrice, ""),
            PCredits.history,
            TIME.days(),
            true);
        pricesAve = new HistoryTradable(
            new INFO(Dic.¤¤sellPrice, ""),
            PCredits.history,
            TIME.days(),
            true);

        unitsImported = new HistoryTradable(new INFO(¤¤units, ¤¤unitsD), PCredits.history, TIME.days(), false);
        unitsExported = new HistoryTradable(new INFO(¤¤units, ¤¤unitsD), PCredits.history, TIME.days(), false);
        priceImported = new HistoryTradable(new INFO(¤¤price, ¤¤priceD), PCredits.history, TIME.days(), false);
        priceExported = new HistoryTradable(new INFO(¤¤price, ¤¤priceD), PCredits.history, TIME.days(), false);

        inExported = new HistoryTradable(new INFO(¤¤InExported, ¤¤InExportedD), PCredits.history, TIME.days(), false);
        outImported = new HistoryTradable(new INFO(¤¤OutImported, ¤¤OutImportedD), PCredits.history, TIME.days(), false);
    }

    private int ri = 0;

    public void Update(double ds)
    {
        ri %= TR.ALL().Count;
        TRADABLE res = TR.ALL()[ri];

        int s = 0;
        int m = int.MaxValue;
        if (DIP.Traders().Count == 0)
        {
            foreach (FactionNPC f in RD.DIST().Neighs())
            {
                if (f.CapitolRegion() != null)
                {
                    s = Math.Max(s, f.Res(res).PriceBuyP());
                    m = Math.Min(m, f.Res(res).PriceSellP());
                }
            }
        }
        else
        {
            foreach (Faction ff in DIP.Traders())
            {
                FactionNPC f = (FactionNPC)ff;
                if (f.CapitolRegion() != null)
                {
                    s = Math.Max(s, f.Res(res).PriceBuyP());
                    m = Math.Min(m, f.Res(res).PriceSellP());
                }
            }
        }

        if (m == int.MaxValue)
            m = 0;
        pricesSell.Set(res, s);
        pricesBuy.Set(res, m);
        pricesAve.Set(res, FACTIONS.PRICE().Get(res));

        ri++;
    }

    public void Trade(double amount, TRADABLE res, int resAm)
    {
        if (amount < 0)
        {
            outImported.Inc(res, (int)-amount);
            unitsImported.Inc(res, resAm);
            int p = unitsImported.Get(res) > 0 ? (outImported.Get(res)) / (unitsImported.Get(res)) : 0;
            priceImported.Set(res, p);
        }
        else
        {
            inExported.Inc(res, (int)amount);
            unitsExported.Inc(res, resAm);
            priceExported.Set(res, (inExported.Get(res) + 1) / (unitsExported.Get(res) + 1));
        }
    }

    public readonly SAVABLE Saver = new SAVABLE()
    {
        public void Save(FilePutter file)
        {
            pricesBuy.Save(file);
            pricesSell.Save(file);
            pricesAve.Save(file);
            outImported.Save(file);
            inExported.Save(file);
            unitsImported.Save(file);
            unitsExported.Save(file);
            priceImported.Save(file);
            priceExported.Save(file);
            file.I(ri);
        }

        public void Load(FileGetter file)
        {
            pricesBuy.Load(file);
            pricesSell.Load(file);
            pricesAve.Load(file);
            outImported.Load(file);
            inExported.Load(file);
            unitsImported.Load(file);
            unitsExported.Load(file);
            priceImported.Load(file);
            priceExported.Load(file);
            ri = file.I();
        }

        public void Clear()
        {
            pricesBuy.Clear();
            pricesSell.Clear();
            pricesAve.Clear();
            outImported.Clear();
            inExported.Clear();
            unitsImported.Clear();
            unitsExported.Clear();
            priceImported.Clear();
            priceExported.Clear();
        }
    };
}