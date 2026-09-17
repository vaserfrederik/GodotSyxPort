using System;
using System.Collections.Generic;
using System.Linq;
using init.sprite.UI;
using init.type;
using settlement.room.main.util;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.info;
using util.text;

public sealed class RoomUpgrades
{
    private readonly int upgrades;
    private readonly double[][] masks;
    private readonly double[] boosts;
    public readonly ArrayListGrower<Lockable<Faction>> reqs = new ArrayListGrower<Lockable<Faction>>();
    private readonly string[] texts;
    private static readonly COLOR ORANGE100 = new ColorImp(127, 100, 0);

    public RoomUpgrades(RoomBlueprintImp blue, RoomInitData init)
    {
        double ai = 0;
        if (init.data().Has("UPGRADES"))
        {
            Json[] jj = init.data().Jsons("UPGRADES", 1);
            masks = new double[jj.Length][];
            boosts = new double[jj.Length];

            upgrades = jj.Length;
            int i = 0;
            int ll = 0;
            foreach (var j in jj)
            {
                double[] mask = j.Ds("RESOURCE_MASK");

                ll = Math.Max(ll, mask.Length);
                double b = j.D("BOOST");
                masks[i] = mask;

                boosts[i] = b;
                if (j.Has("AI"))
                {
                    ai = Math.Max(ai, j.D("AI", 0, 10000));
                }
                else
                {
                    ai = Math.Max(ai, b * 0.5);
                }

                i++;
            }

            for (i = 0; i < boosts.Length; i++)
            {
                if (masks[i].Length < ll)
                {
                    double[] nn = new double[ll];
                    nn.Fill(1);
                    for (int k = 0; k < masks[i].Length; k++)
                        nn[k] = masks[i][k];
                    masks[i] = nn;
                }
            }
            texts = init.text().TextsTry("UPGRADES").ToArray();
            //LOG.ln(blue.key + " " + max());
        }
        else
        {
            upgrades = 1;
            masks = new double[][]
            {
                new double[] { 1 }
            };
            boosts = new double[]
            {
                1
            };
            texts = Array.Empty<string>();
        }

        for (int i = 1; i <= max(); i++)
        {
            final int upAm = i;

            SPRITE icon = new SPRITE.Imp(Icon.L, Icon.L)
            {
                public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    blue.icon.render(r, X1, X2, Y1, Y2);
                    int size = Icon.S * (X2 - X1) / Icon.L;

                    UI.icons().s.chevron(DIR.N);

                    COLOR.BLACK.bind();
                    OPACITY.O66.bind();
                    int sh = size / 8;
                    for (int j = 0; j < upAm; j++)
                        UI.icons().s.chevron(DIR.N).render(r, X1 + sh, X1 + size + sh, Y1 + sh + j * size / 2, Y1 + sh + j * size / 2 + size);
                    OPACITY.unbind();
                    COLOR.unbind();
                    //GCOLOR.T().bronzeGold((upAm-1)/(upgrades-1)).bind();

                    ORANGE100.bind();
                    for (int j = 0; j < upAm; j++)
                        UI.icons().s.chevron(DIR.N).render(r, X1, X1 + size, Y1 + j * size / 2, Y1 + j * size / 2 + size);
                    COLOR.unbind();
                    COLOR.unbind();
                    size = Icon.M * (X2 - X1) / (Icon.L * 2);

                    for (int ri = 0; ri < blue.constructor().resources(); ri++)
                    {
                        if (masks[upAm][ri] > 0 && masks[upAm - 1][ri] == 0)
                        {
                            blue.constructor().resource(ri).icon().render(r, X2 - size, X2, Y2 - size, Y2);
                            Y2 -= size * 0.75;
                        }
                    }
                }
            };

            reqs.Add(GVALUES.FACTION.LOCK.push("ROOM_" + init.key() + "_UPGRADE_" + i, blue.info.name + " (" + Dic.¤¤Upgrade + " " + GFORMAT.toNumeral(i) + ")", "", icon));
        }
    }

    public void pushBonus(RoomBlueprintIns<?> blue, Boostable bo)
    {
        if (max() <= 0)
            return;

        double from = boost(0);
        double to = boost(max());
        BSourceInfo in = new BSourceInfo(Dic.¤¤Upgrade, UI.icons().s.chevron(DIR.N));
        Booster bos = new BoosterImp(in, from, to, false)
        {
            public double get(BOOSTABLE_O o)
            {
                if (o is FactionNPC)
                {
                    return 0;
                }
                return o.boostableValue(this);

                //double d = o.boostableValue(this);
                //d = CLAMP.d(d, 0, RoomUpgrades.this.max());
                //int di = (int)d;
                //d -= di;
                //double res = RoomUpgrades.this.boost(di) * (1.0 - d);
                //if (di < RoomUpgrades.this.max())
                //    res += RoomUpgrades.this.boost(di + 1) * d;
                //return res;
            }

            public double vGet(Induvidual indu)
            {
                return get(STATS.WORK().EMPLOYED.get(indu));
            }

            private int ci = -120;
            private double c = 0;

            public double vGet(Player f)
            {
                return vGet(HCLASS_RACE.clP());
            }

            private double get(RoomInstance ins)
            {
                if (ins != null && ins.blueprint() == blue)
                {
                    return ins.blueprintI().upgrades().boost(ins.upgrade());
                }
                return 0;
            }

            public double vGet(HCLASS_RACE popTime)
            {
                if (Math.Abs(GAME.updateI() - ci) >= 120)
                {
                    ci = GAME.updateI();
                    c = 0;
                    int am = 0;
                    for (int i = 0; i < blue.instancesSize(); i++)
                    {
                        RoomInstance ins = blue.getInstance(i);
                        int e = ins.employees().employed();
                        c += e * get(ins);
                        am += e;
                    }

                    if (am != 0)
                    {
                        c /= am;
                    }
                }

                return c;
            }

            public double vGet(FactionNPC f)
            {
                //double d = f.bonus.get(blue.index());
                //double eff = aiEff * d;
                //return eff;
                return 0;
            }

            public double vGet(Faction f)
            {
                return 0;
            }
        };
        bos.add(bo);
    }

    public int max()
    {
        return upgrades - 1;
    }

    public double resMask(int upgrade, int ri)
    {
        upgrade = CLAMP.i(upgrade, 0, max());
        ri = CLAMP.i(ri, 0, masks[upgrade].Length - 1);
        return masks[upgrade][ri];
    }

    public double boost(int upgrade)
    {
        return boosts[CLAMP.i(upgrade, 0, boosts.Length - 1)];
    }

    public double upD(RoomInstance room)
    {
        return (1.0 + room.upgrade()) / (max() + 1.0);
    }

    public Lockable<Faction> requires(int upgrade)
    {
        return reqs.Get(upgrade - 1);
    }

    public string desc(int upgrade)
    {
        if (upgrade > 0 && upgrade - 1 < texts.Length)
        {
            return texts[upgrade - 1];
        }
        return null;
    }
}