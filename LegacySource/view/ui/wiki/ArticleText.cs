using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace View.Ui.Wiki
{
    class ArticleText : Article
    {
        private readonly ICharSequence text;
        private readonly List<Special> links = new List<Special>();

        public ArticleText(Json json, KeyMap<Article> amap) : base(json.text("NAME"), json.text("CATEGORY"))
        {
            if (json.has("LINK_KEY"))
            {
                string k = json.value("LINK_KEY");
                if (amap.ContainsKey(k))
                    json.error("this link key already exists", k);
                amap.Add(k, this);
            }

            string t = json.text("TEXT");
            StringBuilder bu = new StringBuilder();

            for (int ci = 0; ci < t.Length; ci++)
            {
                if (t[ci] == '<')
                {
                    int ei = ci;
                    ci++;
                    string error = "either <a LINK_KEY TEXT> (wiki link), <c RRR_GGG_BBB TEXT> (colored text) or <u TEXT> (underscore)";

                    if (ci >= t.Length)
                    {
                        Err(json, t, error, ei);
                        continue;
                    }

                    if (t[ci] == 'a')
                    {
                        error = "Expecting: <a LINK_KEY TEXT> where LINK_KEY is another wiki entry's link, and TEXT is the text for the link.";
                        ci = Next(t, ci, ' ') + 1;
                        if (ci < 0)
                        {
                            Err(json, t, error, ei);
                            continue;
                        }
                        int ni = Next(t, ci, ' ');
                        if (ni < 0)
                        {
                            Err(json, t, error, ei);
                            continue;
                        }
                        string key = t.Substring(ci, ni - ci);
                        ci = ni + 1;
                        ni = Next(t, ci, '>');
                        if (ni < 0)
                        {
                            Err(json, t, error, ei);
                            continue;
                        }
                        string content = t.Substring(ci, ni - ci);
                        int pos = bu.Length;
                        bu.Append(content);
                        links.Add(new LinkButt(pos, bu, content, key));
                        ci = ni;
                    }
                    else if (t[ci] == 'c')
                    {
                        error = "Expecting: <c RRR_GGG_BBB TEXT> where RRR_GGG_BBB is a color, and TEXT is the text for the link.";
                        ci = Next(t, ci, ' ') + 1;
                        if (ci < 0)
                        {
                            Err(json, t, error, ei);
                            continue;
                        }
                        int ni = Next(t, ci, ' ');
                        if (ni < 0)
                        {
                            Err(json, t, error, ei);
                            continue;
                        }
                        string key = t.Substring(ci, ni - ci);
                        Color col = Color.White;
                        try
                        {
                            col = new ColorImp().Set(key, json);
                        }
                        catch (Exception e)
                        {
                            Err(json, t, error, ei);
                            Console.WriteLine(e);
                            continue;
                        }

                        ci = ni + 1;
                        ni = Next(t, ci, '>');
                        if (ni < 0)
                        {
                            Err(json, t, error, ei);
                            continue;
                        }
                        string content = t.Substring(ci, ni - ci);
                        int pos = bu.Length;
                        bu.Append(content);
                        links.Add(new ColButt(pos, bu, content, col));
                        ci = ni;
                    }
                    else
                    {
                        Err(json, t, error, ei);
                        continue;
                    }

                }
                else
                {
                    bu.Append(t[ci]);
                }
            }

            text = bu.ToString();
        }

        private int Next(string t, int ci, char c)
        {
            for (; ci < t.Length; ci++)
            {
                if (t[ci] == c)
                    return ci;
            }
            return -10;
        }

        private void Err(Json json, string t, string error, int ci)
        {
            GAME.Warn(json.errorGet("TEXT", "Error in wiki entry. " + error + " Around: '" + t.Substring(Math.Max(ci - 8, 0), Math.Min(ci + 32, t.Length - 1)) + "'"));
        }

        public override GuiSection MakeSection(IList<Article> all, int width)
        {
            return new WikiArticle(this, width, all);
        }


        internal sealed class WikiArticle : GuiSection
        {
            private static Special hovered;

            private int row;
            private readonly int max;
            private readonly IList<Special> linkButts;
            private readonly ArticleText e;

            public WikiArticle(ArticleText e, int width, IList<Article> all) : base()
            {
                this.e = e;
                Font f = UI.FONT().M;

                body().setWidth(width);

                int maxRows = (HEIGHT) / f.height();
                int rows = UI.FONT().M.GetRowAmount(e.text, width - 32);

                int m = rows - maxRows;
                if (m > 0) m = 0;
                max = m;

                linkButts = e.links;
            }

            public override void Render(SPRITE_RENDERER r)
            {
                base.Render(r);
                Font f = UI.FONT().M;
                int x = body().x();
                int y = body().y();

                foreach (var special in linkButts)
                {
                    if (special.position >= row && special.position < row + max)
                    {
                        special.render(r, hovered == special, special.text, x, y);
                        y += f.height();
                    }
                }
            }

            public override bool Click()
            {
                if (hovered != null)
                {
                    hovered.click();
                    return true;
                }
                return base.Click();
            }

            public override bool Hover(int x, int y)
            {
                Font f = UI.FONT().M;
                int startX = body().x();
                int startY = body().y();

                foreach (var special in linkButts)
                {
                    if (special.position >= row && special.position < row + max)
                    {
                        int specialY = startY + (special.position - row) * f.height();
                        if (x >= startX && x <= startX + f.width(special.text, 0, special.text.Length, 1.0) && y >= specialY && y <= specialY + f.height())
                        {
                            hovered = special;
                            return true;
                        }
                    }
                }

                hovered = null;
                return base.Hover(x, y);
            }

            public override bool Scroll(int direction)
            {
                if (direction < 0 && row > 0)
                {
                    row--;
                }
                else if (direction > 0 && row + max < e.text.Length)
                {
                    row++;
                }
                return true;
            }
        }

        private abstract class Special
        {
            public readonly int position;
            public readonly ICharSequence text;

            public Special(int position, ICharSequence all, ICharSequence text)
            {
                this.text = text;
                this.position = position;
            }

            public abstract void render(SPRITE_RENDERER r, bool hovered, ICharSequence text, int x1, int y1);

            public abstract void click();
        }

        private class LinkButt : Special
        {
            private readonly string key;

            public LinkButt(int position, ICharSequence all, ICharSequence text, string key) : base(position, all, text)
            {
                this.key = key;
            }

            public override void render(SPRITE_RENDERER r, bool hovered, ICharSequence text, int x1, int y1)
            {
                Article entry = WIKI.links.Get(key);

                if (hovered)
                    Color.White.Bind();
                else if (entry == null)
                    GCOLOR.T().IBAD.Bind();
                else
                    GCOLOR.T().IGOOD.Bind();
                UI.FONT().M.render(r, text, x1, y1);
            }

            public override void click()
            {
                Article entry = WIKI.links.Get(key);
                if (entry != null)
                {
                    VIEW.UI().wiki.set(entry);
                }
            }
        }

        private class ColButt : Special
        {
            private readonly Color col;

            public ColButt(int position, ICharSequence all, ICharSequence text, Color color) : base(position, all, text)
            {
                this.col = color;
            }

            public override void render(SPRITE_RENDERER r, bool hovered, ICharSequence text, int x1, int y1)
            {
                col.Bind();
                UI.FONT().M.render(r, text, x1, y1);
            }

            public override void click()
            {
            }
        }
    }
}