using System;
using System.Collections.Generic;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.sets;
using view.tool;
using world;

namespace world.map.road
{
    class Placer
    {
        public readonly LinkedList<PLACABLE> placers = new LinkedList<PLACABLE>();

        public Placer()
        {
            PlacableMulti undo = new PlacableMulti("remove", "", UI.icons().m.cancel)
            {
                IsPlacable = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    if (WORLD.ROADS().placable.is(tx, ty))
                    {
                        return null;
                    }
                    return "E";
                },

                Place = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    WORLD.ROADS().set(tx, ty, false);
                }
            };

            PlacableMulti road = new PlacableMulti("road", "", WORLD.BUILDINGS().sprites.roads.makeSprite(0x0F))
            {
                IsPlacable = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    if (WORLD.ROADS().placable.is(tx, ty))
                    {
                        return null;
                    }
                    return "E";
                },

                Place = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    WORLD.ROADS().set(tx, ty, true);
                },

                GetUndo = () => undo
            };

            PlacableMulti harbour = new PlacableMulti("bridge", "", WORLD.BUILDINGS().sprites.harbour.makeSprite(0))
            {
                IsPlacable = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    if (!WORLD.ROADS().canBridge.is(tx, ty))
                        return "E";
                    return null;
                },

                Place = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    WORLD.ROADS().bridge.set(tx, ty, true);
                },

                GetUndo = () => undo
            };

            PlacableMulti roadMiniUndo = new PlacableMulti("magnify", "", WORLD.BUILDINGS().sprites.roads.makeSprite(0x0F).resized(Icon.L).twin(UI.icons().s.arrowUp, DIR.N, 0))
            {
                IsPlacable = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    if (WORLD.ROADS().is(tx, ty))
                    {
                        return null;
                    }
                    return "E";
                },

                Place = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    WORLD.ROADS().minified.set(tx, ty, false);
                }
            };

            PlacableMulti roadMini = new PlacableMulti("minify", "", WORLD.BUILDINGS().sprites.roads.makeSprite(0x0F).resized(Icon.L).twin(UI.icons().s.arrowDown, DIR.S, 0))
            {
                IsPlacable = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    if (WORLD.ROADS().is(tx, ty))
                    {
                        return null;
                    }
                    return "E";
                },

                Place = (int tx, int ty, AREA area, PLACER_TYPE type) =>
                {
                    WORLD.ROADS().minified.set(tx, ty, true);
                },

                GetUndo = () => roadMiniUndo
            };

            placers.Add(road);
            placers.Add(harbour);
            placers.Add(undo);
            placers.Add(roadMini);
            placers.Add(roadMiniUndo);
        }
    }
}