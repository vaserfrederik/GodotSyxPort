using System;
using System.Text;
using init.constant;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.gui.misc;
using util.gui.panel;
using util.text;
using view.keyboard;

namespace view.interrupter
{
    public sealed class ITextInput : Interrupter
    {
        private readonly GuiSection s = new GuiSection();
        private readonly Str title = new Str(64);

        private readonly GInput in;
        private STRING_RECIEVER client;
        private readonly InterManager m;

        public ITextInput(InterManager m)
        {
            this.m = m;

            s.AddDownC(0, new SPRITE.Imp(400, UI.FONT().H2.height() * 2)
            {
                public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    GCOLOR.T().H1.bind();
                    UI.FONT().H2.renderIn(r, X1, Y1, DIR.C, title, width(), height(), 1);
                    COLOR.unbind();
                }
            });

            StringInputSprite input = new StringInputSprite(48, UI.FONT().M);
            in = new GInput(input);

            s.AddRelBody(16, DIR.S, in);

            GuiSection buttons = new GuiSection();

            buttons.Add(new GButt.ButtPanel(UI.icons().m.ok)
            {
                protected override void clickA()
                {
                    hide();
                    client.acceptString(input.text());
                }
            }.hoverTitleSet(Dic.¤¤OK));

            buttons.AddRightC(0, new GButt.ButtPanel(UI.icons().m.cancel)
            {
                protected override void clickA()
                {
                    hide();
                    client.acceptString(null);
                }
            }.hoverTitleSet(Dic.¤¤cancel));

            s.AddRelBody(16, DIR.S, buttons);

            s.pad(8, 8);

            GPanel p = new GPanel(s.body());
            p.setBig();

            s.Add(p);
            s.moveLastToBack();

            s.body().centerIn(C.DIM());
        }

        protected override bool render(Renderer r, float ds)
        {
            in.listen();
            s.render(r, ds);

            return true;
        }

        public void requestInput(STRING_RECIEVER client, CharSequence title)
        {
            requestInput(client, title, null);
        }

        public void requestInput(STRING_RECIEVER client, CharSequence title, CharSequence placeholder)
        {
            this.title.clear().add(title);
            this.client = client;
            in.text().clear();
            if (placeholder != null)
                in.text().add(placeholder);
            in.focus();
            base.show(m);
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            s.hover(mCoo);
            return true;
        }

        protected override void hoverTimer(GBox text)
        {
            s.hoverInfoGet(text);
        }

        protected override void mouseClick(MButt button)
        {
            s.click();
        }

        protected override bool update(float ds)
        {
            if (KEYS.MAIN().ESCAPE.consumeClick() || MButt.RIGHT.isDown())
            {
                client.acceptString(null);
                hide();
            }
            else if (KEYS.MAIN().ENTER.consumeClick())
            {
                hide();
                client.acceptString(in.text());
            }
            KEYS.clear();

            return false;
        }
    }
}