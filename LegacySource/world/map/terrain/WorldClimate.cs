using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.clickable;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data.GETTER;
using util.gui.misc;
using view.subview;
using view.tool;
using world;
using world.map.terrain;

namespace WorldMapTerrain
{
    public sealed class WorldClimate : WorldTerrainResource
    {
        private readonly Bitsmap1D map = new Bitsmap1D(0, CLIMATES.ALL().Count, TAREA());
        private readonly Bitsmap1D offmap = new Bitsmap1D(0, 3, TAREA());

        public WorldClimate()
        {
        }

        private readonly MAP_OBJECTE<CLIMATE> setter = new MAP_OBJECTE<CLIMATE>()
        {
            Get = tile => CLIMATES.ALL()[map.Get(tile)],
            GetXY = (tx, ty) => Get(tx + ty * TWIDTH()),
            Set = (tile, objectValue) =>
            {
                map.Set(tile, objectValue.Index());
                WORLD.changeTile(tile % TWIDTH(), tile / TWIDTH());
            },
            SetXY = (tx, ty, objectValue) =>
            {
                if (IN_BOUNDS(tx, ty))
                    Set(tx + ty * TWIDTH(), objectValue);
            }
        };

        private readonly MAP_DOUBLEE offset = new MAP_DOUBLEE()
        {
            Get = (tx, ty) => IN_BOUNDS(tx, ty) ? Get(tx + ty * TWIDTH()) : 0,
            GetTile = tile => offmap.Get(tile) - 3,
            Set = (tx, ty, value) => IN_BOUNDS(tx, ty) ? Set(tx + ty * TWIDTH(), value) : this,
            SetTile = (tile, value) =>
            {
                int v = (int)(value * 4);
                v = CLAMP.i(v, -3, 4);
                v += 3;
                offmap.Set(tile, v);
                return this;
            }
        };

        public readonly MAP_OBJECT<CLIMATE> getter = setter;

        protected override void Save(FilePutter saveFile)
        {
            map.Save(saveFile);
            offmap.Save(saveFile);
        }

        protected override void Load(FileGetter saveFile)
        {
            map.Load(saveFile);
            offmap.Load(saveFile);
        }

        public override LIST<PLACABLE> Placers(ToolManager tm)
        {
            ArrayListGrower<PLACABLE> placers = new ArrayListGrower<PLACABLE>();
            GETTER_IMP<CLIMATE> pg = new GETTER_IMP<CLIMATE>(CLIMATES.COLD());
            LinkedList<CLICKABLE> butts = new LinkedList<CLICKABLE>();

            foreach (CLIMATE c in CLIMATES.ALL())
            {
                butts.Add(new GButt.ButtPanel(c.Name)
                {
                    ClickA = () => pg.Set(c),
                    RenAction = () => selectedSet(pg.Get() == c)
                });
            }

            placers.Add(new PlacableMulti(CLIMATES.INFO().Name, "", CLIMATES.COLD().Icon)
            {
                Place = (tx, ty, area, type) => setter.Set(tx, ty, pg.Get()),
                IsPlacable = (tx, ty, area, type) => null,
                UpdateRegardless = (window, selected) => WORLD.OVERLAY().climate.Add(),
                GetAdditionalButt = () => butts
            });

            return placers;
        }
    }
}