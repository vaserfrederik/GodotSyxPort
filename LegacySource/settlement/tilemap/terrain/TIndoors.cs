using System;
using System.IO;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.sets;

namespace settlement.tilemap.terrain
{
    public class TIndoors : MAP_BOOLEAN
    {
        private readonly Bitmap2D reservable;
        private int x, y;

        private readonly SAVABLE saver = new SAVABLE()
        {
            Save = file => reservable.Save(file),
            Load = file => reservable.Load(file),
            Clear = () => reservable.Clear()
        };

        public TIndoors()
        {
            reservable = new Bitmap2D(TILE_BOUNDS, false);
        }

        public bool Remove(int tx, int ty)
        {
            if (Is(tx, ty))
            {
                x = tx;
                y = ty;
                if (service.FindableReservedCanBe())
                {
                    service.FindableReserve();
                    return false;
                }
                reservable.Set(tx, ty, false);
                return true;
            }
            reservable.Set(tx, ty, false);
            return false;
        }

        public void Add(int tx, int ty, bool reserved)
        {
            if (Is(tx, ty))
            {
                x = tx;
                y = ty;
                service.FindableReserveCancel();
                if (reserved)
                    service.FindableReserve();
            }
        }

        public FINDABLE Findable(int tx, int ty)
        {
            if (Is(tx, ty))
            {
                x = tx;
                y = ty;
                return service;
            }
            return null;
        }

        private readonly FINDABLE service = new FINDABLE()
        {
            X = () => x,
            Y = () => y,
            FindableReservedCanBe = () => reservable.Is(x + y * TWIDTH),
            FindableReserve = () =>
            {
                if (!FindableReservedCanBe())
                {
                    throw new RuntimeException();
                }

                PATH().Finders.Indoor.Report(this, -1);
                reservable.Set(x + y * TWIDTH, false);
            },
            FindableReservedIs = () => !reservable.Is(x + y * TWIDTH),
            FindableReserveCancel = () =>
            {
                if (FindableReservedIs())
                {
                    reservable.Set(x + y * TWIDTH, true);
                    PATH().Finders.Indoor.Report(this, 1);
                }
            }
        };

        public override bool Is(int tile)
        {
            return TERRAIN().Get(tile).RoofIs() && PATH().Availability.Get(tile).Player > 0;
        }

        public override bool Is(int tx, int ty)
        {
            return TERRAIN().Get(tx, ty).RoofIs() && PATH().Availability.Get(tx, ty).Player > 0;
        }
    }
}