using System;
using System.Collections.Generic;
using Init.Constant;
using Init.Tech;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.DataTypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Misc;
using Snake2D.Util.Sprite;
using Util.Data.INT;
using Util.Gui.Slider;
using View.Keyboard;
using View.Main;

namespace View.UI.Tech
{
    internal class Tree : GuiSection
    {
        private readonly NodeCreator rows;
        private GuiSection content = new GuiSection();
        private readonly int dh;
        private readonly int dw;
        private bool dragging = false;
        private Coo dragCoo = new Coo();
        private Coo dragXY = new Coo();

        public Tree(TechTree tree, int height, int width)
        {
            width -= 24;

            Body.SetDim(width, height);
            Add(content);

            rows = new NodeCreator(tree);

            foreach (RENDEROBJ rr in rows.Rows)
            {
                content.AddDown(0, rr);
            }

            dh = content.Body().Height() - height;
            dw = content.Body().Width() - width;

            Add(new SPRITE.Imp(0)
            {
                public void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    COLOR.WHITE15.Render(r, 0, C.WIDTH(), 0, Body().Y1() - 16);
                    if (dh > 0)
                    {
                        COLOR.WHITE15.Render(r, Body().X2() - 32, C.WIDTH(), 0, C.HEIGHT());
                    }

                    if (dw > 0)
                    {
                        COLOR.WHITE15.Render(r, 0, C.WIDTH(), Body().Y2() - 32, C.HEIGHT());
                    }
                }
            }, 0, 0);

            if (dh > 0)
            {
                INTE ii = new INTE()
                {
                    public int Min() => 0,
                    public int Max() => dh,
                    public int Get() => Body().Y1() - content.Body().Y1(),
                    public void Set(int t) => content.Body().MoveY1(Body().Y1() - t)
                };

                GSliderVer sl = new GSliderVer(ii, height - 32);
                sl.Body().MoveY1(Body().Y1());
                sl.Body().MoveX2(Body().Width() - 6);

                Add(sl);
            }

            if (dw > 0)
            {
                IntImp ii = new IntImp(0, dw)
                {
                    public void Set(int t)
                    {
                        t = CLAMP.i(t, 0, dw);
                        content.Body().MoveX1(-t);
                        base.Set(t);
                    }
                };
                GSliderHor sl = new GSliderHor(ii, width);
                sl.Body().MoveY2(Body().Y2() - 6);
                sl.Body().MoveX1(Body().X1());

                Add(sl);
            }

            Adjust(0);
        }

        protected override void MoveCallback()
        {
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            dragging &= MButt.LEFT.IsDown();

            if (dragging)
            {
                if (dw > 0)
                {
                    int x1 = dragXY.X() + (VIEW.Mouse().X() - dragCoo.X());
                    x1 = CLAMP.i(x1, Body().X1() - dw, Body().X1());
                    content.Body().MoveX1(x1);
                }
                if (dh > 0)
                {
                    int y1 = dragXY.Y() + (VIEW.Mouse().Y() - dragCoo.Y());
                    y1 = CLAMP.i(y1, Body().Y1() - dh, Body().Y1());
                    content.Body().MoveY1(y1);
                }
            }
            else if (Body().HoldsPoint(VIEW.Mouse()))
            {
                double d = MButt.ClearWheelSpin() * Node.HEIGHT();
                if (KEYS.MAIN().MOD.IsPressed())
                {
                    if (dw > 0)
                    {
                        int x1 = (int)(content.Body().X1() + d);
                        x1 = CLAMP.i(x1, Body().X1() - dw, Body().X1());
                        content.Body().MoveX1(x1);
                        d = 0;
                    }
                }
                if (dh > 0)
                {
                    int y1 = (int)(content.Body().Y1() + d);
                    y1 = CLAMP.i(y1, Body().Y1() - dh, Body().Y1());
                    content.Body().MoveY1(y1);
                }
            }

            base.Render(r, ds);
        }

        public override bool Click()
        {
            if (!base.Click() || (content.HoveredIs() && content.GetHovered() is RENDEROBJ))
            {
                if (HoveredIs())
                {
                    dragging = true;
                    dragCoo.Set(VIEW.Mouse());
                    dragXY.Set(content.Body().X1(), content.Body().Y1());
                }
            }
            return true;
        }

        private void Adjust(int fr)
        {
            int x1 = content.Body().X1();
            int y1 = content.Body().Y1();
            int w = content.Body().Width();
            int h = content.Body().Height();
            content.Clear();
            content.Body().SetDim(w, h);
            content.Body().MoveX1Y1(x1, y1);
            int y = y1;

            {
                int dy = 0;
                for (int i = 0; i < fr; i++)
                {
                    dy += rows.Rows[i].Body().Height();
                }
                y -= dy;
            }

            foreach (RENDEROBJ rr in rows.Rows)
            {
                int hi = rr.Body().Height();

                rr.Body().MoveX1(x1);
                rr.Body().MoveY1(y);

                if (y >= y1 && y + hi <= content.Body().Y2())
                    content.Add(rr);

                y += hi;
            }
        }
    }
}