using System;
using System.Collections.Generic;
using game;
using game.faction;
using init.constant;
using init.sprite;
using settlement.main;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.panel;
using util.text;
using view.main;
using view.sett.ui.minimap;
using view.subview;
using view.tool;

namespace view.sett
{
    public class SettViewStart : VIEW.ViewSub
    {
        private readonly GameWindow window = new GameWindow(
            1,
            C.DIM(),
            SETT.PIXEL_BOUNDS,
            0);
        private readonly ToolManager manager;
        private readonly UIMinimapSett mini;
        private readonly ToolDefault def;
        private readonly ToolConfig config;

        public SettViewStart()
        {
            D.t(this);

            GuiSection s = new GuiSection();
            s.body().setDim(200, 1);
            // {
            //     int x = 0;
            //     foreach (Minable m in RESOURCES.minables().all())
            //     {
            //         s.add(new GStat()
            //         {
            //             public override void update(GText text)
            //             {
            //                 GFORMAT.i(text, SETT.MINERALS().totals.get(m));
            //             }

            //             public override void hoverInfoGet(GBox b)
            //             {
            //                 b.text(m.name);
            //             };
            //         }.hv(m.resource.icon()), (x % 6) * 60, (x / 6) * 24);
            //         x++;
            //     }
            // }

            {
                final CLICKABLE butt = new GButt.ButtPanel(new SPRITE.Twin(SPRITES.icons().m.terrain, SPRITES.icons().m.rotate))
                {
                    protected override void clickA()
                    {
                        SETT.reGenerate();
                    }
                };
                s.addRelBody(4, DIR.S, butt.hoverInfoSet(D.g("Regenerate")));
            }

            GPanel p = new GPanel(s.body()).setBig();
            p.setTitle(D.g("start", "Landing Party"));
            p.body.moveY1(80);
            p.body.centerX(C.DIM());
            s.body().centerIn(p.inner());
            s.add(p);
            s.moveLastToBack();

            manager = new ToolManager(uiManager, window);
            mini = new UIMinimapSett(uiManager, 0, window, UIMinimapSettConfig.ALL);
            mini.panel().addOverlays();
            mini.panel().addScreenshot(null);

            config = new ToolConfig()
            {
                public override bool back()
                {
                    return false;
                }

                public override void update(bool UIHovered)
                {
                    if (POP.tot(null) > 0)
                    {
                        VIEW.s().activate();

                        FACTIONS.player().bonusesCustom.apply();

                        GAME.setGameStart();
                    }
                }

                public override void addUI(LISTE<RENDEROBJ> uis)
                {
                    uis.add(s);
                    // uis.add(butt);
                }
            };
            window.setzoomoutMax(3);
            def = new ToolDefault(manager);
            manager.place(SETT.PLACERS().landingParty, config);
        }

        protected override void hover(COORDINATE mCoo, bool mouseHasMoved)
        {

        }

        protected override void mouseClick(MButt button)
        {

        }

        protected override void hoverTimer(double mouseTimer, GBox text)
        {

        }

        protected override bool update(float ds, bool should)
        {
            VIEW.s().getWindow().copy(window);
            if (MButt.RIGHT.isDown())
            {
                manager.set(def);
            }
            else if (POP.tot(null) == 0)
                manager.place(SETT.PLACERS().landingParty, config);

            return true;
        }

        protected override void render(Renderer r, float ds, bool hide)
        {
            s().render(r, ds, window, mini.config);
            if (window.consumeHover())
            {
                SETT.LIGHTS().renderMouse(window.pixel().x(), window.pixel().y(), -window.pixels().relX(), -window.pixels().relY(), 5);

                if (window.hasZoomedOutMoreandConsumeThatMotherFZoom())
                    mini.open();
            }
        }

        public override void activate()
        {
            window.stop();
            window.copy(VIEW.s().getWindow());
            base.activate();
        }

        public void clear()
        {

        }
    }
}