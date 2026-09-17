using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Snake2D;
using Util.Colors;
using Util.Gui.Misc;
using Util.Rendering;
using Util.Text;
using View.Main;

namespace Init.Sprite
{
    public class RLoadPrinter
    {
        private readonly GuiSection section = new GuiSection();
        private readonly RenderObj.Sprite bg = new RenderObj.Sprite();
        private readonly GTextR info = new GTextR(UI.FONT().S, 200);

        private readonly Quote quote = new Quote();

        private readonly Fire torch1 = new Fire(5);
        private readonly Fire torch2 = new Fire(4);

        private readonly AmbientLight light = new AmbientLight();
        private long lastInit = 0;

        private bool minified = false;

        private readonly GText miniThing = new GText(UI.FONT().H1, 24);
        private readonly Str regularThing = new Str(24);
        private string miniText;
        private int miniI;
        private int regularI;
        private readonly int miniD;
        private readonly RenderObj loadingBig = UI.decor().getDecored(Dic.¤¤loading);
        public RLoadPrinter()
        {
            section.Add(UI.decor().frame(quote.body()));

            {
                RenderObj r = UI.decor().getDecored("SONGS OF SYX");
                r.body().centerX(section);
                r.body().moveY2(section.body().y1());
                section.Add(r);
            }

            section.Add(quote);
            section.body().centerIn(C.DIM());

            bg.setSprite(SPRITES.loadScreen());
            bg.body().centerIn(C.DIM());

            torch1.setRadius(1000);
            torch1.set(bg.body().x1() - 100, C.HEIGHT() / 2);
            torch1.setFalloff(3f);
            torch1.setFlickerFactor(25f);
            torch1.setZ(25);
            torch2.setFalloff(3f);
            torch2.setRadius(500);
            torch2.set(bg.body().x2() + 100, C.HEIGHT() / 2);
            torch2.setFlickerFactor(10f);
            torch2.setZ(25);

            torch1.flicker(1f);
            torch2.flicker(1f);

            light.setDir(220);
            light.setTilt(25);
            light.g(1.3f);
            light.b(1.3f);
            light.r(1.3f);

            init();

            miniThing.lablify();
            miniThing.add('.').add('.').add('.');
            miniD = miniThing.adjustWidth().width();
        }

        public void Minify(bool minify, string title)
        {
            minified = minify;
            miniText = title;
            miniI = 0;
        }

        public void Print(string str)
        {
            if (minified)
            {
                CORE.renderer().Clear();
                TIME.light().applyGuiLight(0, C.DIM());

                miniThing.set(miniText);
                int wi = miniThing.width();
                for (int i = 0; i < (miniI / 4) % 4; i++)
                {
                    miniThing.add('.');
                }
                miniI++;
                int x1 = C.DIM().cX() - wi / 2;

                UI.PANEL().titleBoxes[UI.PANEL().titleBoxes.Length - 1].renderCY(CORE.renderer(), x1, C.DIM().cY(), wi + miniD);
                miniThing.renderCY(CORE.renderer(), x1, C.DIM().cY());

                CORE.renderer().newLayer(false, 0);
                VIEW.render();
                CORE.swapAndPoll();
                return;
            }

            regularThing.clear().add(str);
            for (int i = 0; i < (regularI / 4) % 4; i++)
            {
                regularThing.add('.');
            }
            regularI++;

            render(regularThing, false);

            CORE.swapAndPoll();
        }

        public void Printempty()
        {
            CORE.renderer().Clear();
            CORE.renderer().shadeLight(true);
            CORE.renderer().shadowDepthSet((byte)0);
            shadow.init(1, 0.5f, 0.5f);
            shadow.setDistance2Ground(2).setHeight(24);
            light.register(C.DIM());

            loadingBig.body().centerIn(C.DIM());
            loadingBig.render(shadow, lastInit);
            loadingBig.render(CORE.renderer(), lastInit);

            CORE.renderer().newLayer(false, 0);
            byte none = 0;
            byte full = -1;
            CORE.renderer().registerLight(torch1, bg.body().x1(), bg.body().x1() + 100, bg.body().y1(), bg.body().y2(), full, full, none, none);
            CORE.renderer().registerLight(torch1, bg.body().x1() + 100, bg.body().x2(), bg.body().y1(), bg.body().y2(), full, full, full, full);

            CORE.renderer().registerLight(torch2, bg.body().x2() - 100, bg.body().x2(), bg.body().y1(), bg.body().y2(), none, none, full, full);
            CORE.renderer().registerLight(torch2, bg.body().x1(), bg.body().x2() - 100, bg.body().y1(), bg.body().y2(), full, full, full, full);

            bg.render(CORE.renderer(), 0);

            OPACITY.O50.bind();
            COLOR.BLACK.render(CORE.renderer(), quote.body(), 16);
            OPACITY.unbind();
            CORE.swapAndPoll();
        }

        private readonly ShadowBatch.Real shadow = new ShadowBatch.Real();
        {
            shadow.setDistance2GroundUI(12);
        }

        public void render(string str, bool flash)
        {
            if (str == null)
                return;

            CORE.renderer().Clear();
            CORE.renderer().shadeLight(true);
            CORE.renderer().shadowDepthSet((byte)0);
            shadow.init(1, 0.5f, 0.5f);
            shadow.setDistance2Ground(2).setHeight(24);
            light.register(C.DIM());

            info.text().clear().set(str);
            info.body().centerX(C.DIM());
            info.body().moveY1(section.body().y2() + 50);

            if (flash)
                info.text().color(COLOR.WHITE2WHITE);
            else
                info.text().normalify();
            info.render(CORE.renderer(), 0);
            info.render(shadow, 0);

            shadow.setDistance2Ground(2).setHeight(24);
            section.render(CORE.renderer(), 0);
            section.render(shadow, 0);

            CORE.renderer().newLayer(false, 0);
            byte none = 0;
            byte full = -1;
            CORE.renderer().registerLight(torch1, bg.body().x1(), bg.body().x1() + 100, bg.body().y1(), bg.body().y2(), full, full, none, none);
            CORE.renderer().registerLight(torch1, bg.body().x1() + 100, bg.body().x2(), bg.body().y1(), bg.body().y2(), full, full, full, full);

            CORE.renderer().registerLight(torch2, bg.body().x2() - 100, bg.body().x2(), bg.body().y1(), bg.body().y2(), none, none, full, full);
            CORE.renderer().registerLight(torch2, bg.body().x1(), bg.body().x2() - 100, bg.body().y1(), bg.body().y2(), full, full, full, full);

            bg.render(CORE.renderer(), 0);

            OPACITY.O50.bind();
            COLOR.BLACK.render(CORE.renderer(), quote.body(), 16);
            OPACITY.unbind();
            CORE.swapAndPoll();
        }

        public bool IsMini()
        {
            return minified;
        }

        private class Quote : RenderObj.RenderImp
        {
            private readonly Text quote = new Text(UI.FONT().M, 400);
            private readonly Text author = new Text(UI.FONT().H2, 400);

            private readonly string[] quotes;
            private readonly string[] authors;

            public Quote()
            {
                body.setWidth(2 * C.MIN_WIDTH / 3);
                body.setHeight(180);
                quote.setMaxWidth(body.width());
                author.setMaxWidth(body.width());
                Json json = new Json(PATHS.TEXT_MISC().gets("Quotes"));
                string[] qs = json.texts("QUOTES");
                if (qs.Length == 0)
                    json.error("Insufficient quotes. Need at least one", "QUOTES");
                int l = qs.Length;
                quotes = new string[l];
                authors = new string[l];
                for (int i = 0; i < l; i++)
                {
                    string[] q = qs[i].Split(":::");
                    if (q.Length != 2)
                    {
                        q = qs[i].Split("::");
                    }
                    if (q.Length != 2)
                    {
                        q = qs[i].Split("::");
                        LOG.ln("unable to parse " + qs[i]);
                        //json.error("Unable to parse quote. Seperate quote and author with ':::'", "QUOTES");
                        quotes[i] = "";
                        authors[i] = "";
                    }
                    else
                    {
                        quotes[i] = q[0];
                        authors[i] = q[1];
                    }
                }
            }

            public void Set()
            {
                int i = RND.rInt(quotes.Length);
                quote.set(quotes[i]);
                author.set(authors[i]);
            }

            public override void render(SpriteRenderer r, float ds)
            {
                int x1 = body.x1() + (body.width() - quote.width()) / 2;
                int y1 = body.y1() + (body.height() - (quote.height() + author.height() + 10)) / 2;

                quote.render(r, x1, y1);
                y1 += quote.height();

                int dx = (body.width() - author.width());
                if (dx > 30)
                    dx -= 30;

                x1 = body.x1() + dx;

                GColor.T().H2.bind();
                author.render(r, x1, y1 + 10);
                COLOR.unbind();
            }
        }
    }
}