using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game.faction.npc.stockpile;
using game.GAME;
using game.VERSION;
using game.boosting;
using game.faction;
using init.trade;
using settlement.main;
using settlement.recipe;
using snake2d;
using snake2d.util.misc;
using view.interrupter;

class Updater
{
    public static double recoveryRate = 0.2;

    public Updater()
    {
        ACTION a = new ACTION(() =>
        {
            foreach (FactionNPC f in FACTIONS.NPCs())
            {
                f.stockpile.saver().clear();
                f.stockpile.update(f, 0);
                f.credits().set(0);
            }
            GAME.factions().prime();
        });

        IDebugPanel.add("TRADE RESET", a);

        GAME.saver().onAfterLoad(new ACTION_O<Path>(t =>
        {
            if (VERSION.versionIsBefore(71, 22))
                a.exe();
        }));

        IDebugPanel.add("TRADE DEBUG", new ACTION(() =>
        {
            KeyMap<EE> map = new KeyMap<EE>();

            double[] rb = new double[SETT.RECIPES().all().size()];
            foreach (FactionNPC f in FACTIONS.NPCs())
            {
                foreach (Recipe ins in SETT.RECIPES().all())
                {
                    rb[ins.index] += ins.bo.get(f);

                    foreach (Booster b in ins.bo.all())
                    {
                        string k = b.ToString();
                        if (!map.containsKey(k))
                        {
                            EE e = new EE();
                            e.bo = b;
                            e.t = ins.bo;
                            map.put(k, e);
                        }
                        map.get(k).vv += b.get(f);
                        map.get(k).am++;
                    }
                }
            }

            foreach (Recipe r in SETT.RECIPES().all())
            {
                LOG.ln($"{r.name} {Math.Round(100 * rb[r.index()] / FACTIONS.NPCs().size())}");
            }
            LOG.ln();

            foreach (string s in map.keysSorted())
            {
                EE e = map.get(s);

                double v = e.vv / e.am;
                if (e.bo.isMul && v == 1)
                    continue;
                if (!e.bo.isMul && v == 0)
                    continue;

                string off = "" + (100 - (int)(100 * e.vv / e.am));

                object[] ss = {
                    e.bo.info.name,
                    String.Format("{0:F2}", e.vv / e.am),
                    e.t.name,
                    (e.bo.isMul ? "*" : "") + e.bo.max(),
                    off
                };

                string sss = String.Format("{0,-20} | {1,9} | {2,-25} | {3,6} | {4,-5}", ss);

                LOG.ln(sss);
            }

            KeyMap<EE> winners = new KeyMap<EE>();
            foreach (Recipe ins in SETT.RECIPES().all())
            {
                if (!winners.containsKey(ins.bo.key))
                    winners.put(ins.bo.key, new EE());
            }

            for (int i = 0; i < 25 * winners.keys().size(); i++)
            {
                Array.Fill(rb, 0);
                SETT.RECIPES().randomizeAIBoosts();
                foreach (FactionNPC f in FACTIONS.NPCs())
                {
                    foreach (Recipe ins in SETT.RECIPES().all())
                    {
                        rb[ins.index] += ins.bo.get(f);

                        winners.get(ins.bo.key).vv += ins.bo.get(f);
                        winners.get(ins.bo.key).a++;
                    }
                }

                double g = double.MaxValue;
                Recipe best = null;

                foreach (Recipe ins in SETT.RECIPES().all())
                {
                    if (best == null || rb[ins.index] < g)
                    {
                        best = ins;
                        g = rb[ins.index];
                    }
                }

                winners.get(best.bo.key).am++;
            }

            foreach (string k in winners.keysSorted())
            {
                object[] ss = {
                    k,
                    winners.get(k).am,
                    String.Format("{0:F2}", winners.get(k).vv / winners.get(k).a),
                };

                string sss = String.Format("{0,-25} | {1,9} | {2,-9}", ss);
                LOG.ln(sss);
            }
        }));
    }

    private class EE
    {
        public Boostable t;
        public Booster bo;
        public double vv;
        public int am;
        public int a;
    }

    public void update(NPCStockpile s, double time)
    {
        for (int i = 0; i < TR.ALL().size(); i++)
        {
            TRADABLE res = TR.ALL().get(i);
            player(res, s, time);
            equalize(res, s, time);
        }
    }

    private void player(TRADABLE res, NPCStockpile s, double time)
    {
        //NPCRes rr = s.res(res);
        //double pam = rr.playerTraded();
        //if (pam == 0)
        //    return;
        //double am = Math.Abs(pam);
        //double max = rr.playerTradeLimit();
        //am -= max * time;
        //am = CLAMP.d(am, 0, double.MaxValue);
        //rr.playerSet(Math.Sign(pam) * am);
    }

    private void equalize(TRADABLE tr, NPCStockpile s, double time)
    {
        NPCRes res = s.res(tr);
        double d = time * res.dailyConsumption();

        consume(s, tr, d);
    }

    private void consume(NPCStockpile s, TRADABLE res, double amount)
    {
        NPCRes rr = s.res(res);
        double pp = Math.Abs(rr.playerTraded() / rr.offset());
        pp = CLAMP.d(pp, 0, 1);
        rr.inc(-amount);
        rr.playerSet(rr.playerTraded() - amount * pp);
        Recipe r = rr.recipe();
        double iam = amount / r.aiRate;
        foreach (RecipeInput i in r.ins)
        {
            double a = i.rate * iam;
            consume(s, i.res, a);
        }
    }
}