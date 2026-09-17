using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game;
using game.faction;
using game.faction.player;
using init.race;
using init.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.misc;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;
using view.subview;
using view.tool;
using view.world;
using world;

namespace view.world.generator
{
    public class WorldViewGenerator : VIEW.ViewSubSimple
    {
        static readonly CharSequence ¤¤generate = "¤generate";
        static readonly CharSequence ¤¤regenerate = "¤regenerate";
        static readonly CharSequence ¤¤start = "¤start";
        static readonly CharSequence ¤¤home = "¤home";

        static
        {
            D.ts(typeof(WorldViewGenerator));
        }

        readonly GameWindow window;
        readonly ToolManager tools;
        readonly IMinimap minimap;
        readonly Intr dummy;
        readonly ISidePanels panels;
        public static readonly ACTION loadPrint = new ACTION
        {
            exe = () =>
            {
                if (!SPRITES.loader().isMini())
                    SPRITES.loader().minify(true, Dic.¤¤Generating);
                SPRITES.loader().print(Dic.¤¤Generating);
            }
        };

        bool canSelectRace = true;
        bool hasSeletedRace = false;
        bool hasProfiled = false;
        bool hasSelectedTitles = true;

        public WorldViewGenerator()
        {
            foreach (PTitle t in FACTIONS.player().titles.all())
            {
                if (t.unlocked())
                    hasSelectedTitles = false;
            }

            this.window = WorldView.createwindow();
            dummy = new Intr(this);
            minimap = new IMinimap(this);
            tools = new ToolManager(uiManager, window);
            window.setZoomout(2);
            window.centererTile.set(WORLD.TWIDTH() / 2, WORLD.THEIGHT() / 2);
            panels = new ISidePanels(uiManager, 0);
            set();
        }

        public static void setresettle(Race race)
        {
            if (VIEW.current() is WorldViewGenerator)
            {
                WorldViewGenerator g = (WorldViewGenerator)VIEW.current();
                FACTIONS.player().setRace(race);
                g.hasSeletedRace = true;
                g.hasProfiled = true;
                g.canSelectRace = false;
                g.set();
            }
            else
            {
                LOG.err(VIEW.current());
            }
        }

        public override void activate()
        {
            base.activate();
            window.stop();
            WORLD.FOW().toggled.set(false);
        }

        public override void deactivate()
        {
            WORLD.FOW().toggled.set(true);
        }

        protected override void hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            window.hover();
        }

        protected override void mouseClick(MButt button)
        {
        }

        protected override void hoverTimer(double mouseTimer, GBox text)
        {
        }

        protected override bool update(float ds, bool should)
        {
            return true;
        }

        protected override void render(Renderer r, float ds, bool hide)
        {
            window.crop(uiManager.viewPort());
            GAME.world().render(r, ds, window.zoomout(), window.pixels(), window.view().x1(), window.view().y1());
        }

        void reset()
        {
            uiManager.clear();
            dummy.add(null, null);
            tools.place(null);
            minimap.hide();
        }

        public void set()
        {
            reset();
            WorldGen g = WORLD.GEN();

            if (!hasSeletedRace && canSelectRace)
            {
                new StagePickRace(this);
            }
            else if (!hasProfiled)
            {
                new StageVisuals(this);
            }
            else if (!hasSelectedTitles && FACTIONS.player().titles.unlocked() > 0)
            {
                new StagePickTitles(this);
            }
            else if (!g.hasGeneratedTerrain)
            {
                new StageTerrain(this);
            }
            else if (g.playerX < 0)
            {
                new StageCapitol(this, false);
            }
            else
            {
                new StageFinish(this);
            }
        }
    }
}