using System;
using game;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.misc;
using util.gui.misc;
using util.gui.panel;
using view.interrupter;
using view.main;
using world.battle.spec;

namespace view.world.ui.battle
{
    public class UIWBattlePrompt
    {
        private readonly Inter inter = new Inter();
        private readonly Battle battleBattle = new BattleBattle(inter);
        private readonly Battle battleAssist = new BattleAssist(inter);
        private readonly Battle battleLastStand = new BattleLastStand(inter);
        private readonly Battle battleSally = new BattleSally(inter);
        private readonly BattleSiege siege = new BattleSiege(inter);

        public UIWBattlePrompt()
        {
        }

        public bool IsBusty()
        {
            return inter.IsActivated();
        }

        public void Battle(WBattleSpec battle)
        {
            Prompt(battle, battleBattle);
        }

        public void Assist(WBattleSpec battle)
        {
            Prompt(battle, battleAssist);
        }

        public void LastStand(WBattleSpec battle)
        {
            Prompt(battle, battleLastStand);
        }

        public void BattleSally(WBattleSpec battle)
        {
            Prompt(battle, battleSally);
        }

        private void Prompt(WBattleSpec battle, Battle b)
        {
            if (inter.IsActivated())
            {
                LOG.Err("Oh no!");
                return;
            }

            b.Get(battle);

            inter.Set(b.Get(battle), true, battle.Player.Coo().X(), battle.Player.Coo().Y());
        }

        public void Siege(WBattleSiege siege)
        {
            if (inter.IsActivated())
            {
                LOG.Err("Oh no!");
                return;
            }
            inter.Set(this.siege.Get(siege), true, siege.Besiged.Cx(), siege.Besiged.Cy());
        }

        public void Result(WBattleSiege.Result siege)
        {
            inter.Set(new Conquer(inter, siege), false, siege.Besiged.Cx(), siege.Besiged.Cy());
        }

        public void Result(WBattleResult battle, bool retreat)
        {
            inter.Set(new Res(inter, battle, retreat), false, battle.Player.Coo().X(), battle.Player.Coo().Y());
        }

        private class Inter : Interrupter, ACTION
        {
            private GuiSection s;
            private bool canSave;
            private readonly GPanel panel = new GPanel();

            public Inter()
            {
                Pin();
                PersistantSet();
                panel.SetBig();
            }

            protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
            {
                s.Hover(mCoo);
                panel.Hover(mCoo);
                return true;
            }

            public void Set(GuiSection s, bool canSave, int cx, int cy)
            {
                if (IsActivated())
                    throw new Exception();

                VIEW.World().UiManager.Clear();
                VIEW.World().Panels.Clear();
                this.s = s;
                this.canSave = canSave;
                panel.Inner().Set(s);
                panel.Inner().CenterIn(C.DIM());
                panel.Inner().MoveY2(C.HEIGHT() - 100);

                s.Body().CenterIn(panel.Inner());
                VIEW.World().Activate();
                VIEW.World().Window.SetZoomout(0);
                VIEW.World().Window.CentererTile.Set(cx, cy + 200 / C.TILE_SIZE);
                Show(VIEW.Inters().Manager);
            }

            protected override void MouseClick(MButt button)
            {
                if (button == MButt.LEFT)
                    s.Click();
            }

            protected override void HoverTimer(GBox text)
            {
                s.HoverInfoGet(text);
            }

            protected override bool Render(Renderer r, float ds)
            {
                panel.Render(r, ds);
                s.Render(r, ds);
                return true;
            }

            protected override bool Update(float ds)
            {
                GAME.SPEED.TmpPause();
                return false;
            }

            public bool CanSave()
            {
                return canSave;
            }

            public void Exe()
            {
                Hide();
            }
        }
    }
}