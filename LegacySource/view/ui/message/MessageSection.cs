using System;
using System.Text;
using init.constant;
using init.sprite.UI;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using util.gui.misc;

namespace view.ui.message
{
    public abstract class MessageSection : Message
    {
        private static readonly long serialVersionUID = 1L;
        protected static readonly int WIDTH = 900;
        private static readonly int PM = C.SG * 10;
        private transient GuiSection section;

        public MessageSection(CharSequence title) : base(title)
        {
        }

        protected MessageSection paragraph(CharSequence text)
        {
            GText t = new GText(UI.FONT().M, text).clickify();
            t.setMaxWidth(WIDTH - 2 * PM);
            t.adjustWidth();
            section.add(t, 0, section.getLastY2() + PM);
            return this;
        }

        protected MessageSection paragraph(CharSequence text, Font font, COLOR color)
        {
            GText t = new GText(font, text).color(color);
            t.setMaxWidth(WIDTH - 2 * PM);
            t.adjustWidth();
            section.add(t, 0, section.getLastY2() + PM);
            return this;
        }

        protected override GuiSection makeSection()
        {
            section = new GuiSection();
            make(section);
            return section;
        }

        protected abstract void make(GuiSection section);
    }
}