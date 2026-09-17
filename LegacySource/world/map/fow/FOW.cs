using System;
using System.Collections.Generic;
using System.IO;
using game.debug;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using init.constant;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.sets;
using util;
using util.data;
using util.rendering;
using view.world.panel;
using world;
using world.map.regions;
using world.region;

namespace world.map.fow
{
    public sealed class FOW : WorldResource, MAP_BOOLEAN
    {
        private readonly Bitmap2D tmp = new Bitmap2D(WORLD.TBOUNDS(), false);
        private readonly Bitmap2D visible = new Bitmap2D(WORLD.TBOUNDS(), false);

        public BOOLEANImp toggled = new BOOLEANImp(true)
        {
            public override BOOLEANImp Set(bool b)
            {
                dirty = true;
                base.Set(b);
                return this;
            }
        };
        private bool dirty = true;

        public FOW()
            : base("Fog of War", "FOW")
        {
            IDebugPanelWorld.Add("toggle fow", toggled);
        }

        public void SetDirty()
        {
            dirty = true;
        }

        void Update()
        {
            visible.Clear();

            Flooder f = GUTIL.Flooder();
            f.Init(this);

            for (int i = 0; i < FACTIONS.Player().Realm().Regions(); i++)
            {
                Region reg = FACTIONS.Player().Realm().Region(i);
                f.PushSloppy(reg.Cx(), reg.Cy(), 0);
            }

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();

                visible.Set(t, true);

                Region from = WORLD.REGIONS().Map.Get(t);
                foreach (DIR d in DIR.ALL)
                {
                    if (from == null && WORLD.REGIONS().Map.Get(t, d) == null)
                        visible.Set(t, d, true);
                    else if (from != null && from.Is(t, d))
                        visible.Set(t, d, true);
                    if (WORLD.PATH().Map.Can(t, d))
                    {
                        Region to = WORLD.REGIONS().Map.Get(t, d);
                        if (to == null || from == null || from == to || from.Faction() == FACTIONS.Player() || (from.Faction() != null && DIP.Get((FactionNPC)from.Faction()).Transit))
                            GUTIL.Flooder().PushSmaller(t, d, t.Value() + d.TileDistance());
                    }
                }
            }

            f.Done();
        }

        public void Render(WRenContext data)
        {
            if (!toggled.B)
                return;
            CORE.Renderer().ShadowDepthSet((byte)255);
            WORLD.CENTRE().Sprite.RenderAboveTerrain(data);
            RenderIterator it = data.Data.OnScreenTiles(0, 0, 0, 0);

            while (it.Has())
            {
                Render(data, it);
                it.Next();
            }
        }

        public void Render(WRenContext con, RenderIterator it)
        {
            if (!Is(it.Tile()))
                return;
            if (WORLD.REGIONS().CentreTile().Is(it.Tile()) && Is(it.Tile()))
            {
                CORE.Renderer().ShadowDepthSet((byte)127);
            }
            else
            {
                CORE.Renderer().ShadowDepthSet((byte)255);
            }
            CORE.Renderer().RenderShadow(it.X(), it.X() + C.TILE_SIZE, it.Y(), it.Y() + C.TILE_SIZE, COLOR.WHITE100.Texture(), (byte)0);
            tmp.Set(it.Tile(), false);
        }

        public void Enlighten(int tx, int ty, int radius)
        {
            if (!toggled.B)
                return;
            for (int i = 0; GUTIL.Circle().Radius(i) <= radius; i++)
            {
                tmp.Set(tx + GUTIL.Circle().Get(i).X(), ty + GUTIL.Circle().Get(i).Y(), true);
            }
        }

        public bool Is(int tile)
        {
            if (!toggled.B)
                return false;
            if (tmp.Is(tile))
                return false;
            if (visible.Is(tile))
                return false;
            Region reg = WORLD.REGIONS().Map.Get(tile);
            if (reg != null && RD.DIST().Reachable(reg))
                return false;
            if (reg != null && reg.Faction() == FACTIONS.Player())
                return false;
            return true;
        }

        public bool Is(int tx, int ty)
        {
            return Is(tx + ty * WORLD.TWIDTH());
        }

        protected override void Update(double ds, Profiler prof)
        {
            prof.LogStart(this);
            if (FACTIONS.Player().CapitolRegion() == null || !SETT.Exists())
                return;
            if (dirty)
                Update();
            dirty = false;
            base.Update(ds, prof);
            prof.LogEnd(this);
        }

        private readonly WorldResourceManager saver = new WorldResourceManager()
        {
            public override void Save(FilePutter file)
            {
                // TODO Auto-generated method stub
            }

            public override void Load(FileGetter file)
            {
                dirty = true;
            }

            public override void Clear()
            {
                dirty = true;
            }
        };

        public override WorldResourceManager Saver()
        {
            return saver;
        }

        protected override void InitBeforePlay()
        {
            dirty = true;
        }
    }
}