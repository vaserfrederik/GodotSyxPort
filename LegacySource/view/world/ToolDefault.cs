using System;
using System.Collections.Generic;
using game.faction;
using game.faction.npc;
using init.settings;
using init.sprite;
using init.type;
using settlement.tilemap.ground;
using snake2d;
using util.gui.misc;
using util.info;
using view.main;
using view.subview;
using view.tool;
using view.world.ui;
using world;
using world.entity;
using world.entity.army;
using world.map.landmark;
using world.map.regions;

namespace view.world
{
    internal sealed class ToolDefault : Tool
    {
        private bool exploring = false;
        private readonly ToolConfig config = new ToolConfig();

        public ToolDefault(ToolManager manager) : base(manager)
        {
        }

        protected override void UpdateHovered(float ds, GameWindow window)
        {
            exploring &= MButt.RIGHT.isDown();
            Update(ds, window);
            if (exploring)
            {
                Explore(window);
            }
            else
            {
                Hover(window.Pixel(), window);
            }
        }

        private void Hover(COORDINATE coo, GameWindow window)
        {
            if (!PIXELS().HoldsPoint(coo))
                return;

            WEntity e = ENTITIES().GetTallest(coo);
            GBox box = VIEW.hoverBox();
            if (e != null && (e.Faction() == FACTIONS.Player() || !WORLD.FOW().Is(window.Tile())))
            {
                WORLD.OVERLAY().HoverEntity(e);
                WorldHoverer.Hover(box, e);
            }
            else
            {
                Region reg = WORLD.REGIONS().centre.Get(window.Tile());
                if (reg != null)
                {
                    WORLD.OVERLAY().Hover(reg);
                    VIEW.World().UI.regions.Hover(reg, box);
                }
            }
            VIEW.World().UI.factions.Hover(window.Tile().X(), window.Tile().Y());
        }

        protected override void Update(float ds, GameWindow window)
        {
        }

        protected override void RenderHovered(SPRITE_RENDERER r, float ds, GameWindow window, GBox box)
        {
            if (exploring)
            {
                SPRITES.cons().BIG.dashed.Render(r, 0, window.Tile().Rel().X(), window.Tile().Rel().Y());
                VIEW.Mouse().SetReplacement(SPRITES.icons().m.questionmark);
            }
        }

        protected override bool RightClick()
        {
            exploring = true;
            return false;
        }

        protected override void Click(GameWindow window)
        {
            if (!PIXELS().HoldsPoint(window.Pixel()))
                return;

            if (VIEW.World().UI.factions.OpenIs())
            {
                Region reg = WORLD.REGIONS().centre.Get(window.Tile());
                if (reg != null)
                {
                    if (reg.Faction() is FactionNPC)
                        VIEW.World().UI.factions.Open((FactionNPC)reg.Faction());
                }
                return;
            }

            foreach (WEntity e in ENTITIES().Fill(window.Pixel().X(), window.Pixel().Y()))
            {
                if (e != null && (e.Faction() == FACTIONS.Player() || !WORLD.FOW().Is(window.Tile())))
                {
                    if (e is WArmy)
                    {
                        WArmy a = (WArmy)e;
                        if (a.Faction() == FACTIONS.Player() || S.Get().developer)
                        {
                            VIEW.World().UI.armies.Open(a);
                            return;
                        }
                    }
                }
            }

            Region reg = WORLD.REGIONS().centre.Get(window.Tile());
            if (reg != null)
            {
                VIEW.World().UI.regions.Open(reg, true);
            }
        }

        protected override ToolConfig DefaultConfig()
        {
            return config;
        }

        private const int Tabs = 5;

        internal static void Explore(GameWindow win)
        {
            if (!PIXELS().HoldsPoint(win.Pixel()))
                return;

            int tx = win.Tile().X();
            int ty = win.Tile().Y();

            GBox b = VIEW.hoverBox();

            WORLD.OVERLAY().Landmarks.Add();

            b.Title(TERRAINS.world.Get(tx, ty).Name);
            b.Add(SPRITES.icons().m.crossair);
            b.Tab(1);
            b.Add(b.Text().Add(tx));
            b.Tab(2);
            b.Add(b.Text().Add(ty));
            b.NL();

            b.TextLL(Ground.¤¤moisture);
            b.Tab(Tabs);
            b.Add(GFORMAT.Perc(b.Text(), WORLD.MOISTURE().Get(tx, ty)));
            b.NL();

            CLIMATE z = CLIMATE().getter.Get(tx, ty);
            b.TextLL(CLIMATES.INFO().Name);
            b.Tab(Tabs);
            b.Text(z.Name);
            b.NL();

            WorldLandmark a = LANDMARKS().setter.Get(tx, ty);
            if (a != null)
            {
                b.NL(8);
                WORLD.OVERLAY().Landmarks.Hover(a);
                b.TextLL(a.Name);
                b.NL();
                b.Text(a.Description);
                b.NL();
            }

            if (S.Get().developer)
            {
                b.Add(b.Text().Add(WORLD.FOW().Is(tx, ty)));
                b.NL();
            }
        }
    }
}