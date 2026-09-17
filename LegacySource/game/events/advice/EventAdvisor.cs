using System;
using System.Collections.Generic;
using System.IO;
using game.events.EVENTS;
using game.events.citizen;
using init.paths;
using settlement.main;
using settlement.stats;
using settlement.stats.standing;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.info;
using util.text;
using view.main;
using view.sett;
using view.ui.message;

namespace game.events.advice
{
    public sealed class EventAdvisor : EventResource
    {
        private bool toggled = true;
        private double timer = 5;
        private LinkedList<Advice> all = new LinkedList<Advice>();
        private LinkedList<Advice> active = new LinkedList<Advice>();

        private static readonly CharSequence ¤¤Advice = "Advisor: ";

        static
        {
            D.ts(typeof(EventAdvisor));
        }

        public EventAdvisor()
            : base("ADVICE")
        {
            Json json = new Json(PATHS.TEXT_MISC().gets("Advice"));

            new AdviceHighlight("ROOMS", "WORKFORCE", json)
            {
                public override bool Shouldsend()
                {
                    int t = STATS.WORK().workforce();
                    int e = SETT.ROOMS().employment.NEEDED.get();
                    if (t - e < -5)
                    {
                        return true;
                    }
                    return false;
                }
            };

            new AdviceHighlight("HEALTH", "SICKNESS", json)
            {
                public override bool Shouldsend()
                {
                    if (STATS.DISEASE().sick().data().get(null) > 0)
                        return true;
                    return false;
                }
            };

            new AdviceHighlight("LAW", "CRIME", json)
            {
                public override bool Shouldsend()
                {
                    if (STATS.LAW().crimeHistory().get(null) > 0)
                        return true;
                    return false;
                }
            };

            new AdviceHighlight("CITIZENS", "LOYALTY", json)
            {
                public override bool Shouldsend()
                {
                    if (STANDINGS.CITIZEN().loyalty.getD(null) < EventCitizen.breakPoint && STATS.POP().POP.data().get(null) > 15)
                        return true;
                    return false;
                }
            };

            foreach (Advice a in all)
                active.add(a);
        }

        protected override void update(double ds)
        {
            if (!toggled)
                return;
            if (!VIEW.s().IsActive())
            {
                timer = 3;
                return;
            }
            timer -= ds;
            if (timer < 0)
            {
                timer += 5;
                foreach (Advice a in active)
                {
                    if (a.send())
                    {
                        active.remove(a);
                        return;
                    }
                }
            }
        }

        protected override void save(FilePutter file)
        {
            file.i(active.size());
            foreach (Advice a in active)
                file.i(a.index);
        }

        protected override void load(FileGetter file) throws IOException
        {
            active.clear();
            int k = file.i();
            for (int i = 0; i < k; i++)
            {
                int q = file.i();
                if (q >= 0 && q < all.size())
                    active.add(all.get(q));
            }
        }

        protected override void clear()
        {
            active.clear();
            foreach (Advice a in all)
                active.add(a);
        }

        public abstract class Advice
        {
            private readonly int index;

            public Advice()
            {
                this.index = all.add(this);
            }

            public abstract bool send();
        }

        public abstract class AdviceHighlight : Advice
        {
            private readonly string keyButt;
            private readonly INFO info;

            public AdviceHighlight(string keyButt, string keyj, Json json)
            {
                this.keyButt = keyButt;
                UISettMap.getByKey(keyButt);
                info = new INFO(json.json(keyj));
            }

            public abstract bool Shouldsend();

            public override bool send()
            {
                if (Shouldsend())
                {
                    new MessageHighlight(info.name, info.desc, keyButt).send();
                    return true;
                }
                return false;
            }
        }

        private class MessageHighlight : MessageSection
        {
            private readonly string UIKey;
            private readonly string body;

            public MessageHighlight(CharSequence title, CharSequence body, string UIKey)
                : base("" + ¤¤Advice + title)
            {
                this.UIKey = UIKey;
                this.body = "" + body;
            }

            protected override void make(GuiSection section)
            {
                paragraph(body);

                section.addDown(0, new RENDEROBJ.RenderImp(0)
                {
                    readonly RENDEROBJ o = UISettMap.getByKey(UIKey);
                    public void render(SPRITE_RENDERER r, float ds)
                    {
                        highlight(section, r, o);
                        if (!VIEW.s().IsActive())
                            VIEW.s().activate();
                    }
                });
            }

            private static void highlight(GuiSection s, SPRITE_RENDERER r, RENDEROBJ o)
            {
                COLOR c = COLOR.RED2RED;

                c.render(r, o.body().x1() - 8, o.body().x2() + 8, o.body().y1() - 8, o.body().y1() - 4);
                c.render(r, o.body().x1() - 8, o.body().x2() + 8, o.body().y2() + 8, o.body().y2() + 4);
                c.render(r, o.body().x1() - 8, o.body().x1() - 4, o.body().y1() - 8, o.body().y2() + 8);
                c.render(r, o.body().x2() + 4, o.body().x2() + 8, o.body().y1() - 8, o.body().y2() + 8);

                if (o.body().cX() < s.body().cX())
                {
                    c.render(r, o.body().x2() + 4, s.body().cX() + 4, o.body().cY() - 4, o.body().cY() + 4);
                }
                else
                {
                    c.render(r, o.body().x1() - 4, s.body().cX() + 4, o.body().cY() - 4, o.body().cY() + 4);
                }

                int y1 = s.body().y1() - 80;
                int y2 = s.body().y2();

                if (o.body().y2() < y1)
                {
                    c.render(r, s.body().cX() - 4, s.body().cX() + 4, o.body().cY(), y1);
                }
                else
                {
                    c.render(r, s.body().cX() - 4, s.body().cX() + 4, o.body().cY(), y2);
                }

                OPACITY.unbind();
            }
        }
    }
}