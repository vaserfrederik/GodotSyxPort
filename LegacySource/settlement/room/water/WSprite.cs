using System;
using System.IO;
using System.Runtime.CompilerServices;
using game.time;
using init.constant;
using init.sprite;
using settlement.main;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.tilemap.ground;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.map;
using snake2d.util.sprite;
using util.rendering;
using util.spritecomposer;
using util.spritecomposer.ComposerDests;
using util.spritecomposer.ComposerSources;
using util.spritecomposer.ComposerThings;
using util.spritecomposer.ComposerUtil;

namespace settlement.room.water
{
    public sealed class WSprite
    {
        private readonly ROOM_WATER w;
        private readonly ColorImp col = new ColorImp(20, 40, 100);
        private readonly TILE_SHEET stencil;
        private readonly TILE_SHEET sroad;
        private readonly TILE_SHEET edge;
        private readonly TILE_SHEET sbridge;

        public WSprite(ROOM_WATER w, RoomInitData init) : this(w, init, false)
        {
        }

        private WSprite(ROOM_WATER w, RoomInitData init, bool isBridge) : this(w, init, isBridge, false)
        {
        }

        private WSprite(ROOM_WATER w, RoomInitData init, bool isBridge, bool isPlaceholder) : this(w, init, isBridge, isPlaceholder, false)
        {
        }

        private WSprite(ROOM_WATER w, RoomInitData init, bool isBridge, bool isPlaceholder, bool isBelow)
        {
            this.w = w;

            edge = new ITileSheet(init.gSprite.Get("CANAL"), 288, 172)
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.house.Init(0, 0, 2, 2, d.s16);
                    s.house.SetVar(0).Paste(true);
                    s.house.SetVar(0).PasteRotated(1, true);
                    s.house.SetVar(1).Paste(true);
                    s.house.SetVar(1).PasteRotated(1, true);
                    return d.s16.SaveGame();
                }
            }.Get();

            stencil = new ITileSheet()
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.house.SetVar(2).Paste(true);
                    return d.s16.SaveGame();
                }
            }.Get();

            sroad = new ITileSheet()
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.house.SetVar(3).Paste(true);
                    return d.s16.SaveGame();
                }
            }.Get();

            sbridge = new ITileSheet()
            {
                protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.Init(0, s.house.Body().y2(), 1, 1, 2, 1, d.s16).Paste(3, true);
                    return d.s16.SaveGame();
                }
            }.Get();
        }

        public void Render(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, int edgeMask, int flowMask, bool bridge)
        {
            bool bb = false;
            foreach (DIR d in DIR.ORTHO)
            {
                if (WillConnectable.Is(it.Tx(), it.Ty(), d))
                    edgeMask |= d.Mask();
                else if (bridge && !bb && !SETT.PATH().Solidity.Is(it.Tx(), it.Ty(), d) && SETT.FLOOR().Getter.Is(it.Tx(), it.Ty(), d))
                {
                    d = d.Perpendicular();
                    if (!SETT.PATH().Solidity.Is(it.Tx(), it.Ty(), d) && SETT.FLOOR().Getter.Is(it.Tx(), it.Ty(), d))
                    {
                        bb = true;
                    }
                }
            }

            int steI = edgeMask;
            edgeMask = edgeMask + 16 * (it.Ran() & 3);
            edge.Render(r, edgeMask, it.X(), it.Y());

            TextureCoords ste = bb ? sroad.GetTexture(steI) : stencil.GetTexture(steI);
            CORE.Renderer().SetMaxDepth(it.X(), it.X() + C.TILE_SIZE, it.Y(), it.Y() + C.TILE_SIZE, ste, CORE.Renderer().GetDepth() + 1);

            {
                GroundType g = SETT.GROUND().MAP.Get(it.Tile());
                ColorImp.TMP.Set(g.Col(it.Tile()));
                ColorImp.TMP.ShadeSelf(0.8);
                ColorImp.TMP.Bind();
                stencil.RenderTextured(SETT.GROUND().GetTexture(it.Tile(), it.Ran()), steI, it.X(), it.Y());
                COLOR.Unbind();
            }

            if (flowMask != 0)
            {
                int ms = 0;
                int am = 0;
                foreach (DIR d in DIR.ORTHO)
                {
                    if ((d.Mask() & flowMask) != 0)
                    {
                        am++;
                    }
                    int dx = it.Tx() + d.X();
                    int dy = it.Ty() + d.Y();
                    if (!w.pumpable.Is(dx, dy) || w.pumpable.Get(dx, dy).Dirmask(dx, dy) != 0)
                        ms |= d.Mask();
                }

                int op = 255 / (am + 1);
                OpacityImp.TMP.Set(op);
                OpacityImp.TMP.Bind();
                col.Bind();

                double dd = (TIME.CurrentSecond() * 12);
                ms &= steI;

                foreach (DIR d in DIR.ORTHO)
                {
                    if ((d.Mask() & flowMask) != 0)
                    {
                        TextureCoords tex = SPRITES.textures().dis_small.Get(it.Tx() * C.T_PIXELS + dd * -d.X(), it.Ty() * C.T_PIXELS + dd * -d.Y());
                        stencil.RenderTextured(tex, ms, it.X(), it.Y());
                    }
                }

                OPACITY.Unbind();
                COLOR.Unbind();
            }

            if (bb)
            {
                sbridge.Render(r, it.Ran() & 7, it.X(), it.Y());
            }
        }

        public void RenderWater(SPRITE_RENDERER r, RenderIterator it, TILE_SHEET stencil, int tm, int op)
        {
            {
                GroundType g = SETT.GROUND().MAP.Get(it.Tile());
                ColorImp.TMP.Set(g.Col(it.Tile()));
                ColorImp.TMP.ShadeSelf(0.8);
                ColorImp.TMP.Bind();
                stencil.RenderTextured(SETT.GROUND().GetTexture(it.Tile(), it.Ran()), tm, it.X(), it.Y());
                COLOR.Unbind();
            }

            if (op > 0)
            {
                OPACITY.Bind(op);
                stencil.RenderTextured(SETT.GROUND().GetTexture(it.Tile(), it.Ran()), tm, it.X(), it.Y());
                OPACITY.Unbind();
            }
        }

        public void RenderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
        {
            data = 0;
            foreach (DIR d in DIR.ORTHO)
            {
                if (SETT.ROOMS().WATER.sprite.WillConnectable.Is(it.Tx(), it.Ty(), d))
                    data |= d.Mask();
            }
            SETT.ROOMS().WATER.sprite.renderBelow(r, s, it, data, bridge);
        }

        public void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
            SETT.ROOMS().WATER.sprite.renderPlaceholder(r, x, y, tx, ty);
        }

        public byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
        {
            return 0;
        }

        public static class RSprite : RoomSprite
        {
            private readonly RoomBlueprintImp blue;
            private readonly RoomPumpable pump;
            private readonly bool bridge;

            public RSprite(RoomBlueprintImp blue, RoomPumpable pump, bool bridge)
            {
                this.blue = blue;
                this.pump = pump;
                this.bridge = bridge;
            }

            public int SData()
            {
                return 0;
            }

            public void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
            {
                SETT.ROOMS().WATER.sprite.renderPlaceholder(r, x, y, tx, ty);
            }

            public void RenderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
            {
                data = 0;
                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.ROOMS().WATER.sprite.WillConnectable.Is(it.Tx(), it.Ty(), d))
                        data |= d.Mask();
                }
                SETT.ROOMS().WATER.sprite.renderBelow(r, s, it, data, bridge);
            }

            public bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
            {
                int edge = 0;
                foreach (DIR d in DIR.ORTHO)
                {
                    if (SETT.ROOMS().WATER.sprite.WillConnectable.Is(it.Tx(), it.Ty(), d))
                        data |= d.Mask();
                }
                int flow = blue.Is(it.Tile()) ? pump.Dirmask(it.Tx(), it.Ty()) : 0;

                SETT.ROOMS().WATER.sprite.render(r, s, it, edge, flow, bridge);

                return false;
            }

            public byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
            {
                return 0;
            }
        }

        public MAP_BOOLEAN WillConnectable => new MAP_BOOLEAN()
        {
            public override bool Is(int tx, int ty)
            {
                RoomBlueprintImp b = SETT.ROOMS().map.blueprintImp.Get(tx, ty);
                if (b == w.canal || b == w.drain)
                    return true;
                else if (b == w.pump)
                {
                    return w.pump.IsCanalConnection(tx, ty);
                }
                return false;
            }

            public override bool Is(int tile)
            {
                throw new RuntimeException();
            }
        };
    }
}