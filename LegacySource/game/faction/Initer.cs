using System;
using System.Collections.Generic;
using init.paths;
using init.sprite;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using world.region;
using world.region.pop;

namespace game.faction
{
    internal sealed class Initer
    {
        public Initer(LIST<Faction> all)
        {
            SetColors(all);

            RDRace race = RD.RACE(FACTIONS.player().race());
            FACTIONS.player().name.Clear().Add("Jakaton");
            if (race != null)
                FACTIONS.player().name.Clear().Add(race.names.fNames.next());

            {
                Bitmap2D[] datas = BitmapSprite.read(PATHS.SPRITE().getFolder("ui").get("FactionBanners"));

                for (int i = 0; i < datas.Length; i++)
                {
                    int i2 = RND.rInt(datas.Length);
                    Bitmap2D old = datas[i];
                    datas[i] = datas[i2];
                    datas[i2] = old;
                }

                int ki = 0;
                foreach (Faction f in all)
                {
                    f.banner().sprite.paint(datas[ki++]);
                    ki = ki % datas.Length;
                }
            }
        }

        private void SetColors(LIST<Faction> all)
        {
            ArrayList<ColorImp> cols = new ArrayList<ColorImp>(ColorImp.cols(new Json(PATHS.WORLD().init.getFolder("config").gets("Faction")), "COLORS"));
            for (int i = 0; i < cols.size(); i++)
                cols.swap(i, RND.rInt(cols.size()));

            int i = 0;
            foreach (Faction f in all)
            {
                int kk = i / cols.size();
                COLOR col = cols.getC(i);
                if (kk > 0)
                {
                    col = new ColorImp().interpolate(col, cols.getC(i + kk), 0.5);
                }

                f.banner().colorFG().set(cols.getC(i + cols.size() / 2));
                f.banner().colorBG().set(col);
                i++;
            }
        }
    }
}