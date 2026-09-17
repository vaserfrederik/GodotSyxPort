using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.faction;
using game.raiding;
using init.constant;
using init.sprite;
using snake2d;
using util.info;
using util.rendering;
using view.main;
using world;
using world.entity;
using world.entity.army;
using world.map.regions;
using world.region;
using world.region.pop;

namespace world.overlay
{
    public class WorldOverlays
    {
        public readonly OverlayTileNormal minerals = new OverlayMineral();
        public readonly OverlayRegnames regNames = new OverlayRegnames();
        
        public readonly EThings things = new EThings();
        public readonly ERegion regionOutline = new ERegion();
        public readonly OverlayExplore landmarks = new OverlayExplore();
        public readonly EPath path = new EPath();
        public readonly OverlayTileNormal climate = new OverlayClimate();
        public readonly OverlayTileNormal factions = new OverlayFaction();
        public readonly OverlayTileNormal biome = new OverlayRaceBiome();
        private readonly Edger edger = new Edger(WORLD.TWIDTH(), WORLD.THEIGHT());
        private readonly Army army = new Army();
        public readonly OverlayRegAbs raiders = new OverlayRegAbs(RaidingMap.¤¤Name, RaidingMap.¤¤desc, true)
        {
            
            public override double value(Region reg)
            {
                return CLAMP.d(GAME.raiders().entry.get(reg).security(), 0, 1);
            }
            
            public override bool is(Region reg)
            {
                return reg.faction() == FACTIONS.player() && !reg.capitol();
            }
            
            public override void renderAbove(Renderer ren, ShadowBatch s, RenderData data)
            {
                
                base.renderAbove(ren, s, data);
                
                COLOR.WHITE2WHITE.bind();
                
                foreach (RaidEntryPoint c in GAME.raiders().entry.entrySpots())
                {
                    int x = data.transformGX(c.c().x() * C.TILE_SIZE);
                    int y = data.transformGY(c.c().y() * C.TILE_SIZE);
                    
                    UI.icons().s.alert.renderScaled(ren, x, y, C.SCALE);	
                }

                COLOR.unbind();			
            }
            
        };
        
        public readonly OverlayRegAbs health = new OverlayRegAbs(RDHealth.¤¤name, RDHealth.¤¤desc, true)
        {
            
            public override double value(Region reg)
            {
                return RD.HEALTH().getD(reg);
            }
            
            public override bool is(Region reg)
            {
                return reg.faction() == FACTIONS.player();
            }
            
        };
        
        public readonly OverlayRegAbs loyalty = new OverlayRegAbs(RDRaces.¤¤Loyalty, RDRaces.¤¤LoyaltyD, true)
        {
            
            public override double value(Region reg)
            {
                return RD.RACES().loyaltyAll.getD(reg);
            }
            
            public override bool is(Region reg)
            {
                return reg.faction() == FACTIONS.player();
            }
            
        };
        
        public readonly List<OverlayTileNormal> togglable = new List<OverlayTileNormal>
        {
            new OverlayPathing(),
            factions,
            new OverlayDiplomacy(),
            new OverlayMineral(),
            climate,
            biome,
            raiders,
            health,
            loyalty
        };
        
        private Overlay current;
        public Overlay debug;
        
        public WorldOverlays() throws IOException
        {
            
        }
        
        private bool hide = false;

        public void hide()
        {
            this.hide = true;
        }
        
        
        public bool renderBelow(Renderer r, ShadowBatch s, RenderData data, int zoomout)
        {
            
            if (current == null)
                return false;
            Overlay o = current;
            current = null;
            bool ret = !hide && o.renderBelow(r, s, data);
            COLOR.unbind();
            OPACITY.unbind();
            hide = false;
            return ret;
        }
        
        public void render(Renderer r, ShadowBatch s, RenderData data, int zoomout)
        {
            if (hide)
            {
                things.clear();
                return;
            }
            if (debug != null)
                current = debug;
            if (current == null)
                current = regNames;
            
            things.render(r, s, data);
            path.render(r, s, data);
            regionOutline.renderAbove(r, s, data);
            edger.render(r, data, zoomout);
            if (current != null)
                current.renderAbove(r, s, data);
            things.clear();
            COLOR.unbind();
            OPACITY.unbind();
        }

        public void hover(Region reg)
        {
            regionOutline.add(reg);
            regNames.exclude(reg);
            hoverBox(reg);
        }
        
        public void hoverEntity(WEntity ent)
        {
            things.hover(ent);
            if (ent.path() != null)
            {
                path.add(ent.ctx(), ent.cty(), ent.path().destX(), ent.path().destY(), ent.path().treaty());
            }
            
        }
        
        public void hoverArmy(WArmy army)
        {
            if (army == null)
            {
                this.army.add(null);
                
            }
            else
            {
                this.army.add(army.faction());
                hoverEntity(army);
            }
        }
        
        
        public void hoverArmy(Faction f)
        {
            this.army.add(f);
        }
        
//        public void hover(RECTANGLE body, COLOR color, bool thick, int margin)
//        {
//            things.add(body.x1()-margin, body.y1()-margin, body.width()+margin*2, body.height()+margin*2, color, thick);
//        }
//        
//        public void hover(int x1, int y1, int w, int h, COLOR color, bool thick)
//        {
//            things.add(x1, y1, w, h, color, thick);
//        }
//        
//        public void hover(WEntity e)
//        {
//            hover(e.body(), GCOLORS_MAP.get(e.faction()), true, 6);
//        }
        
        public void hoverBox(Region region)
        {
            
            if (region == null)
                return;
            
            int x1 = region.cx() * C.TILE_SIZE;
            int y1 = region.cy() * C.TILE_SIZE;
            
            if (region.capitol())
            {
                things.hover(x1 - C.TILE_SIZE - C.TILE_SIZEH, y1 - C.TILE_SIZE - C.TILE_SIZEH, C.TILE_SIZE * 4, C.TILE_SIZE * 4, region.faction().banner().colorBG(), true);
            }
            else if (region.faction() != null)
            {
                things.hover(x1 - C.TILE_SIZE + C.TILE_SIZEH / 2, y1 - C.TILE_SIZE + C.TILE_SIZEH / 2, C.TILE_SIZE * 2 + C.TILE_SIZEH, C.TILE_SIZE * 2 + C.TILE_SIZEH, region.faction().banner().colorBG(), false);
            }
            else
            {
                things.hover(x1 - C.TILE_SIZE + C.TILE_SIZEH / 2, y1 - C.TILE_SIZE + C.TILE_SIZEH / 2, C.TILE_SIZE * 2 + C.TILE_SIZEH, C.TILE_SIZE * 2 + C.TILE_SIZEH, COLOR.WHITE65, false);
            }
            WORLD.MINIMAP().hilight(region);
        }
        
        
        public abstract class Overlay
        {
            
            public Overlay()
            {
                
            }
            
            public void add()
            {
                WORLD.OVERLAY().current = this;
            }
            
            public bool added()
            {
                return WORLD.OVERLAY().current == this;
            }
            
            protected COORD mouse = new COORD();
            
            protected COORD mouse()
            {
                mouse.x = (int)(VIEW.M.x() / C.TILE_SIZE);
                mouse.y = (int)(VIEW.M.y() / C.TILE_SIZE);
                return mouse;
            }
        }
        
        public class OverlayTile : Overlay
        {
            private bool above;
            private bool below;
            
            public OverlayTile(bool above, bool below)
            {
                this.above = above;
                this.below = below;
                
            }
            
            public override void renderAbove(Renderer r, ShadowBatch s, RenderData data)
            {
                if (above)
                {
                    RenderIterator it = data.onScreenTiles(0, 0, 0, 0);
                    while (it.hasNext())
                    {
                        renderAbove(r, s, it);
                        it.next();
                    }
                }
                
            }

            public override bool renderBelow(Renderer r, ShadowBatch s, RenderData data)
            {
                if (below)
                {
                    RenderIterator it = data.onScreenTiles(0, 0, 0, 0);
                    while (it.hasNext())
                    {
                        renderBelow(r, s, it);
                        it.next();
                    }
                }
                return below;
            }
            
            protected void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
            {
                
            }
            protected void renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
            {
                
            }
            
            protected static void renderUnder(int m, SPRITE_RENDERER r, RenderIterator it)
            {
                SPRITES.cons().BIG.filled.render(r, m, it.x(), it.y());
            }

        }
        
        public abstract class OverlayTileNormal : OverlayTile
        {
            public readonly INFO info;
            public OverlayTileNormal(CharSequence name, CharSequence desc, bool above, bool below) : base(above, below)
            {
                info = new INFO(name, desc);
            }

            public override void add()
            {
                base.add();
            }
            
        }

    }
}