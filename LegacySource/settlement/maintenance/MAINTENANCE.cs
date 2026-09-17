using System;
using System.IO;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.keymap;
using view.sett;
using view.tool;

namespace settlement.maintenance
{
    public sealed class MAINTENANCE : SettResource
    {
        private readonly Bitmap2D pisser = new Bitmap2D(SETT.TILE_BOUNDS, false);
        private readonly Bitmap2D preserved = new Bitmap2D(SETT.TILE_BOUNDS, false);
        private readonly Bitmap2D pdisabled = new Bitmap2D(SETT.TILE_BOUNDS, false);
        public readonly Bitmap2D pFreeFetch = new Bitmap2D(SETT.TILE_BOUNDS, false);
        private readonly Bitsmap2D bresource = new Bitsmap2D(0, 4, SETT.TILE_BOUNDS);
        public readonly PLACABLE enablePlacer = new PlacerDormant();
        private readonly MConsumption cons = new MConsumption(this);

        public readonly double tilesPerDay = 1.0 / 48.0;
        public readonly double resRate = 1.0 / 64.0;
        public readonly SPRITE icon = UI.icons().s.degrade;

        readonly MType[] types = new MType[]
        {
            new MRoom(),
            new MFloor(),
        };

        public MAINTENANCE() : base("MAINTENANCE", false)
        {
            IDebugPanelSett.Add(new PlacableMulti("MAINTENANCE_DEGRADEx4")
            {
                public override void Place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    Vandalise(tx, ty);
                }

                public override CharSequence IsPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    // TODO Auto-generated method stub
                    return null;
                }
            });

            IDebugPanelSett.Add(new PlacableMulti("MAINTENANCE_DEGRADE_X1")
            {
                public override void Place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    UpdateTileDay(tx, ty, tx + ty * SETT.TWIDTH, 10);
                }

                public override CharSequence IsPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    // TODO Auto-generated method stub
                    return null;
                }
            });

            IDebugPanelSett.Add(new PlacableMulti("MAINTENANCE_DEGRADE")
            {
                public override void Place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    UpdateTileDay(tx, ty, tx + ty * SETT.TWIDTH);
                }

                public override CharSequence IsPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    // TODO Auto-generated method stub
                    return null;
                }
            });

            IDebugPanelSett.Add("MAINTENANCE ana", new ACTION()
            {
                public override void Exe()
                {
                    new Test();
                }
            });
        }

        double sp = 1;
        int upI = -1;

        public void UpdateTileDay(int tx, int ty, int tile)
        {
            if (upI != GAME.UpdateI())
            {
                upI = GAME.UpdateI();
                sp = Speed();
            }
            UpdateTileDay(tx, ty, tile, sp);
        }

        private void UpdateTileDay(int tx, int ty, int tile, double speed)
        {
            foreach (MType t in types)
            {
                if (t.Degrade(tx, ty, tile, speed))
                {
                    if (t.Validate(tx, ty) && !pisser.Is(tx, ty) && t.ShouldPlace(tx, ty, false))
                    {
                        SETT.PATH().finders.maintenance.Remove(tx, ty);
                        pisser.Set(tx, ty, true);
                        bresource.Set(tx, ty, t.ShouldPlaceResource(tx, ty));
                        preserved.Set(tx, ty, false);
                        SETT.PATH().finders.maintenance.Add(tx, ty);
                    }
                    break;
                }
            }
        }

        public void SetChanged(int tx, int ty)
        {
            cons.Change(tx, ty);
        }

        public override void Save(FilePutter file)
        {
            pisser.Save(file);
            preserved.Save(file);
            bresource.Save(file);
            pdisabled.Save(file);
            MAPSAVE.SaveMeta(file, RESOURCES.ALL());
        }

        public override void Load(FileGetter file) => throw new NotImplementedException();
        public override void Clear()
        {
            pisser.Clear();
            preserved.Clear();
            bresource.Clear();
            pdisabled.Clear();
        }

        protected override void AfterTick()
        {
            cons.Update();
        }

        protected override void Init(bool loaded)
        {
            cons.Init();
        }

        public MAP_BOOLEAN needs = new MAP_BOOLEAN()
        {
            public override bool Is(int tx, int ty)
            {
                foreach (MType t in types)
                {
                    if (t.Validate(tx, ty))
                        return true;
                }
                return false;
            }

            public override bool Is(int tile) => Is(tile % SETT.TWIDTH, tile / SETT.THEIGHT);
        };

        public MAP_BOOLEAN isser = new MAP_BOOLEAN()
        {
            public override bool Is(int tx, int ty)
            {
                if (pisser.Is(tx, ty))
                {
                    foreach (MType t in types)
                    {
                        if (t.Validate(tx, ty))
                            return true;
                    }
                    pisser.Set(tx, ty, false);
                }
                return false;
            }

            public override bool Is(int tile) => Is(tile % SETT.TWIDTH, tile / SETT.THEIGHT);
        };

        public MAP_BOOLEANE reserved = new MAP_BOOLEANE()
        {
            public override bool Is(int tx, int ty) => isser.Is(tx, ty) && preserved.Is(tx, ty);

            public override bool Is(int tile) => Is(tile % SETT.TWIDTH, tile / SETT.THEIGHT);

            public override MAP_BOOLEANE Set(int tx, int ty, bool value)
            {
                if (value)
                {
                    preserved.Set(tx, ty, false);
                }
                else
                {
                    pisser.Set(tx, ty, false);
                }
                return this;
            }

            public override MAP_BOOLEANE Set(int tile, bool value) => Set(tile % SETT.TWIDTH, tile / SETT.THEIGHT, value);
        };

        public MAP_BOOLEANE disabled = new MAP_BOOLEANE()
        {
            public override bool Is(int tx, int ty) => pdisabled.Is(tx, ty);

            public override bool Is(int tile) => Is(tile % SETT.TWIDTH, tile / SETT.THEIGHT);

            public override MAP_BOOLEANE Set(int tx, int ty, bool value)
            {
                pdisabled.Set(tx, ty, value);
                return this;
            }

            public override MAP_BOOLEANE Set(int tile, bool value) => Set(tile % SETT.TWIDTH, tile / SETT.THEIGHT, value);
        };

        public final MAP_DOUBLE degrade = new MAP_DOUBLE()
        {
            public override double Get(int tx, int ty)
            {
                foreach (MType t in types)
                {
                    if (t.Validate(tx, ty))
                        t.Degrade(tx, ty);
                }
                return 0;
            }

            public override double Get(int tile) => Get(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
        };

        public MAP_OBJECT<RESOURCE> resource = new MAP_OBJECT<RESOURCE>()
        {
            public override RESOURCE Get(int tile) => Get(tile % SETT.TWIDTH, tile / SETT.THEIGHT);

            public override RESOURCE Get(int tx, int ty)
            {
                if (pisser.Is(tx, ty))
                {
                    int bi = bresource.Get(tx, ty);
                    if (bi == 0)
                        return null;
                    foreach (MType t in types)
                    {
                        if (t.Validate(tx, ty))
                            return t.Res(tx, ty, bi);
                    }
                    pisser.Set(tx, ty, false);
                }
                return null;
            }
        };

        public void Vandalise(int tx, int ty)
        {
            foreach (MType t in types)
            {
                if (t.Validate(tx, ty))
                {
                    t.Vandalize(tx, ty);
                    if (!pisser.Is(tx, ty) && t.ShouldPlace(tx, ty, false))
                    {
                        SETT.PATH().finders.maintenance.Remove(tx, ty);
                        pisser.Set(tx, ty, true);
                        preserved.Set(tx, ty, false);
                        bresource.Set(tx, ty, t.ShouldPlaceResource(tx, ty));
                        SETT.PATH().finders.maintenance.Add(tx, ty);
                    }
                    break;
                }
            }
        }

        public void Maintain(int tx, int ty)
        {
            foreach (MType t in types)
            {
                if (t.Validate(tx, ty))
                {
                    t.Maintain(tx, ty);
                    SETT.PATH().finders.maintenance.Remove(tx, ty);
                    pisser.Set(tx, ty, false);
                    if (t.ShouldPlace(tx, ty, true))
                    {
                        pisser.Set(tx, ty, true);
                        bresource.Set(tx, ty, t.ShouldPlaceResource(tx, ty));
                        preserved.Set(tx, ty, false);
                    }
                    SETT.PATH().finders.maintenance.Add(tx, ty);
                    return;
                }
            }
        }

        public void InitRoomDegrade(Room room, int mX, int mY)
        {
            MRoom.InitRoom(room, mX, mY);
        }

        public double EstimateGlobal(RESOURCE res)
        {
            return cons.Get(res) * Speed();
        }

        public double EstimateGlobalRaw(RESOURCE res)
        {
            return cons.Get(res);
        }

        public double Speed()
        {
            double m = BOOSTABLES.CIVICS().MAINTENANCE.Get(HCLASS_RACE.clP(null, null));
            if (m <= 0)
                return 10;
            return CLAMP.d(1.0 / (m), 0, 10);
        }
    }
}