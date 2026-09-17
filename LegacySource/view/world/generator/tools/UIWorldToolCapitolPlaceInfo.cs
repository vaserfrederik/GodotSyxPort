using System;
using System.Collections.Generic;
using System.Linq;
using static World.WORLD;
using static Game.Boosting.BoostSpec;
using static Game.Boosting.BoostableCat;
using static Init.Race.Race;
using static Init.Resources.Minable;
using static Init.Resources.RESOURCES;
using static Init.Sprite.UI.UI;
using static Init.Type.CLIMATE;
using static Init.Type.CLIMATES;
using static Init.Type.TERRAIN;
using static Init.Type.TERRAINS;
using Snake2D.PathTile;
using Snake2D.Util.DataTypes;
using Snake2D.Util.Gui;
using Util.Data;
using Util.Gui.Misc;
using Util.Info;
using Util.Text;
using World.Map.Regions.Centre;
using World.Map.Terrain;

namespace View.World.Generator.Tools
{
    public class UIWorldToolCapitolPlaceInfo
    {
        private static readonly string ¤¤moisture = "Moisture";
        private static readonly string ¤¤moistureD = "High natural moisture spots have little need for additional irrigation, but more potential health risks.";

        private static readonly string ¤¤climate = "¤{0} do not prefer climate: {1}. It will be a bit harder to please them.";
        private static readonly string ¤¤isolated = "¤This location is isolated. You will have less trade and less chance to forge an empire and have to rely more on your own production.";
        private static readonly string ¤¤neigh = "¤You will have few of your own species nearby, which might make expanding harder in late game.";

        static UIWorldToolCapitolPlaceInfo()
        {
            D.ts(typeof(UIWorldToolCapitolPlaceInfo));
        }

        private readonly GuiSection s = new GuiSection();
        private readonly WorldTerrainInfo info = new WorldTerrainInfo();
        private readonly WorldTerrainInfo area = new WorldTerrainInfo();

        public UIWorldToolCapitolPlaceInfo()
        {
            int m = 170;

            s.AddDown(0, new GStat()
            {
                public override void Update(GText text)
                {
                    text.Add(WORLD.CLIMATE().Getter.Get(info.Tx, info.Ty).Name);
                }
            }.Hh(CLIMATES.INFO().Name, m));

            s.AddDown(2, new GHeader.HeaderHorizontal(Dic.¤¤Fertility, new GMeter.GMeterSprite(GMeter.C_REDGREEN, info.Fertility(), 64, 12), m));

            s.Body().IncrH(14);

            foreach (TERRAIN t in TERRAINS.ALL())
            {
                s.Add(t.Icon(), 0, s.Body().Y2() - 2);
                s.AddRightC(4, new GText(UI.FONT().S, t.Name).LablifySub());
                s.AddCentredY(new GMeter.GMeterSprite(GMeter.C_ORANGE, info.Get(t), 64, 12), m);
            }

            s.Body().IncrW(48);
        }

        private readonly BOOLEANO<BoostSpec> filter = new BOOLEANO<BoostSpec>()
        {
            public override bool Is(BoostSpec t)
            {
                return (t.Boostable.Cat.TypeMask & BoostableCat.TYPE_SETT) != 0;
            }
        };

        public void PlaceInfo(GBox b, int tx1, int ty1, Race race)
        {
            info.InitCity(tx1, ty1);
            int cx = tx1 + WCentre.TILE_DIM / 2;
            int cy = ty1 + WCentre.TILE_DIM / 2;

            {
                CLIMATE climate = WORLD.CLIMATE().Getter.Get(cx, cy);

                b.TextLL(CLIMATES.INFO().Name);
                b.Tab(6);
                b.Text(WORLD.CLIMATE().Getter.Get(cx, cy).Name);
                b.NL();

                climate.Boosters.Hover(b, 1.0, null, filter, -1);

                b.NL();

                double cl = race.Population().Climate(climate);
                if (cl < race.Population().MaxClimate())
                {
                    GText t = b.Text();
                    t.Add(¤¤climate);
                    t.Insert(0, race.Info.Names);
                    t.Insert(1, climate.Name);
                    t.Warnify();
                    b.Add(t);
                }
                b.Sep();
            }

            {
                foreach (Minable m in RESOURCES.Minables().All())
                {
                    double d = 0;
                    foreach (TERRAIN te in TERRAINS.ALL())
                    {
                        d += info.Get(te).GetD() * m.Terrain(te);
                    }
                    b.Add(m.Resource.Icon());
                    b.Add(GFORMAT.Perc(b.Text(), 4.0 * d));
                }

                b.NL(4);

                b.TextLL(¤¤moisture);
                b.Tab(6);
                b.Add(GFORMAT.PercGood(b.Text(), info.Fertility().GetD()));
                b.NL();
                b.Text(¤¤moistureD);
                b.NL(8);

                double v = 0;
                foreach (TERRAIN te in TERRAINS.ALL())
                {
                    if (info.Get(te).GetD() > 0)
                    {
                        b.TextLL(te.Name);
                        b.Tab(6);
                        b.Add(GFORMAT.PercGood(b.Text(), info.Get(te).GetD()));
                        b.NL();
                        b.Text(te.Desc);
                        b.NL(8);
                    }
                    double t = area.Get(te).GetD() * race.Population().Terrain(te);
                    foreach (CLIMATE c in CLIMATES.ALL())
                        v += t * race.Population().Climate(c) * area.Get(c).GetD();
                }
                v /= (race.Population().MaxClimate() * race.Population().MaxTerrain());

                if (v < 0.25)
                {
                    b.Add(b.Text().Warnify().Add(¤¤neigh));
                }
                b.Sep();
            }

            b.NL(8);

            int size = GetSize(cx, cy, 1500);

            if (size < 750)
            {
                b.Add(b.Text().Warnify().Add(¤¤isolated));
                b.NL(8);
            }
        }

        private int GetSize(int sx, int sy, int max)
        {
            GUTIL.Flooder().Init(this);
            GUTIL.Flooder().PushSloppy(sx, sy, 0);
            area.Clear();
            int size = 0;
            while (GUTIL.Flooder().HasMore() && size < max)
            {
                PathTile t = GUTIL.Flooder().PollSmallest();
                size++;
                area.Add(t.X(), t.Y());
                for (int di = 0; di < DIR.ORTHO.Size; di++)
                {
                    DIR d = DIR.ORTHO.Get(di);
                    int toX = t.X() + d.X();
                    int toY = t.Y() + d.Y();
                    if (!WORLD.IN_BOUNDS(toX, toY))
                        continue;
                    if (WATER().Has.Is(t.X(), t.Y()))
                    {
                        if (!WATER().CanCrossByLand(t.X(), t.Y(), toX, toY))
                            continue;
                    }
                    if (MOUNTAIN().CoversTile(toX, toY))
                        continue;

                    if (FOREST().Amount.Get(t.X(), t.Y()) == 1.0 && FOREST().Amount.Get(toX, toY) == 1)
                        continue;

                    GUTIL.Flooder().PushSmaller(toX, toY, t.GetValue() + d.TileDistance());
                }
            }
            area.Divide(size);
            GUTIL.Flooder().Done();
            return size;
        }
    }
}