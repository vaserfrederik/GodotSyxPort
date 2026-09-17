using System;
using System.Collections.Generic;
using System.IO;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.main.furnisher
{
    public abstract class Furnisher
    {
        public const int MAX_RESOURCES = 4;

        private readonly ArrayList<FurnisherItemTile> tiles = new ArrayList<FurnisherItemTile>(255);
        private readonly ArrayList<FurnisherItem> allItems = new ArrayList<FurnisherItem>(255);
        private readonly ArrayListGrower<FurnisherItemGroup> pgroups = new ArrayListGrower<FurnisherItemGroup>();
        private readonly ArrayListGrower<FurnisherItemGroup> ggroups = new ArrayListGrower<FurnisherItemGroup>();
        private readonly ArrayList<FurnisherStat> stats;
        private readonly LIST<RESOURCE> resources;
        private readonly double[] areaCost;
        protected readonly LIST<Floor> floors;

        public readonly COLOR miniColor;
        private readonly FurnisherMinimapColor colorPimp;

        protected static Json[] jsonGroupText;
        protected static Json[] jsonGroupData;
        protected static Json[] jsonStat;

        protected readonly double[] envValue = new double[SETT.ENV().map.all().size()];
        protected readonly double[] envRadius = new double[SETT.ENV().map.all().size()];

        protected Furnisher(RoomInitData init, int items, int stats) : this(init, items, stats, 0, 0)
        {
        }

        protected Furnisher(RoomInitData init, int items, int stats, int nopA, int nopB)
        {
            tiles.add(null);
            allItems.add(null);
            if (FurnisherItem.itemsTmp.size() != 0)
                throw new Exception("someone forgot to flush...");

            Json data = init.data();
            Json text = init.text();

            resources = RESOURCES.map().readMany(data);
            if (resources.size() > MAX_RESOURCES)
                data.error("Too many resources declared. Max is 4", "RESOURCES");
            areaCost = data.ds("AREA_COSTS", resources.size());
            if (data.has(SETT.FLOOR().map.key))
            {
                if (data.arrayIs(SETT.FLOOR().map.key))
                {
                    floors = SETT.FLOOR().map.readManyWarn(SETT.FLOOR().map.key, data);
                }
                else
                {
                    Floor f = SETT.FLOOR().map.readTry(data);
                    if (f == null)
                        data.error("no floor named: ", SETT.FLOOR().map.key);
                    floors = new ArrayList<Floor>(f);
                }
            }
            else
            {
                floors = null;
            }

            jsonStat = null;
            if (stats > 0)
            {
                jsonStat = text.jsons("STATS", stats);
                if (stats != jsonStat.Length)
                    text.error("Invalid amount of stats declared. Should be " + stats + " not " + jsonStat.Length, "STATS");
            }
            this.stats = new ArrayList<FurnisherStat>(stats);

            jsonGroupText = null;
            jsonGroupData = null;
            if (items > 0)
            {
                jsonGroupData = data.jsons("ITEMS", items);
                jsonGroupText = text.jsons("ITEMS", items);
                if (items != jsonGroupData.Length)
                    data.error("Invalid amount of items declared. Should be " + items + " not " + jsonGroupData.Length, "ITEMS");
            }
            if (items == 0)
                items = 1;

            miniColor = new ColorImp(data, "MINI_COLOR");
            colorPimp = new FurnisherMinimapColor(data);

            if (data.has("ENVIRONMENT_EMIT"))
            {
                Json j = data.json("ENVIRONMENT_EMIT");
                foreach (string k in j.keys())
                {
                    SettEnv e = SETT.ENV().map.rmap.getWarn(k, j);
                    if (e != null)
                    {
                        Json jj = j.json(k);
                        envValue[e.index()] = jj.d("VALUE", 0, 1);
                        envRadius[e.index()] = jj.d("RADIUS", 0, 1);
                    }
                }
            }
        }

        public bool envValue(SettEnv e, SettEnvValue v, int tx, int ty)
        {
            if (envRadius[e.index()] != 0)
            {
                v.radius = envRadius[e.index()];
                v.value = envValue[e.index()];
                return true;
            }
            return false;
        }

        public bool envValue(SettEnv e)
        {
            if (envRadius[e.index()] != 0)
            {
                return true;
            }
            return false;
        }

        public int resources()
        {
            return resources.size();
        }

        public RESOURCE resource(int index)
        {
            return resources.get(index);
        }

        public bool resourceHas(int index, int upgrade)
        {
            return blue().upgrades().resMask(upgrade, index) > 0;
        }

        public double areaCost(int index, int upgrade)
        {
            return areaCost[index] * blue().upgrades().resMask(upgrade, index);
        }

        public double areaCostFlat(int index)
        {
            return areaCost[index];
        }

        public abstract bool usesArea();

        public virtual CharSequence placable(int tx, int ty, FurnisherItem item, FurnisherItemTile tile)
        {
            return null;
        }

        protected FurnisherItemGroup flush(int min, int max, int rots)
        {
            if (rots != 0 && rots != 1 && rots != 3)
                throw new Exception("" + rots);

            FurnisherItemGroup f = new FurnisherItemGroup(
                this, rots,
                jsonGroupText[ggroups.size()].text("NAME"),
                jsonGroupText[ggroups.size()].text("DESC"),
                min,
                max,
                jsonGroupData[ggroups.size()].ds("COSTS", resources.size()),
                jsonGroupData[ggroups.size()].ds("STATS", stats.size())
            );
            ggroups.add(f);
            return f;
        }

        protected void flushSingle(INFO info)
        {
            if (jsonGroupText != null)
                throw new Exception("" + jsonGroupText.Length);
            new FurnisherItemGroup(
                this, 0,
                info.name,
                info.desc,
                0,
                1,
                new double[0],
                new double[0]
            );
        }

        public FurnisherItemGroup flush()
        {
            return flush(1, 1, 0);
        }

        public void put(FurnisherItem item, int x, int y, int rot)
        {
            if (item == null)
                return;

            allItems.add(item);
            item.pos.set(x, y);
            item.rot = rot;
            if (item.tile != null)
                tiles.add(item.tile);
        }

        public void put(FurnisherItem item, int x, int y)
        {
            put(item, x, y, 0);
        }

        public void put(FurnisherItem item)
        {
            put(item, 0, 0, 0);
        }

        public void renderTileBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, bool floored)
        {
        }

        public void renderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
        }

        public void doBeforePlanning(int tx, int ty)
        {
        }

        public bool removeFertility()
        {
            return true;
        }

        public bool removeTerrain(int tx, int ty)
        {
            return !SETT.TERRAIN().NADA.is(tx, ty);
        }

        public COLOR miniColor(int tx, int ty)
        {
            if (colorPimp != null)
                return colorPimp.get(tx, ty);
            return miniColor;
        }

        public COLOR miniColorPimped(ColorImp origional, int tx, int ty, bool northern, bool southern)
        {
            foreach (DIR d in DIR.ORTHO)
            {
                Room r2 = ROOMS().map.get(tx, ty, d);
                if (r2 == null || !r2.isSame(tx + d.x(), ty + d.y(), tx, ty))
                    return origional.shadeSelf(0.8);
            }
            return origional;
        }

        public bool mustBeIndoors()
        {
            return true;
        }

        public Addable overlay()
        {
            return null;
        }

        public bool isHeavy()
        {
            return false;
        }

        public bool needsIsolation()
        {
            return mustBeIndoors() && blue().degradeRate() > 0;
        }

        public bool needFlooring()
        {
            return true;
        }

        public bool mustBeOutdoors()
        {
            return false;
        }

        public abstract Room create(TmpArea area, RoomInit init);

        public abstract RoomBlueprintImp blue();

        public bool canBeCopied()
        {
            return true;
        }

        public void doAfterConstructionInited()
        {
        }

        public FurnisherItem secretReplacementItem(int rot, FurnisherItem origional)
        {
            return null;
        }

        public CharSequence warning(AREA area)
        {
            return null;
        }

        public CharSequence constructionProblem(AREA area)
        {
            return null;
        }

        public void placeInfo(GBox box, FurnisherItem item, int x1, int y1)
        {
        }

        public bool joinsWithFloor()
        {
            return false;
        }

        public bool isSpecialAreaPlacable()
        {
            return false;
        }

        public bool growsGrass(int tx, int ty)
        {
            return false;
        }

        public Diagonalizer dia(int tx, int ty)
        {
            return null;
        }

        public RoomState getConstructionState()
        {
            return null;
        }
    }
}