using System.Collections.Generic;
using init.INIT;
using init.race;
using init.resources;
using init.sprite.UI.Icons.S;
using init.sprite.UI;
using snake2d;
using snake2d.util.sets;
using util.keymap;

namespace init.trade
{
    public class TR
    {
        private readonly ArrayListGrower<TRADABLE> all = new ArrayListGrower<TRADABLE>();
        private readonly ArrayListGrower<TRADABLEO<RESOURCE>> res = new ArrayListGrower<TRADABLEO<RESOURCE>>();
        private readonly ArrayListGrower<TRADABLEO<Race>> slaves = new ArrayListGrower<TRADABLEO<Race>>();
        private readonly RMAPS<TRADABLE> map;
        private static TR s;

        public TR(INIT init)
        {
            s = this;
            foreach (RESOURCE r in RESOURCES.ALL())
            {
                string key = "RES_" + (r.key.StartsWith("_") ? r.key.Substring(1) : r.key);
                int index = all.size();
                SPRITE icon = r.icon();
                TRADABLEO<RESOURCE> tt = new TRADABLEO<RESOURCE>(r, key, index, r, icon);
                res.add(tt);
                all.add(tt);
            }

            foreach (Race r in RACES.all())
            {
                string key = "SLAVE_" + r.key;
                int index = all.size();
                SPRITE icon = new SPRITE.Imp(IconS.M, IconS.M)
                {
                    render = (SPRITE_RENDERER rr, int X1, int X2, int Y1, int Y2) =>
                    {
                        r.appearance().icon.render(rr, X1, X2, Y1, Y2);
                        UI.icons().s.slave.render(rr, X2 - 16, X2, Y2 - 16, Y2);
                    }
                };
                TRADABLEO<Race> tt = new TRADABLEO<Race>(r, key, index, r.info, icon);
                slaves.add(tt);
                all.add(tt);
            }

            map = new RMAPS<TRADABLE>("TRADABLE", all);
        }

        public static LIST<TRADABLE> ALL()
        {
            return s.all;
        }

        public static LIST<TRADABLEO<RESOURCE>> RES()
        {
            return s.res;
        }

        public static LIST<TRADABLEO<Race>> SLAVES()
        {
            return s.slaves;
        }

        public static RMAPS<TRADABLE> MAP()
        {
            return s.map;
        }

        public static TRADABLEO<RESOURCE> get(RESOURCE res)
        {
            return s.res.get(res.index());
        }

        public static TRADABLEO<Race> get(Race res)
        {
            return s.slaves.get(res.index());
        }
    }
}