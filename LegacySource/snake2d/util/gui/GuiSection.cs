using System;
using System.Collections.Generic;

namespace snake2d.util.gui
{
    public class GuiSection : CLICKABLE
    {
        protected readonly List<RENDEROBJ> renderables = new List<RENDEROBJ>();

        private RECTANGLE previous;
        private HOVERABLE hovered;
        protected CLICKABLE clicked;

        private ACTION clickAction;

        private bool visable = true;
        protected bool active = true;

        private Bounds bounds = new Bounds();

        protected bool hoveredIs = false;

        private string hoverInfo = null;
        private string hoverTitle = null;

        public GuiSection()
        {
            previous = bounds;
        }

        public GuiSection(float x, float y)
        {
            bounds.moveX1Y1(x, y);
            previous = bounds;
        }

        public GuiSection(HOVERABLE r)
        {
            bounds.set(r.body());
            add(r);
        }

        public GuiSection(CLICKABLE g)
        {
            bounds.set(g.body());
            renderables.Add(g);
            previous = g.body();
            hovered = g;
        }

        public GuiSection(SPRITE s, int x, int y)
        {
            bounds.set(x, x, y, y);
            add(s, x, y);
        }

        public void clear()
        {
            renderables.Clear();
            hovered = null;
            clicked = null;
            hoveredIs = false;
            clickAction = null;
            bounds.setWidth(0).setHeight(0);
            bounds.moveX1Y1(0, 0);
            previous = bounds;
        }

        public List<RENDEROBJ> elements()
        {
            return renderables;
        }

        public void pad(int margin)
        {
            body().incrW(margin * 2);
            body().incrH(margin * 2);
            foreach (RENDEROBJ r in renderables)
                r.body().incrX(margin).incrY(margin);
        }

        public void pad(int mx, int my)
        {
            body().incrW(mx * 2);
            body().incrH(my * 2);
            foreach (RENDEROBJ r in renderables)
                r.body().incrX(mx).incrY(my);
        }

        public void padX(int left, int right)
        {
            body().incrX(left);
            body().incrW(right);
            foreach (RENDEROBJ r in renderables)
                r.body().incrX(right);
        }

        public override bool hover(COORDINATE mCoo)
        {
            if (!visable)
                return false;

            hoveredIs = mCoo.isInside(bounds);

            foreach (var ren in renderables)
            {
                if (ren.hover(mCoo))
                {
                    hovered = ren as HOVERABLE;
                    break;
                }
            }

            return hoveredIs;
        }

        public override bool click(COORDINATE mCoo)
        {
            if (!active || !visable)
                return false;

            foreach (var ren in renderables)
            {
                if (ren.click(mCoo))
                {
                    clicked = ren as CLICKABLE;
                    return true;
                }
            }

            return false;
        }

        public override void render(RENDER_CONTEXT context)
        {
            foreach (var ren in renderables)
                ren.render(context);
        }

        protected void clickCallback()
        {
            if (clicked != null && clickAction != null)
                clickAction.run();
        }

        protected void hoverInfoSelf(GUI_BOX box)
        {
            if (hoverInfo != null)
                box.text(hoverInfo);
            if (hoverTitle != null)
                box.title(hoverTitle);
        }

        private class Bounds : Rec
        {
            public override Rec moveX1(double X1)
            {
                int dx = (int)(X1 - x);

                foreach (RENDEROBJ ren in renderables)
                {
                    if (ren.body() == this)
                        throw new Exception();
                    ren.body().incrX(dx);
                }
                x = X1;
                moveCallback();
                return this;
            }

            public override Rec moveY1(double Y1)
            {
                int dy = (int)(Y1 - y);

                foreach (RENDEROBJ ren in renderables)
                    ren.body().incrY(dy);
                y = Y1;
                moveCallback();
                return this;
            }
        }

        protected void moveCallback()
        {
        }

        public void merge(GuiSection section)
        {
            foreach (RENDEROBJ r in section.renderables)
                renderables.Add(r);
            if (body().width() == 0 && body().height() == 0)
                body().set(section);
            else
                body().unify(section.body());
            previous = section.body();
        }

        public void absorb(GuiSection section)
        {
            foreach (RENDEROBJ r in section.renderables)
                add(r);
            body().unify(section.body());
        }

        public override Rec body()
        {
            return bounds;
        }

        public GuiSection hoverInfoSet(string s)
        {
            this.hoverInfo = s;
            return this;
        }

        public CLICKABLE hoverTitleSet(string s)
        {
            this.hoverTitle = s;
            return this;
        }

        public CLICKABLE clickActionSet(ACTION f)
        {
            this.clickAction = f;
            return this;
        }

        protected HOVERABLE hovered()
        {
            return hovered;
        }

        public GuiSection addRelBody(int m, DIR e, RENDEROBJ ren)
        {
            int cx = body().cX() + e.x() * ((ren.body().width() + body().width()) / 2 + m);
            int cy = body().cY() + e.y() * ((ren.body().height() + body().height()) / 2 + m);
            ren.body().moveC(cx, cy);
            add(ren);
            return this;
        }

        public GuiSection addRelBody(int m, DIR e, SPRITE ren)
        {
            return addRelBody(m, e, new RENDEROBJ.Sprite(ren));
        }
    }
}