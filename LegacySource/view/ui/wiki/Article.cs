using System;
using System.Collections.Generic;
using init.constant;
using init.sprite;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.sets;
using util.gui.misc;
using view.main;

namespace view.ui.wiki
{
    abstract class Article
    {
        public readonly string key;
        public readonly string title;
        public readonly string category;
        public readonly GuiSection section = new GuiSection
        {
            Render = (r, ds) =>
            {
                Colors.border.Render(r, Body(), -5);
                COLOR.WHITE05.Render(r, Body(), -6);
                base.Render(r, ds);
            }
        };
        public const int HEIGHT = C.HEIGHT() - WIKI.TOP_HEIGHT * 2 - 32;

        protected Article(string title, string category)
        {
            this.title = title;
            this.category = category;
            this.key = category + title;
        }

        protected void Init(List<Article> all, int width)
        {
            section.Body().SetWidth(width);
            section.Body().SetHeight(C.HEIGHT() - WIKI.TOP_HEIGHT - 32);
            GHeader h = new GHeader(title);
            h.Body().CenterX(section.Body());
            h.Body().CenterY(0, WIKI.TOP_HEIGHT);
            section.Add(h);
            GButt.Glow b = new GButt.Glow(SPRITES.icons().m.exit)
            {
                ClickA = () =>
                {
                    VIEW.UI().wiki.Remove(this);
                }
            };
            b.Body().CenterY(0, WIKI.TOP_HEIGHT);
            b.Body().MoveX2(width - 8);
            section.Add(b);
            GuiSection s = MakeSection(all, width - 48);
            s.Body().MoveY1(WIKI.TOP_HEIGHT + 6);
            s.Body().MoveX1(24);
            section.Add(s);
        }

        protected abstract GuiSection MakeSection(List<Article> all, int width);
    }
}