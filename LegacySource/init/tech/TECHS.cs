using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using game;
using init.paths;
using init.tech.TECH;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using util.info;
using util.text;

public static class TECHS
{
    private static LIST<TECH> ALL;
    private static LIST<TechCurrency> costs;
    public static CharSequence ¤¤name = "Technology";
    private static CharSequence ¤¤desc = "Technologies unlocks various boosts and rooms.";
    static { D.ts(TECHS); }
    private static readonly INFO info = new INFO(¤¤name, ¤¤desc);

    private static LIST<TechTree> trees;

    public static LIST<TechTree> TREES()
    {
        return trees;
    }

    public static LIST<TECH> ALL()
    {
        return ALL;
    }

    public static LIST<TechCurrency> COSTS()
    {
        return costs;
    }

    public static INFO INFO()
    {
        return info;
    }

    public TECHS() throws IOException
    {
        TechCurrencies cc = new TechCurrencies();

        KeyMap<TECH> map = new KeyMap<TECH>();
        LinkedList<TECH> all = new LinkedList<TECH>();

        {
            ArrayListGrower<TechTree> trees = new ArrayListGrower<TechTree>();
            ResFolder data = new ResFolder("tech", false);

            foreach (string key in data.init.getFiles())
            {
                Json dd = new Json(data.init.gets(key));
                Json tt = new Json(data.text.gets(key));
                trees.add(new TechTree(cc, key, dd, tt, all));
            }

            ArrayList<TechTree> li = new ArrayList<TechTree>(trees);
            li.sort((o1, o2) => o1.cat - o2.cat);
            TECHS.trees = li;
        }

        costs = cc.all;

        ALL = new ArrayList<TECH>(all);

        foreach (TECH t in ALL)
        {
            if (map.ContainsKey(t.key))
            {
                throw new Errors.DataError(t.key + " is more than once in the tree!");
            }
            map.Put(t.key, t);
        }

        foreach (TECH tech in ALL)
        {
            Json j = tech.requiresTech;
            tech.requiresTech = null;
            LinkedList<TechRequirement> needs = new LinkedList<TechRequirement>();
            if (j.Has("REQUIRES_TECH_LEVEL"))
            {
                Json jj = j.Json("REQUIRES_TECH_LEVEL");
                foreach (string k in jj.Keys())
                {
                    string kk = k;
                    if (!map.ContainsKey(k))
                    {
                        if (tech.tree != null)
                            k = tech.tree.key + "_" + k;

                        if (!map.ContainsKey(k))
                        {
                            GAME.Warn(jj.errorGet(k, "REQUIRES_TECH_LEVEL"));
                            continue;
                        }
                    }
                    TechRequirement t = new TechRequirement(map.Get(k), jj.i(kk, 0, map.Get(k).levelMax));
                    needs.Add(t);
                }
            }
            tech.Set(new ArrayList<TechRequirement>(needs));
            tech.prune(new ArrayList<TechRequirement>(needs));
        }

        detectCycles();

        {
            int[] reqed = Alloc.ii(ALL.size());

            for (int i = 0; i < ALL.size(); i++)
            {
                TECH t = ALL.get(i);
                Array.Fill(reqed, 0);
                fillRequirements(reqed, t);

                LinkedList<TechRequirement> needs = new LinkedList<TechRequirement>();
                for (int ri = 0; ri < ALL.size(); ri++)
                {
                    if (reqed[ri] > 0)
                    {
                        TechRequirement tt = new TechRequirement(ALL.get(ri), reqed[ri] - 1);
                        needs.Add(tt);
                    }
                }

                t.Set(new ArrayList<TechRequirement>(needs));
            }
        }
    }

    private void fillRequirements(int[] reqed, TECH t)
    {
        for (int i = 0; i < t.requires().size(); i++)
        {
            TechRequirement r = t.requires().get(i);
            reqed[r.tech.index()] = Math.Max(reqed[r.tech.index()], r.level + 1);
            fillRequirements(reqed, r.tech);
        }
    }

    private void detectCycles()
    {
        bool[] checked = new bool[ALL.size()];

        for (int i = 0; i < ALL.size(); i++)
        {
            if (ALL.get(i).requires().size() == 0)
                continue;
            Array.Fill(checked, false);
            detectCycles(ALL.get(i), checked);
        }
    }

    private void detectCycles(TECH tech, bool[] checked)
    {
        checked[tech.index()] = true;

        for (int i = 0; i < tech.requires().size(); i++)
        {
            TECH t = tech.requires().get(i).tech;
            if (checked[t.index()])
                throw new Errors.DataError("tech: " + t.key + " has a cyclic requirement", "");
        }

        for (int i = 0; i < tech.requires().size(); i++)
        {
            TECH t = tech.requires().get(i).tech;
            detectCycles(t, checked.ToArray());
        }
    }
}