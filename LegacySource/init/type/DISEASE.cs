using System;
using System.Collections.Generic;
using settlement.main;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.keymap;
using util.text;

public sealed class DISEASE : MAPPED
{
    private readonly int index;
    public readonly INFO info;
    public readonly string key;

    public readonly double infectRate;
    public readonly double incubationDays;
    public readonly double fatalityRate;
    private readonly double[] coccurence;
    private readonly double[] toccurence;
    public readonly int length;
    public readonly COLOR color;
    public readonly bool epidemic;
    public readonly bool regular;

    private static readonly CharSequence ¤¤Spread = "Spread";
    private static readonly CharSequence ¤¤Occurrence = "Occurrence";
    private static readonly CharSequence ¤¤Incubation = "Incubation Days";
    private static readonly CharSequence ¤¤Length = "Infection Days";
    private static readonly CharSequence ¤¤Lethality = "lethality";

    static
    {
        D.ts(typeof(DISEASE));
    }

    public DISEASE(LISTE<DISEASE> all, string key, Json data, Json text)
    {
        index = all.Add(this);
        this.key = key;
        info = new INFO(text);

        infectRate = data.d("SPREAD", 0, 1);
        incubationDays = data.i("INCUBATION_DAYS", 1, 100);
        fatalityRate = data.d("FATALITY_RATE", 0, 1.0);
        coccurence = new double[CLIMATES.ALL().Count];
        toccurence = new double[TERRAINS.ALL().Count];
        CLIMATES.MAP().readFill("OCCURRENCE_CLIMATE", coccurence, data, 0.00000, 100000);
        TERRAINS.MAP().readFill("OCCURRENCE_TERRAIN", toccurence, data, 0.00000, 100000);
        length = data.i("INFECTION_DAYS", 1, 100);
        epidemic = data.bool("EPIDEMIC");
        regular = data.bool("REGULAR");
        color = new ColorImp(data).shade(2.0);
    }

    public override int index()
    {
        return index;
    }

    public override string ToString()
    {
        return key;
    }

    public void hover(GUI_BOX text)
    {
        GBox b = (GBox)text;
        info.hover(b);

        {
            double occ = 0;

            b.NL();
            b.textLL(¤¤Occurrence);
            b.NL();
            int tt = 0;

            foreach (TERRAIN t in TERRAINS.ALL())
            {
                occ += toccurence[t.index()] * SETT.WORLD_AREA().info.get(t).getD();
                if (tt > 6)
                {
                    tt = 0;
                    b.NL();
                }
                b.add(t.icon());
                b.add(GFORMAT.f0(b.text(), toccurence[t.index()]));
            }
            b.NL();
            CLIMATE climate = SETT.ENV().climate();
            occ *= coccurence[climate.index()];
            b.textLL(CLIMATES.INFO().name);
            b.tab(6);
            b.add(GFORMAT.f0(b.text(), coccurence[climate.index()]));
            b.NL();
            b.textSLL(Dic.¤¤Total);
            b.tab(6);
            b.add(GFORMAT.f0(b.text(), occ));

            b.sep();
        }

        b.NL();
        b.textLL(¤¤Spread);
        b.tab(6);
        b.add(GFORMAT.percInv(b.text(), infectRate));
        b.NL();

        b.textLL(¤¤Incubation);
        b.tab(6);
        b.add(GFORMAT.f(b.text(), incubationDays, 1));
        b.NL();

        b.textLL(¤¤Length);
        b.tab(6);
        b.add(GFORMAT.f(b.text(), length, 1));
        b.NL();

        b.NL(8);
        b.textLL(¤¤Lethality);
        b.tab(6);
        b.add(GFORMAT.percInv(b.text(), fatalityRate));

        b.sep();
    }

    public double occurence()
    {
        double occ = 0;

        foreach (TERRAIN t in TERRAINS.ALL())
        {
            occ = Math.Max(occ, toccurence[t.index()] * SETT.WORLD_AREA().info.get(t).getD());
        }
        CLIMATE climate = SETT.ENV().climate();
        occ *= coccurence[climate.index()];
        return occ;
    }

    public override string key()
    {
        return key;
    }
}