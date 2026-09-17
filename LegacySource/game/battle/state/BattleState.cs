using System;
using System.IO;
using System.Collections.Generic;
using game.battle.div;
using game.save;
using init.constant;
using init.paths;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main.throne;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sets;
using util.gui.misc;
using util.text;
using view.battle;
using view.interrupter;
using view.main;
using view.sett.ui.minimap;
using view.subview;
using world;
using world.battle;
using world.battle.spec;
using world.map.regions.centre;
using world.map.road;

namespace game.battle.state
{
    public class BattleState
    {
        private bool deploying = true;
        private bool concluded = false;
        private readonly Rec deploymentTiles = new Rec();
        private double throneTimer = 0;
        public static readonly int throneMax = 60 * 5;
        private readonly BattleStateExiter resolve;
        private readonly Path saveFile;

        public static string debugLoad = "__battledebug";

        public static void setGenerate(BattleStateExiter resolve, BattleStateSpec spec)
        {
            save("__beforeBattle");
            if (!PATHS.Local().Save().Exists("__battle"))
                PATHS.Local().Save().Create("__battle");
            BattleState s = new BattleState(resolve, PATHS.Local().Save().Get("__battle"));

            new BattleStateGenerator().Generate(s, spec, s.deploymentTiles);
            save("__battle");
            s.view();
            VIEW.B().GetWindow().CentererTile.Set(THRONE.Coo());
            VIEW.B().GetWindow().Zoomoutmax();
        }

        public static void setLoaded(BattleStateExiter resolve, Path saveFile, bool deploy)
        {
            BattleState s = new BattleState(resolve, saveFile);
            s.view();
        }

        private BattleState(BattleStateExiter resolve, Path saveFile)
        {
            GAME.BATTLE_THREADS().Pause();
            this.saveFile = saveFile;
            this.resolve = resolve;
            deploying = true;
            throneTimer = throneMax;

            DIR d = DIR.Get(THRONE.Coo(), SETT.TWIDTH / 2, SETT.TWIDTH / 2);

            deploymentTiles.Set(
                SETT.TILE_BOUNDS.CX() + d.Next(-2).X() * SETT.TWIDTH / 2, SETT.TILE_BOUNDS.CX() + d.Next(3).X() * SETT.TWIDTH / 2,
                SETT.TILE_BOUNDS.CY() + d.Next(-2).Y() * SETT.TWIDTH / 2, SETT.TILE_BOUNDS.CY() + d.Next(3).Y() * SETT.TWIDTH / 2);
            deploymentTiles.MakePositive();
            view();
            VIEW.B().GetWindow().CentererTile.Set(THRONE.Coo());
            VIEW.B().GetWindow().Zoomoutmax();
        }

        private void view()
        {
            VIEW.Messages().HideAll();
            GAME.SPEED.SpeedSet(0);
            VIEW.B().Activate(this);
        }

        private static void save(string name)
        {
            if (PATHS.Local().Save().Exists(name))
                PATHS.Local().Save().Delete(name);
            if (GAME.Saver().Save(name) == null)
                throw new Errors.DataError(name, PATHS.Local().Save().Get(name));
        }

        public void reloadBattle()
        {
            new GameLoader(saveFile)
            {
                public override void DoAfterSet()
                {
                    GAME.BATTLE_THREADS().Pause();
                    concluded = false;
                    deploying = true;

                    throneTimer = throneMax;

                    VIEW.B().Activate(this);
                    VIEW.B().GetWindow().CentererTile.Set(THRONE.Coo());
                    VIEW.B().GetWindow().Zoomoutmax();
                    GAME.SPEED.SpeedSet(0);
                }
            }.Set();
        }

        public double ThroneTimer()
        {
            return throneTimer;
        }

        public bool Deploying()
        {
            return deploying;
        }

        public void Deploy()
        {
            deploying = false;
            GAME.SetGameStart();
        }

        public RECTANGLE DeploymentBounds()
        {
            return deploymentTiles;
        }

        void liveResolve(bool retreat, bool win)
        {
            BATTLE_RESULT res = BATTLE_RESULT.VICTORY;
            if (throneTimer <= 0)
                res = BATTLE_RESULT.DEFEAT;
            else if (retreat)
                res = BATTLE_RESULT.RETREAT;
            else if (!win)
                res = BATTLE_RESULT.DEFEAT;

            int eDeaths = STATS.POP().COUNT.Leaves().Get(CAUSE_LEAVES.SLAYED().Index()).Statistics(HCLASSES.OTHER()).Get(null);
            int pLosses = STATS.POP().COUNT.Leaves().Get(CAUSE_LEAVES.SLAYED().Index()).Statistics(HCLASSES.CITIZEN()).Get(null);
            resolve.Exit(res, pLosses, eDeaths);
        }

        public void liveRetreat()
        {
            liveResolve(true, false);
        }

        public int LiveRetreatLosses()
        {
            int am = (int)Math.Ceiling(GAME.ARMIES().Enemy().Men() * WBattles.RetreatPenalty);
            foreach (Div d in GAME.ARMIES().Player().Divisions())
            {
                if (d.Status().IsFighting())
                {
                    am += d.MenNrOf() * 0.5;
                }
            }
            if (am > GAME.ARMIES().Player().Men())
            {
                am = GAME.ARMIES().Player().Men();
            }
            return am;
        }

        public void Update(double ds)
        {
            if (concluded)
                return;

            if (deploying)
                return;

            if (GAME.ARMIES().Player().Men() == 0 || GAME.ARMIES().Enemy().Men() == 0)
            {
                if (GAME.ARMIES().Player().Men() == 0)
                    new ILiveConclude(Dic.¤¤Defeat, false, false);
                else if (GAME.ARMIES().Enemy().Men() == 0)
                    new ILiveConclude(Dic.¤¤Victory, false, true);
                concluded = true;
                return;
            }

            LIST<ENTITY> es = SETT.ENTITIES().FillTiles(THRONE.Coo().X() - 4, THRONE.Coo().Y() - 4, 8, 8);

            double tt = throneTimer - ds;
            throneTimer = throneMax;
            foreach (ENTITY e in es)
            {
                if (e is Humanoid)
                {
                    Humanoid h = (Humanoid)e;
                    if (h.Indu().HType().IsHostile() && h.Division() != null)
                    {
                        throneTimer = tt;
                        break;
                    }
                }
            }

            if (throneTimer <= 0)
            {
                new ILiveConclude(Dic.¤¤Defeat, false, false);
                concluded = true;
                return;
            }
        }

        private final class ILiveConclude : Interrupter
        {
            private readonly GameWindow window = new GameWindow(C.DIM(), SETT.PIXEL_BOUNDS, 0);
            private readonly GuiSection section;

            ILiveConclude(CharSequence title, bool retreat, bool win)
            {
                Pin();
                PersistantSet();

                section = new UIBattleResult(title)
                {
                    protected override void Close()
                    {
                        Hide();
                        liveResolve(retreat, win);
                    }
                };
                window.Copy(VIEW.B().GetWindow());
                GAME.BATTLE_THREADS().Pause();
                VIEW.Inters().Manager.Add(this);
                section.Body().CenterIn(C.DIM());
            }

            protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
            {
                section.Hover(mCoo);
                return true;
            }

            protected override void MouseClick(MButt button)
            {
                if (button == MButt.LEFT)
                    section.Click();
            }

            protected override void HoverTimer(double ds)
            {
                // Implementation for hover timer
            }

            protected override bool Update(double ds)
            {
                GAME.SPEED.SpeedSet(1);
                return true;
            }
        }

        public static bool OkWorldTile(int tx, int ty, DIR eDir)
        {
            return OkWorldTile(tx, ty) && OkWorldTile(tx + eDir.X(), ty + eDir.Y());
        }

        private static bool OkWorldTile(int tx, int ty)
        {
            if (tx - WCentre.TILE_DIM / 2 < 0 || ty - WCentre.TILE_DIM / 2 < 2 || tx + WCentre.TILE_DIM / 2 >= TWIDTH() || ty + WCentre.TILE_DIM / 2 >= THEIGHT())
                return false;
            for (int di = 0; di < DIR.ALLC.Size(); di++)
            {
                DIR d = DIR.ALLC.Get(di);
                if (WORLD.MOUNTAIN().Is(tx, ty))
                    return false;
                if (WORLD.WATER().IsBig.Is(tx, ty))
                {
                    return false;
                }

                if (!WTRAV.IsGoodLandTile(tx + d.X(), ty + d.Y()))
                    return false;
                if (d != DIR.C && !WTRAV.Can(tx, ty, d, false))
                    return false;
            }
            return true;
        }
    }
}