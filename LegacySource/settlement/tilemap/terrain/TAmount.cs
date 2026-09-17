using System;
using snake2d.util.datatypes;
using snake2d.util.map;
using view.sett;
using view.tool;

namespace settlement.tilemap.terrain
{
    public abstract class TAmount : MAP_INTE
    {
        public readonly int max;
        private readonly double maxI;

        protected TAmount(int max, string name)
        {
            this.max = max;
            this.maxI = 1.0 / max;

            PlacableMulti undo = new PlacableMulti(name + " decrease")
            {
                Place = (tx, ty, area, type) => increment(tx, ty, -1),
                IsPlacable = (tx, ty, area, type) => get(tx, ty) > 0 ? null : "E"
            };

            PlacableMulti place = new PlacableMulti(name + " increase")
            {
                Place = (tx, ty, area, type) => increment(tx, ty, 1),
                IsPlacable = (tx, ty, area, type) => get(tx, ty) < max ? null : "E",
                GetUndo = () => undo
            };

            IDebugPanelSett.Add(place);
        }

        public override int get(int tx, int ty)
        {
            if (SETT.IN_BOUNDS(tx, ty))
            {
                return get(tx + ty * SETT.TWIDTH);
            }
            return 0;
        }

        public override MAP_INTE set(int tx, int ty, int value)
        {
            if (SETT.IN_BOUNDS(tx, ty))
            {
                return set(tx + ty * SETT.TWIDTH, value);
            }
            return this;
        }

        public readonly MAP_DOUBLEE DM = new MAP_DOUBLEE()
        {
            get = (tx, ty) => TAmount.this.get(tx, ty) * maxI,
            getTile = tile => TAmount.this.get(tile) * maxI,
            set = (tx, ty, value) =>
            {
                TAmount.this.set(tx, ty, (int)(value * max));
                return this;
            },
            setTile = (tile, value) =>
            {
                TAmount.this.set(tile, (int)(value * max));
                return this;
            }
        };

        protected abstract void increment(int tx, int ty, int amount);
        protected abstract int get(int index);
        protected abstract MAP_INTE set(int index, int value);
    }
}