using System;
using System.Collections.Generic;
using Game.Battle;
using Game.Battle.Div;
using Game.Battle.Formation;
using Game.Battle.Thread.Order;
using Init.Constant;
using Init.Sprite;
using Snake2D.Renderer;
using Snake2D.Util.Color;
using Snake2D.Util.Datatypes;
using Util.Colors;
using Util.Gui.Misc;
using Util.Rendering;
using View.Battle.BattlePlacer;
using View.Main;
using View.Subview;

namespace View.Battle
{
    internal class BattlePlacerSpin : Mode
    {
        private readonly GameWindow w;
        private readonly DivSelection s;
        private readonly DivFormationImp form = new DivFormationImp();
        private readonly Action a;

        public BattlePlacerSpin(GameWindow w, DivSelection s, Action a)
        {
            this.w = w;
            this.s = s;
            this.a = a;
        }

        private readonly BattleOrderTask task = new BattleOrderTask();
        private double cx, cy;
        private readonly VectorImp vec = new VectorImp();

        public override void Update(bool hovered)
        {
            cx = 0;
            cy = 0;
            foreach (Div d in s.Selection())
            {
                d.Order().Dest.Get(form);
                cx += form.Start().X;
                cy += form.Start().Y;
            }
            cx /= s.Selection().Count;
            cy /= s.Selection().Count;

            if (!hovered)
                return;

            if (a.ClickReleased)
            {
                foreach (Div d in s.Selection())
                {
                    DivFormationImp f = GetFor(d);
                    if (f != null)
                    {
                        d.Order().Dest.Set(f);
                        task.Move(d);
                        d.Order().Task.Set(task);
                    }
                }
                if (VIEW.B().State() != null && VIEW.B().State().Deploying())
                {
                    GAME.ARMIES().InitAndTeleport(s.Selection());
                }
            }
        }

        private DivFormationImp GetFor(Div d)
        {
            d.Order().Dest.Get(form);

            double newAngle = 0;
            {
                double destDX = w.Pixel().X - a.Start.X;
                destDX /= (100 << w.Zoomout());
                destDX %= Math.PI * 2;
                newAngle = destDX;
            }

            double dist = vec.Set(cx, cy, form.Start().X, form.Start().Y);

            vec.RotateRad(newAngle);

            double x1 = a.Start.X + vec.NX() * dist;
            double y1 = a.Start.Y + vec.NY() * dist;

            vec.Set(form.DX(), form.DY());
            vec.RotateRad(newAngle);

            return GAME.ARMIES().Placer.Deployer.Deploy(d.Info, d.MenNrOf(), d.Settings().Formation, (int)x1, (int)y1, vec.NX(), vec.NY(), form.Width(), d.Army());
        }

        public override void Render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
        {
            VIEW.Mouse().SetReplacement(SPRITES.Icons().M.Rotate);

            if (a.Clicked)
            {
                int x = w.Pixel().X;
                int y = w.Pixel().Y;
                {
                    if (GAME.ARMIES().Placer.IsBlocked(x, y, C.TILE_SIZE, GAME.ARMIES().Player()))
                        GCOLOR.MAP().BAD.Bind();
                    else
                        GCOLOR.MAP().BATTLE_OK.Bind();
                    SPRITES.Cons().BIG.Dots.RenderCentered(r, 0, x - data.OffX1(), y - data.OffY1());
                }

                COLOR.Unbind();
                foreach (Div d in s.Selection())
                    GAME.ARMIES().Placer.Render(r, GetFor(d), data);
                return;
            }
        }

        public override void HoverTimer(GBox text)
        {
        }
    }
}