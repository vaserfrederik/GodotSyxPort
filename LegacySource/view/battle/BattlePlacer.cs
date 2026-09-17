using System;
using System.Collections.Generic;
using game;
using game.battle.div;
using game.battle.thread.order;
using init.constant;
using settlement.room.military.artillery;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.gui.misc;
using util.rendering;
using view.keyboard;
using view.subview;

namespace view.battle
{
    public sealed class BattlePlacer
    {
        private readonly GameWindow w;
        public readonly DivSelection s;
        private readonly BattlePlacerRenderer ren;

        private Mode current;
        private readonly Mode selectMore;
        private readonly Mode position;
        private readonly Mode spin;
        private readonly BattlePlacerAttack attack;

        private readonly Action action = new Action();
        private readonly BattleOrderTask task = new BattleOrderTask();

        public BattlePlacer(GameWindow w, DivSelection s)
        {
            this.w = w;
            this.s = s;
            selectMore = new BattlePlacerSelect(w, s, action);
            position = new BattlePlacerPlace(w, s, action);
            spin = new BattlePlacerSpin(w, s, action);
            attack = new BattlePlacerAttack(w, s, action);
            current = selectMore;
            ren = new BattlePlacerRenderer(this);
        }

        public void Click(MButt butt)
        {
            if (butt == MButt.LEFT)
            {
                action.clicked = true;
                action.start.Set(w.Pixel());
            }
            else if (butt == MButt.RIGHT)
            {
                if (action.clicked)
                {
                    action.clicked = false;
                }
                else
                {
                    s.Clear();
                }
            }
        }

        private readonly Key[] arrowsKeys = new Key[]
        {
            KEYS.BATTLE().UP,
            KEYS.BATTLE().DOWN,
            KEYS.BATTLE().LEFT,
            KEYS.BATTLE().RIGHT
        };

        private readonly DIR[] arrowsDIRS = new DIR[]
        {
            DIR.N, DIR.S, DIR.W, DIR.E
        };

        private readonly int[] arrowPressed = Alloc.Ii(4);

        void KeyPush()
        {
            int dx = 0;
            int dy = 0;

            if (KEYS.BATTLE().SELECT_ALL.ConsumeClick())
            {
                action.clicked = false;
                s.Clear();
                bool allSelected = true;
                foreach (Div d in GAME.ARMIES().Player().Divisions())
                {
                    if (d.MenNrOf() > 0)
                    {
                        allSelected &= s.Selected(d);
                        s.Select(d);
                    }
                }
                if (allSelected)
                {
                    foreach (ArtilleryInstance ins in s.Artillery.All())
                    {
                        s.Artillery.Select(ins);
                    }
                }
            }

            if (KEYS.MAIN().BACKSPACE.ConsumeClick())
            {
                action.clicked = false;
                foreach (Div d in s.Selection())
                {
                    task.Stop(d);
                    d.Order().Task.Set(task);
                }
                foreach (ArtilleryInstance ins in s.Artillery.All())
                {
                    ins.ClearTarget();
                }
            }

            for (int i = 0; i < arrowsKeys.Length; i++)
            {
                if (arrowsKeys[i].ConsumeClick())
                {
                    dx += arrowsDIRS[i].X() * C.TILE_SIZE;
                    dy += arrowsDIRS[i].Y() * C.TILE_SIZE;
                }
                else if (arrowsKeys[i].IsPressed())
                {
                    if (arrowPressed[i]++ > 60)
                    {
                        dx += arrowsDIRS[i].X() * (arrowPressed[i] - 60) * 8;
                        dy += arrowsDIRS[i].Y() * (arrowPressed[i] - 60) * 8;
                        arrowPressed[i] = 60;
                    }
                }
                else
                {
                    arrowPressed[i] = 0;
                }
            }

            if (KEYS.BATTLE().UP.ConsumeClick())
            {
                dy = -C.TILE_SIZE;
            }
            if (KEYS.BATTLE().DOWN.ConsumeClick())
            {
                dy = C.TILE_SIZE;
            }

            if (KEYS.BATTLE().LEFT.ConsumeClick())
                dx = -C.TILE_SIZE;
            if (KEYS.BATTLE().RIGHT.ConsumeClick())
            {
                dx = C.TILE_SIZE;
            }

            if (dx == 0 && dy == 0)
                return;
            action.clicked = false;
            foreach (Div d in s.Selection())
            {
                GAME.ARMIES().Placer.Deploy(d, dx, dy);
            }
        }

        private Mode GetState()
        {
            if (s.AllSelected() <= 0)
                return selectMore;
            if (attack.Init())
            {
                return attack;
            }
            if (s.AllSelected() <= 0 || KEYS.MAIN().UNDO.IsPressed())
            {
                return selectMore;
            }
            else if (KEYS.MAIN().MOD.IsPressed())
            {
                return spin;
            }
            else
            {
                return position;
            }
        }

        public void Update(bool hovered)
        {
            current = GetState();

            KeyPush();
            action.clickReleased = false;

            if (action.clicked && !MButt.LEFT.IsDown())
            {
                action.clicked = false;
                action.clickReleased = true;
            }

            ren.Add(hovered);
            current.Update(hovered);
        }

        public void HoverTimer(GBox text)
        {
            current.HoverTimer(text);
        }

        public abstract class Mode
        {
            public abstract void Update(bool hovered);
            public abstract void HoverTimer(GBox text);
            public abstract void Render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds);
        }

        public class Action
        {
            public Coo start = new Coo();
            public bool clicked;
            public bool clickReleased;
        }
    }
}