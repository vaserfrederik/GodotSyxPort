using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using game;
using init.sprite;
using init.tech;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;

namespace init.tech
{
    public class TechTree
    {
        public readonly COLOR color;
        public readonly SPRITE icon;
        public readonly TECH[][] nodes;
        public readonly string key;
        public readonly string name;
        public readonly int cat;

        public TechTree(TechCurrencies cc, string key, Json jData, Json jText, LISTE<TECH> all)
        {
            this.key = key;

            color = new ColorImp(jData);
            cat = jData.i("CATEGORY", 0, 5, 0);
            name = jText.text("NAME");
            icon = SPRITES.icons().get(jData);
            Json rows = jData.json("TREE");

            Json techs = jData.has("TECHS") ? jData.json("TECHS") : null;
            Json texts = jText.has("TECHS") ? jText.json("TECHS") : null;

            nodes = new TECH[rows.keys().size()][];
            int ri = 0;

            foreach (var __ in rows.keys())
            {
                string[] values = rows.values(__);
                nodes[ri] = new TECH[values.Length];

                for (int ci = 0; ci < values.Length; ci++)
                {
                    string v = values[ci];
                    if (v.Equals("_____") || v.Equals("______"))
                        continue;
                    Json data;
                    Json text;
                    if (techs != null && techs.has(v))
                    {
                        data = techs.json(v);
                        text = texts.has(v) ? texts.json(v) : null;
                    }
                    else
                    {
                        GAME.Warn(rows.errorGet("there is no tech in the nodes folder named: " + v, v));
                        continue;
                    }
                    TECH t = new TECH(cc, this.key + "_" + v, all, data, text, this, ci, ri);
                    nodes[ri][ci] = t;
                }

                ri++;
            }
        }
    }
}