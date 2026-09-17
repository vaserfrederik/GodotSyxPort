using System;
using System.Collections.Generic;
using settlement.tilemap.terrain;
using snake2d.util.datatypes;
using snake2d.util.sets;
using view.sett;
using view.tool;

class TerrainPlacers
{
    TerrainPlacers(Terrain t, LIST<TerrainTile> tiles)
    {
        foreach (TerrainTile tt in tiles)
        {
            Make(tt);
        }

        new Increaser(t.ROCK)
        {
            public override void Place(int tx, int ty, AREA a, PLACER_TYPE type)
            {
                t.ROCK.AmountIncrease(tx, ty);
            }
        };

        new Decreaser(t.ROCK)
        {
            public override void Place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                t.ROCK.AmountDecrease(tx, ty);
            }
        };
    }

    private void Make(TerrainTile t)
    {
        PLACABLE p = new PlacableMulti(t.GetType().Name + " " + t.Name, null, t.GetIcon(), null)
        {
            public override void Place(int tx, int ty, AREA a, PLACER_TYPE type)
            {
                t.PlaceFixed(tx, ty);
            }

            public override CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE type)
            {
                if (!t.IsPlacable(tx, ty))
                    return PlacableMessages.¤¤BLOCKED;
                return null;
            }
        };

        IDebugPanelSett.Add("terrain", p);
    }

    private abstract class Increaser : PlacableMulti
    {
        private TerrainTile t;

        Increaser(TerrainTile t) : base(t.Name + " increase")
        {
            this.t = t;
            IDebugPanelSett.Add("terrain", this);
        }

        public override CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE type)
        {
            return t.Is(tx, ty) ? null : "";
        }
    }

    private abstract class Decreaser : PlacableMulti
    {
        private TerrainTile t;

        Decreaser(TerrainTile t) : base(t.Name + " decrease")
        {
            this.t = t;
            IDebugPanelSett.Add("terrain", this);
        }

        public override CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE type)
        {
            return t.Is(tx, ty) ? null : "";
        }
    }
}