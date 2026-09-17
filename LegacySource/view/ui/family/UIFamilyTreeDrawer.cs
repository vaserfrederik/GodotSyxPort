using System;
using System.Collections.Generic;
using System.Linq;
using Init.Constant;
using Init.Race.Appearance;
using Init.Sprite;
using Init.Sprite.UI;
using Settlement.Stats;
using Settlement.Stats.Relation;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.Colors;
using Snake2D.Util.DataTypes;
using Util.Colors;
using View.Keyboard;
using View.Main;

namespace View.UI.Family
{
    class UIFamilyTreeDrawer
    {
        private readonly StatsRelations r = STATS.REL();
        private const int ww = RPortrait.P_WIDTH + 10;
        private const int hh = RPortrait.P_HEIGHT + 10;
        private const int MX = 12;
        private const int MY = 32;

        private int hoverRef = -1;
        private int hoverRef2 = -1;
        private readonly Window window = new Window();
        private readonly Rec body = new Rec(ww, hh);

        void Init(int refId, UIFamilyTreeRefs refs, UIFamilyTreeAligner poss)
        {
            hoverRef = -1;
            hoverRef2 = -1;
            window.Init(refId, refs, poss);
        }

        public void Drag()
        {
            window.Dragging = true;
            window.ClickedCoo.Set(VIEW.Mouse());
        }

        public void Draw(double ds, UIFamilyTreeRefs refs, UIFamilyTreeAligner poss, int primaryRef)
        {
            hoverRef = hoverRef2;
            hoverRef2 = -1;

            window.Update(ds);
            window.RenderBG();

            for (int refI = 0; refI < refs.Max(); refI++)
            {
                double dx = poss.X2(refI);
                double level = poss.Level(refI);
                double parentX = poss.ParentX(refI);

                DrawRelations(refs.Get(refI), dx, level, parentX);
            }

            for (int refI = 0; refI < refs.Max(); refI++)
            {
                double dx = poss.X2(refI);
                double level = poss.Level(refI);

                int refId = refs.Get(refI);

                int x = window.Sx(dx);
                int y = window.Sy(level);
                body.MoveX1Y1(x, y);

                if (body.HoldsPoint(VIEW.Mouse()))
                {
                    hoverRef = refId;
                    hoverRef2 = refId;
                }
                bool expand = refs.HasChild(refI);
                DrawFrame(refId, primaryRef);
                Draw(refId, expand);
            }
        }

        private void DrawFrame(int refId, int primaryRef)
        {
            bool hovered = hoverRef == refId;

            int scale = window.Scale;

            GCOLOR.UI().Bg(hovered || STATS.REL().Indu(refId) != null, false, hovered).Render(CORE.Renderer(), body);

            GCOLOR.UI().Border().RenderFrame(CORE.Renderer(), body, -scale, scale);

            if (refId == primaryRef)
            {
                COLOR.WHITE25.RenderFrame(CORE.Renderer(), body, 2, 1);
                COLOR.WHITE100.RenderFrame(CORE.Renderer(), body, 3, 1);
                COLOR.WHITE50.RenderFrame(CORE.Renderer(), body, 4, 1);
                COLOR.WHITE25.RenderFrame(CORE.Renderer(), body, 5, 1);
            }
        }

        private void Draw(int refId, bool expand)
        {
            int scale = window.Scale;
            if (STATS.REL().Indu(refId) != null)
            {
                int x1 = body.X1() + 5 * scale;
                int y1 = body.Y1() + 4 * scale;
                STATS.APPEARANCE().PortraitRender(CORE.Renderer(), STATS.REL().Indu(refId), x1, y1, scale);
            }
            else if (STATS.REL().Race(refId) != null)
            {
                COLOR.WHITE65.Bind();
                STATS.REL().Race(refId).Appearance().IconBig.RenderCScaled(CORE.Renderer(), body.CX(), body.CY(), scale);
                COLOR.Unbind();
            }

            if (expand)
            {
                UI.Icons().S.Expand.RenderCScaled(CORE.Renderer(), body.X2() - scale * 8, body.Y2() - scale * 8, window.Scale);
            }
        }

        private void DrawRelations(int refId, double dx, double level, double parentdx)
        {
            bool active = false;
            if (r.HasParent(refId) && hoverRef == r.ParentRef(refId))
            {
                active = true;
            }
            if (r.HasParent(hoverRef) && r.HasParent(refId) && r.ParentRef(refId) == r.ParentRef(hoverRef))
            {
                active = true;
            }

            COLOR col = active ? COLOR.WHITE85 : COLOR.WHITE35;

            int y = window.Sy(level);

            int d = window.Scale * 2;
            int dy = MY * window.Scale;
            int cx = window.Cx(dx);
            if (parentdx >= 0)
            {
                col.Render(CORE.Renderer(), cx - d, cx + d, y - dy / 2, y);

                int px = window.Cx(parentdx);

                col.Render(CORE.Renderer(), px - d, px + d, y - dy, y - dy / 2);

                int x1 = Math.Min(cx, px);
                int xx2 = Math.Max(cx, px);

                col.Render(CORE.Renderer(), x1 - d, xx2 + d, y - dy / 2 - d, y - dy / 2 + d);
            }
        }

        public int Hovered()
        {
            return hoverRef;
        }

        private class Window
        {
            public readonly Rec View = new Rec(C.WIDTH(), C.HEIGHT());
            public int Scale = 1;
            private bool Dragging = false;
            private Coo ClickedCoo = new Coo();
            private readonly Rec Max = new Rec();
            private readonly COLOR Bg = new ColorImp(35, 74, 80);

            void Init(int refId, UIFamilyTreeRefs refs, UIFamilyTreeAligner poss)
            {
                for (int i = 0; i < refs.Max(); i++)
                {
                    double cx = poss.X2(i) * Scale * (ww + MX) + Scale * ww / 2;
                    double cy = poss.Level(i) * Scale * (hh + MY) + Scale * hh / 2;
                    if (i == 0)
                    {
                        Max.MoveX1Y1(cx, cy);
                        Max.SetDim(0);
                    }
                    Max.Unify((int)cx, (int)cy);
                    if (refId == refs.Get(i))
                    {
                        View.MoveC(cx, cy);
                    }
                }

                Max.IncrX(-C.WIDTH() + 50);
                Max.IncrW(C.WIDTH() * 2 - 100);
                Max.IncrY(-C.HEIGHT() + 50);
                Max.IncrH(C.HEIGHT() * 2 - 100);
            }

            public void Update(double ds)
            {
                Dragging &= MButt.LEFT.IsDown();

                double acc = 2000;

                if (KEYS.MAIN().SCROLL_LEFT.IsPressed())
                {
                    View.IncrX(-acc * ds);
                }
                else if (KEYS.MAIN().SCROLL_RIGHT.IsPressed())
                {
                    View.IncrX(acc * ds);
                }
                if (KEYS.MAIN().SCROLL_UP.IsPressed())
                {
                    View.IncrY(-acc * ds);
                }
                else if (KEYS.MAIN().SCROLL_DOWN.IsPressed())
                {
                    View.IncrY(acc * ds);
                }

                if (Dragging)
                {
                    int dx = ClickedCoo.X() - VIEW.Mouse().X();
                    int dy = ClickedCoo.Y() - VIEW.Mouse().Y();
                    View.Incr(dx, dy);
                    ClickedCoo.Set(VIEW.Mouse());
                }

                if (View.X1() < Max.X1())
                    View.MoveX1(Max.X1());
                else if (View.X2() > Max.X2())
                    View.MoveX2(Max.X2());
                if (View.Y1() < Max.Y1())
                    View.MoveY1(Max.Y1());
                else if (View.Y2() > Max.Y2())
                    View.MoveY2(Max.Y2());
            }

            int Sx(double x)
            {
                return (int)(x * Scale * (ww + MX) - View.X1());
            }

            int Sy(double y)
            {
                return (int)(y * Scale * (hh + MY) - View.Y1());
            }

            int Cx(double x)
            {
                return (int)(x * Scale * (ww + MX) - View.X1() + Scale * ww / 2);
            }

            private void RenderBG()
            {
                int dim = C.TILE_SIZE;

                OPACITY.O25.Bind();
                Bg.Bind();

                for (int sy = 0; sy < C.HEIGHT(); sy += dim)
                {
                    for (int sx = 0; sx < C.WIDTH(); sx += dim)
                    {
                        double tx = (sx + View.X1()) / 4;
                        double ty = (sy + View.Y1()) / 4;
                        CORE.Renderer().RenderSprite(sx, sx + dim, sy, sy + dim, SPRITES.Textures().Dis_Big.Get(tx, ty));
                        tx = (sx + View.X1() + 128) / 4;
                        ty = (sy + View.Y1() + 128) / 4;
                        CORE.Renderer().RenderSprite(sx, sx + dim, sy, sy + dim, SPRITES.Textures().Dis_Low.Get(tx, ty));
                    }
                }

                OPACITY.Unbind();
            }
        }
    }
}