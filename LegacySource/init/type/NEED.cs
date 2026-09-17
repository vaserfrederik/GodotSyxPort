using System;
using System.Collections.Generic;
using game.boosting;
using init.paths.PATHS;
using init.sprite.UI;
using settlement.stats;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.keymap;
using util.text;

namespace init.type
{
    public class NEED : MAPPED
    {
        private static readonly string ¤¤rateD = "The rate at which the need of {0} increases daily.";
        static
        {
            D.ts(typeof(NEED));
        }

        public readonly string nameNeed;
        public readonly string key;
        public readonly double event;
        public readonly Boostable rate;
        private readonly int index;
        public readonly bool basic;

        public NEED(string key, ResFolder f, LISTE<NEED> all, BoostableCat cat, SPRITE icon, bool basic)
        {
            this.index = all.add(this);
            this.key = key;
            Json jt = new Json(f.text.gets(key));
            Json jd = new Json(f.init.gets(key));
            this.nameNeed = jt.text("NAME_NEED");
            this.basic = basic;
            this.event = jd.dTry("EVENT", 0, 10000, 0);
            if (icon == null)
                icon = UI.icons().s.clock;
            final SPRITE ii = icon;
            SPRITE ico = new SPRITE.Imp(Icon.S)
            {
                public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    LIST<StatService> g = STATS.SERVICE().perNeed(this);
                    if (g != null && g.size() > 0)
                        g.get(0).icon.render(r, X1, X2, Y1, Y2);
                    else
                        ii.render(r, X1, X2, Y1, Y2);
                }
            };

            this.rate = BOOSTING.push(key, jd.d("RATE"), jt.text("NAME_RATE"), "" + Str.TMP.clear().add(¤¤rateD).insert(0, nameNeed), ico, cat);
        }

        public override int index()
        {
            return index;
        }

        public override string ToString()
        {
            return key;
        }

        public LIST<StatService> sGroup()
        {
            return STATS.SERVICE().perNeed(this);
        }

        public override string key()
        {
            return key;
        }
    }
}