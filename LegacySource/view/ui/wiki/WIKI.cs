using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using snake2d;
using util.colors;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;
using init.constant;
using init.paths;
using init.race;
using System.Linq;

namespace view.ui.wiki
{
    public class WIKI : Interrupter
    {
        public const int TOP_HEIGHT = 0;
        public const int WIDTH = C.WIDTH() - 32;
        public const int HEIGHT = C.HEIGHT() - 16;

        private readonly GuiSection section = new GuiSection();
        public static readonly string ¤¤name = "Tome of Knowledge";

        private static readonly List<Article> articles = new List<Article>(1024);
        private List<Article> added;
        private GuiSection sAdded = new GuiSection();
        private readonly Article[] race;
        private readonly WikiList list;

        private static readonly Dictionary<string, Article> links = new Dictionary<string, Article>();

        static WIKI()
        {
            D.ts(typeof(WIKI));
            new GameDisposable
            {
                protected override void Dispose()
                {
                    articles.Clear();
                    links.Clear();
                }
            };
        }

        public static ACTION add(Json json)
        {
            if (!json.HasProperty("WIKI"))
                return null;
            json = json["WIKI"];
            return padd(json);
        }

        public static ACTION padd(Json json)
        {
            ArticleText a = new ArticleText(json, links);
            articles.Add(a);
            return new ACTION
            {
                public void Exe()
                {
                    VIEW.UI().wiki.Activate();
                    VIEW.UI().wiki.Set(a);
                }
            };
        }

        public WIKI()
        {
            new Colors();

            {
                PATH p = PATHS.TEXT().GetFolder("wiki");
                string[] files = p.GetFiles();
                foreach (string f in files)
                {
                    Json j = new Json(p.Get(f));
                    if (j.HasProperty("WIKI"))
                    {
                        add(j);
                    }
                    if (j.HasProperty("WIKIS"))
                    {
                        Json[] json = j.Jsons("WIKIS");
                        foreach (Json jj in json)
                        {
                            padd(jj);
                        }
                    }
                }
            }

            foreach (Race r in RACES.all())
            {
                race[r.Index()] = new WikiRace(r);
                articles.Add(race[r.Index()]);
            }

            List<Article> sort = articles.OrderBy(a => a.key).ToList();

            articles.Clear();
            foreach (Article a in sort)
                articles.Add(a);

            list = new WikiList(articles, HEIGHT);

            section.Add(list, 0, TOP_HEIGHT);
            int x1 = section.GetLastX2();
            section.Add(sAdded, section.GetLastX2(), TOP_HEIGHT);

            int width = 600;
            int am = (WIDTH - x1) / 600;
            int ex = WIDTH - x1 - am * width;
            ex /= am;
            width += ex;

            added = new List<Article>(am);

            for (int i = 0; i < articles.Count; i++)
            {
                Article a = articles[i];
                a.Init(articles, width);
            }
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            section.Hover(mCoo);
            return true;
        }

        protected override void MouseClick(MButt button)
        {
            if (button == MButt.RIGHT)
            {
                if (!back())
                    Hide();
            }
            else if (button == MButt.LEFT)
            {
                section.Click();
            }
        }

        protected override void HoverTimer(GBox text)
        {
            section.HoverInfoGet(text);
        }

        protected override bool Update(float ds)
        {
            GAME.SPEED.TmpPause();
            return false;
        }

        protected override bool Render(Renderer r, float ds)
        {
            GCOLOR.UI().bg().Render(r, C.DIM());
            section.Render(r, ds);
            return false;
        }

        public override void Hide()
        {
            base.Hide();
        }

        private bool back()
        {
            if (added.Count > 0)
            {
                Remove(added[added.Count - 1]);
                return true;
            }
            else
                return false;
        }

        public void Activate()
        {
            added.Clear();
            Adjust();
            base.Show(VIEW.inters().manager);
        }

        private void Remove(Article a)
        {
            added.Remove(a);
            Adjust();
        }

        public void Set(Article a)
        {
            if (!added.Count.Equals(0) && a == added[0])
                return;

            if (added.Contains(a))
                added.Remove(a);

            if (!added.HasRoom())
                added.RemoveAt(added.Count - 1);

            added.Insert(0, a);
            Adjust();
        }

        private void Adjust()
        {
            int x = sAdded.body().x1();
            int y = sAdded.body().y1();
            sAdded.Clear();

            foreach (Article aa in added)
                sAdded.AddRightC(0, aa.section);

            sAdded.body().MoveX1Y1(x, y);
        }

        public void ShowRace(Race r)
        {
            Activate();
            list.SetList(race[r.Index()]);
            Set(race[r.Index()]);
        }

        public LIST<Article> Added()
        {
            return added;
        }
    }
}