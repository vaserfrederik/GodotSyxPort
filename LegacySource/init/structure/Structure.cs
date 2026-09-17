using System;
using System.Collections.Generic;
using game.faction.player;
using init.constant;
using init.resources;
using settlement.main;
using settlement.tilemap.terrain;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sets;
using util.info;
using util.keymap;
using util.text;

namespace init.structure
{
    public sealed class Structure : INFO, MAPPED
    {
        public readonly string key;
        public readonly string nameWall;
        public readonly string nameCeiling;
        public readonly double durability;
        public readonly RESOURCE resource;
        public readonly int resAmount;
        public readonly PlayerColor tint;
        public readonly COLOR miniColor;
        private readonly int index;

        public readonly double constructTime;

        public Structure(string key, LISTE<Structure> all, Json data, Json text)
            : base(text)
        {
            this.key = key;
            nameWall = text.text("NAME_WALL");
            nameCeiling = text.text("NAME_CEILING");

            constructTime = data.d("BUILD_TIME", 0, 10000);
            durability = data.d("DURABILITY", 0, 1.0) * C.TILE_SIZE;
            if (data.has("RESOURCE"))
            {
                resource = RESOURCES.map().read(data);
                resAmount = data.i("RESOURCE_AMOUNT", 0, 16);
            }
            else
            {
                resource = null;
                resAmount = 0;
            }

            tint = new PlayerColor(new ColorImp(data), "BUILDING_" + key, Dic.¤¤Structures, name);
            miniColor = new ColorImp(data, "MINIMAP_COLOR");

            index = all.add(this);
        }

        public override int index()
        {
            return index;
        }

        public override string key()
        {
            return key;
        }

        public TBuilding terrain()
        {
            return SETT.TERRAIN().BUILDINGS.get(this);
        }
    }
}