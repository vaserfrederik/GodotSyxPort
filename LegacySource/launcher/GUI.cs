using System;
using System.Collections.Generic;

namespace launcher
{
    public static class GUI
    {
        public static readonly COLOR c_hover = new ColorShifting(new ColorImp(127, 127, 65),
            new ColorImp(110, 90, 45));
        public static readonly COLOR c_selected = new ColorImp(80, 110, 65);
        public static readonly COLOR c_hover_selected = new ColorImp(100, 128, 80);
        public static readonly COLOR c_inactive = COLOR.BROWN;
        public static readonly COLOR c_unclickable = new ColorImp(110, 90, 45);
        public static readonly COLOR c_label = new ColorImp(127, 127, 65);
        public static readonly COLOR c_border = new ColorImp(200 / 2, 180 / 2, 160 / 2);

        public GUI(RES res)
        {
        }

        public abstract class Button : CLICKABLE.ClickableAbs
        {
            protected readonly SPRITE s;

            public Button(SPRITE s)
            {
                this.s = s;
                body.setWidth(s.width() + 10).setHeight(s.height() + 10);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive,
                bool isSelected, bool isHovered)
            {
                c_border.renderFrame(r, body, 0, 1);
                COLOR.BLACK.render(r, body, -1);
                (isHovered ? COLOR.WHITE35 : COLOR.WHITE15).render(r, body, -4);

                if (isSelected)
                {
                    COLOR.WHITE100.renderFrame(r, body, -2, 2);
                }

                s.renderCY(r, body().x1() + 5, body().cY());

                if (!isActive)
                {
                    OPACITY.O50.bind();
                    COLOR.BLACK.render(r, body, -4);
                    OPACITY.unbind();
                }
            }
        }

        public class BSprite : Button
        {
            public BSprite(SPRITE s) : base(s)
            {
            }
        }

        public class BSpriteBig : Button
        {
            public BSpriteBig(SPRITE s) : base(sp(s))
            {
            }

            private static SPRITE sp(SPRITE s)
            {
                // Implement logic similar to Java sp method
                return s; // Placeholder
            }
        }

        public class BText : Button
        {
            private readonly string text;

            public BText(string text, SPRITE icon) : base(icon)
            {
                this.text = text;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive,
                bool isSelected, bool isHovered)
            {
                base.render(r, ds, isActive, isSelected, isHovered);
                // Render text logic
            }
        }

        public class BIcon : Button
        {
            public BIcon(SPRITE icon) : base(icon)
            {
            }
        }

        public class BTextIcon : Button
        {
            private readonly string text;
            private readonly SPRITE icon;

            public BTextIcon(string text, SPRITE icon) : base(icon)
            {
                this.text = text;
                this.icon = icon;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive,
                bool isSelected, bool isHovered)
            {
                base.render(r, ds, isActive, isSelected, isHovered);
                // Render text and icon logic
            }
        }

        public class BIconText : Button
        {
            private readonly string text;
            private readonly SPRITE icon;

            public BIconText(string text, SPRITE icon) : base(icon)
            {
                this.text = text;
                this.icon = icon;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive,
                bool isSelected, bool isHovered)
            {
                base.render(r, ds, isActive, isSelected, isHovered);
                // Render icon and text logic
            }
        }

        public class ScrollBox : CLICKABLE
        {
            private readonly List<HOVERABLE> objects = new List<HOVERABLE>();
            private HOVERABLE hovered;
            private bool visable = true;
            private readonly Bounds bounds;

            public ScrollBox()
            {
                bounds = new Bounds();
            }

            public void clear()
            {
                objects.Clear();
                hovered = null;
                bounds.setWidth(0);
                bounds.setHeight(0);
            }

            public int add(HOVERABLE object)
            {
                if (objects.Count == 0)
                {
                    object.body().moveX1Y1(bounds.x1(), bounds.y1());
                }
                else
                {
                    object.body().moveX1Y1(bounds.x1(), objects[objects.Count - 1].body().y2());
                }

                int i = objects.Count;
                objects.Add(object);

                if (object.body().y2() <= bounds.y2())
                    bounds.setHeight(Math.Max(bounds.height(), object.body().y2()));

                if (object.body().width() > bounds.width())
                    bounds.setWidth(object.body().width());

                return i;
            }

            public void add(SPRITE s)
            {
                HOVERABLE.Sprite r = new HOVERABLE.Sprite(s);
                if (objects.Count == 0)
                {
                    r.body().moveX1Y1(bounds.x1(), bounds.y1());
                }
                else
                {
                    r.body().moveX1Y1(bounds.x1(), objects[objects.Count - 1].body().y2());
                }

                objects.Add(r);

                if (r.body().y2() <= bounds.y2())
                    bounds.setHeight(Math.Max(bounds.height(), r.body().y2()));

                if (r.body().width() > bounds.width())
                    bounds.setWidth(r.body().width());
            }

            protected override void render(SPRITE_RENDERER r, float ds)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    objects[i].render(r, ds);
                }
            }

            public override bool hover(COORDINATE mCoo)
            {
                if (hovered != null && hovered.hover(mCoo))
                {
                    return true;
                }
                else
                {
                    hovered = null;
                }

                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] is CLICKABLE g)
                    {
                        if (g.hover(mCoo))
                        {
                            hovered = g;
                            return true;
                        }
                    }
                }

                return false;
            }

            public override bool hoveredIs()
            {
                return hovered != null && hovered.hoveredIs();
            }

            public override bool click()
            {
                return hovered != null && hovered.click();
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                if (hovered != null && hovered.hoveredIs())
                    hovered.hoverInfoGet(text);
            }

            public override CLICKABLE hoverTitleSet(CharSequence s)
            {
                return null;
            }

            public override CLICKABLE activeSet(bool activate)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] is CLICKABLE g)
                        g.activeSet(activate);
                }
                return this;
            }

            public override CLICKABLE hoverSoundSet(SoundEffect sound)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] is CLICKABLE g)
                        g.hoverSoundSet(sound);
                }
                return this;
            }

            public override CLICKABLE clickSoundSet(SoundEffect sound)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] is CLICKABLE g)
                        g.clickSoundSet(sound);
                }
                return this;
            }

            public override CLICKABLE selectedSet(bool yes)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] is CLICKABLE g)
                        g.selectedSet(yes);
                }
                return this;
            }

            public override bool selectedIs()
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] is CLICKABLE g)
                        if (g.selectedIs())
                            return true;
                }
                return false;
            }

            public override bool activeIs()
            {
                return false;
            }

            public override CLICKABLE visableSet(bool yes)
            {
                visable = yes;
                return this;
            }

            public override bool visableIs()
            {
                return visable;
            }

            public override CLICKABLE selectedToggle()
            {
                // Implement logic similar to Java selectedToggle method
                return this;
            }

            public override Rec body()
            {
                return bounds;
            }

            public override CLICKABLE clickActionSet(ACTION f)
            {
                // Implement logic similar to Java clickActionSet method
                return null;
            }

            public override CLICKABLE selectTmp()
            {
                // Implement logic similar to Java selectTmp method
                return null;
            }

            public override CLICKABLE hoverInfoSet(CharSequence s)
            {
                // Implement logic similar to Java hoverInfoSet method
                return null;
            }

            private class Bounds : Rec
            {
                public override Rec moveX1(double X1)
                {
                    double dx = X1 - x1();
                    for (int i = 0; i < objects.Count; i++)
                    {
                        objects[i].body().incrX(dx);
                    }
                    return base.moveX1(X1);
                }

                public override Rec moveY1(double Y1)
                {
                    double dy = Y1 - y1();
                    for (int i = 0; i < objects.Count; i++)
                    {
                        objects[i].body().incrY(dy);
                    }
                    return base.moveY1(Y1);
                }
            }
        }
    }
}