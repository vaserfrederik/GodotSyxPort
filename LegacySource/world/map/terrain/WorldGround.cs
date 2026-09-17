using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using world.WORLD;
using init.constant.Config;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.clickable;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.gui.panel;
using util.gui.table;
using util.io;
using util.math;
using util.text;

namespace world
{
    public class WorldGround : WorldTile
    {
        private Bits rotData;
        private Bits ids;
        private List<WGROUND> all;
        private WGROUND patchedGrass;

        public WorldGround()
        {
            rotData = new Bits(TWIDTH() * TLENGTH(), 4);
            ids = new Bits(TWIDTH() * TLENGTH(), 4);
            all = new List<WGROUND>();
            patchedGrass = null;

            // Load data from file or initialize with default values
            LoadData();
        }

        private void LoadData()
        {
            // Implement file loading logic here
        }

        private void SaveData()
        {
            // Implement file saving logic here
        }

        public override void ChangeTile(int tx, int ty)
        {
            base.ChangeTile(tx, ty);
            SaveData();
        }

        public override MAP_OBJECT<WGROUND> Getter
        {
            get
            {
                return new MAP_OBJECT<WGROUND>
                {
                    Get = (tx, ty) =>
                    {
                        if (!IN_BOUNDS(tx, ty))
                            return null;
                        return all[ids.Get(tx + ty * TWIDTH())];
                    },
                    GetTile = tile =>
                    {
                        return all[ids.Get(tile)];
                    }
                };
            }
        }

        public class WGROUND : PlacableMulti
        {
            private double moisture;
            private int code;
            private int bg;
            private COLOR col;
            private COLOR[] cdeva;
            private ColorImp colImp = new ColorImp();
            private TILE_SHEET over;

            public WGROUND(List<WGROUND> all, string name, double moisture, int bg, COLOR col, TILE_SHEET sheet, COLOR deva) : base(name)
            {
                code = all.Count;
                this.moisture = moisture;
                this.bg = bg;
                this.col = col;
                this.cdeva = new COLOR[8];
                for (int i = 0; i < cdeva.Length; i++)
                {
                    cdeva[i] = new ColorImp().Interpolate(col, deva, (i + 1.0) / cdeva.Length);
                }
                over = sheet;
                all.Add(this);
            }

            public override void Place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                if (!IN_BOUNDS(tx, ty))
                    return;
                WGROUND old = Getter.Get(tx, ty);
                Place(tx, ty);
                if (old != Getter.Get(tx, ty))
                {
                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (IN_BOUNDS(tx, ty, d))
                        {
                            Getter.Get(tx + d.X, ty + d.Y).Place(tx + d.X, ty + d.Y, area, type);
                        }
                    }
                }
            }

            public override CharSequence IsPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                return null;
            }

            public override SPRITE GetIcon()
            {
                return SPRITES.icons().m.cancel;
            }

            public bool Is(int tx, int ty)
            {
                return IN_BOUNDS(tx, ty) && code == ids.Get(tx + ty * TWIDTH());
            }

            public double Moisture()
            {
                return moisture;
            }

            public WGROUND Fallback()
            {
                return patchedGrass;
            }
        }

        public override LIST<PLACABLE> Placers(ToolManager tm)
        {
            ArrayListGrower<PLACABLE> pp = new ArrayListGrower<>();

            pp.Add(new PlacableMulti(Dic.¤¤ground)
            {
                G = all[0],
                Butts = new LinkedList<CLICKABLE>()
                {
                    // Add buttons for each ground type
                }
            });

            return pp;
        }
    }
}