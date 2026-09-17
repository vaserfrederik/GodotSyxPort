using System;
using System.Collections.Generic;
using System.Linq;

namespace snake2d.util.sprite.text
{
    class StringInputSprite : CHAR_LISTENER, SPRITE
    {
        private Font f;
        private static readonly string promt = "|" + "";
        private CharSequence placeholder;
        public int marker = 0;
        private static readonly Str tmp = new Str(512);

        private int selectedI = -1;

        public StringInputSprite(int size, Font font) : base(size)
        {
            this.f = font;
        }

        public StringInputSprite placeHolder(CharSequence ph)
        {
            this.placeholder = ph;
            return this;
        }

        public StringInputSprite font(Font f)
        {
            this.f = f;
            return this;
        }

        public Font font()
        {
            return f;
        }

        protected override void acceptChar(char c)
        {
            if (!listening())
                return;
            if (selectedI >= 0 && selectedI != marker())
            {
                int s = Math.Min(selectedI, marker);
                int e = Math.Max(selectedI, marker);
                tmp.clear().add(text());
                Str tt = text();
                tt.clear();
                for (int i = 0; i < tmp.length(); i++)
                {
                    if (i == s)
                    {
                        tt.add(c);
                    }
                    else if (i < s || i > e)
                        tt.add(tmp.charAt(i));
                }
                marker = s + 1;
            }
            else if (text().spaceLeft() > 0)
            {
                if (marker() == text().length())
                {
                    text().add(c);

                }
                else
                {
                    tmp.clear().add(text());
                    text().clear();
                    int k = 0;
                    for (int i = 0; i < tmp.length(); i++)
                    {
                        if (k++ == marker)
                        {
                            text().add(c);
                            i--;
                        }
                        else
                        {
                            text().add(tmp.charAt(i));
                        }
                    }
                }
                marker++;

            }
            change();

            selectedI = -1;
        }

        protected override void backspace()
        {

            if (!listening())
                return;
            if (text().length() == 0)
                return;
            int m = marker();
            if (removeSelected())
            {
                ;
            }
            else if (m > 0)
            {

                tmp.clear().add(text());
                Str tt = text();
                tt.clear();
                for (int i = 0; i < tmp.length(); i++)
                {
                    if (i + 1 == m)
                    {
                        ;
                    }
                    else
                    {
                        tt.add(tmp.charAt(i));
                    }
                }
                marker--;

            }
            selectedI = -1;
            change();
        }

        public override void del()
        {
            if (!listening())
                return;
            if (text().length() == 0)
                return;
            int m = marker();
            if (removeSelected())
            {
                ;
            }
            else if (m > 0)
            {

                tmp.clear().add(text());
                Str tt = text();
                tt.clear();
                for (int i = 0; i < tmp.length(); i++)
                {
                    if (i == m)
                    {
                        ;
                    }
                    else
                    {
                        tt.add(tmp.charAt(i));
                    }
                }
            }
            selectedI = -1;
            change();
        }

        private bool removeSelected()
        {
            if (!listening())
                return false;
            if (text().length() == 0)
                return false;
            int m = marker();
            if (selectedI >= 0 && selectedI != m)
            {
                int s = Math.Min(selectedI, marker);
                int e = Math.Max(selectedI, marker);
                tmp.clear().add(text());
                Str tt = text();
                tt.clear();
                for (int i = 0; i < tmp.length(); i++)
                {
                    if (i < s || i >= e)
                        tt.add(tmp.charAt(i));
                }
                return true;
            }
            return false;
        }

        public override void left(bool mod)
        {
            if (mod)
            {
                if (selectedI >= 0)
                    selectedI--;
                else
                    selectedI = marker - 1;
                if (selectedI < 0)
                    selectedI = 0;
            }
            else if (selectedI >= 0)
            {
                marker = selectedI;
                selectedI = -1;
            }
            else
            {
                selectedI = -1;
                marker--;
                if (marker < 0)
                    marker = 0;
            }
        }

        public override void right(bool mod)
        {
            if (mod)
            {
                if (selectedI >= 0)
                    selectedI++;
                else
                    selectedI = marker + 1;
                if (selectedI > text().length())
                    selectedI = text().length();
            }
            else if (selectedI >= 0)
            {
                marker = selectedI;
                selectedI = -1;
            }
            else
            {

                marker++;
                if (marker > text().length())
                    marker = text().length();
            }

        }

        public void click(int x1)
        {
            marker = findX(x1);
            selectedI = -1;
        }

        public void select(int x)
        {
            selectedI = -1;
            selectedI = findX(x);
        }

        public void selectAll()
        {
            marker = 0;
            selectedI = text().length();
        }

        private int findX(int x1)
        {
            int x = 0;
            int m = marker();
            if (selectedI >= 0)
                selectedI = findX(x1);
            else
                selectedI = -1;

            return selectedI;
        }

        private int width()
        {
            return f.width(text());
        }

        private int height()
        {
            return f.height();
        }

        public void renAction()
        {

        }

        protected override void change()
        {

        }

        public void renderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
        {
            // TODO Auto-generated method stub

        }

        public InputClickable c(DIR d)
        {
            return new InputClickable(this, d);
        }

        public class InputClickable : CLICKABLE.ClickableAbs
        {
            private readonly StringInputSprite input;
            private COLOR hoverC = COLOR.WHITE2WHITE;
            private COLOR color = COLOR.WHITE100;
            private COLOR active = color.shade(0.7);
            private DIR rep;

            InputClickable(StringInputSprite input, DIR rep) : base()
            {
                this.input = input;
                while (input.text().spaceLeft() > 0)
                {
                    input.text().add('n');
                }
                int w = input.width();
                body.setWidth(w);
                body.setHeight(input.height());
                input.text().clear();
                this.rep = rep;
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {

                input.renAction();
                if (Mouse.currentClicked == this)
                    input.listen();

                int dx = (body().width() - input.width()) / 2;

                int x1 = body().x1() + (rep.x() + 1) * dx;
                if (!isActive)
                    active.bind();
                if (isHovered || Mouse.currentClicked == this)
                    hoverC.bind();
                else
                    color.bind();
                input.render(r, x1, body.y1());
                COLOR.unbind();
            }

            public override bool click()
            {
                if (base.click())
                {
                    Mouse.currentClicked = this;
                    input.listen();
                    input.marker = input.text().length();
                    return true;
                }
                return false;
            }

            public void focus()
            {
                Mouse.currentClicked = this;
                input.listen();
            }

            public InputClickable colors(COLOR normal, COLOR hover)
            {
                this.hoverC = hover;
                this.color = normal;
                this.active = color.shade(0.7);
                return this;
            }
        }
    }
}