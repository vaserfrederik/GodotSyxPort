using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using snake2d;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.interrupter;
using view.ui.message;
using world.map.regions;

namespace game.faction.player
{
    public sealed class PLevels
    {
        private static readonly CharSequence ¤¤mTitle = "¤New level unlocked!";
        private static readonly CharSequence ¤¤mMessage = "¤A new level has been bestowed upon your name!";

        static PLevels()
        {
            D.t(typeof(PLevels));
        }

        public readonly INFO info = new INFO(
            D.g("Level"),
            D.g("Desc", "As you grow in might and population, titles will be bestowed upon your name. Levels will unlock great advantages to a ruler.")
        );

        private readonly List<Level> levels;
        private Level current;
        private bool increase;
        public readonly BoostSpecs boosters;
        private readonly BoostCompound<Level> bos;
        private double time;

        public PLevels()
        {
            PATH data = PATHS.INIT().getFolder("player").getFolder("level");
            PATH text = PATHS.TEXT().getFolder("player").getFolder("level");
            string[] ss = data.getFiles();

            if (false)
            {
                // utopia playstyle has too much loyalty modulators
            }

            levels = new List<Level>(ss.Length);

            foreach (string s in ss)
            {
                new Level(levels, s, data, text);
            }

            if (levels.Count == 0)
                new Errors.DataError("Insufficient levels declared. Needs more than 0", data.get());
            current = levels[0];

            IDebugPanel.add("Increase level", new ACTION
            {
                exe = () =>
                {
                    increase = true;
                    time = TIME.secondsPerDay();
                }
            });

            boosters = new BoostSpecs(Dic.¤¤Level, UI.icons().s.star, true);
            bos = new BoostCompound<Level>(boosters, levels)
            {
                protected override double getValue(Level t) => current.index >= t.index ? 1 : 0,
                protected override BoostSpecs bos(Level t) => t.boosters
            };
        }

        public readonly SAVABLE saver = new SAVABLE
        {
            save = file =>
            {
                file.i(current.index);
                file.d(time);
            },
            load = file =>
            {
                int i = file.i();
                if (i >= levels.Count)
                    i = levels.Count - 1;
                current = levels[i];
                bos.clearChache();
                time = file.d();
            },
            clear = () =>
            {
                current = levels[0];
                bos.clearChache();
                time = 0;
            }
        };

        void update(double ds)
        {
            if (current().index() < levels.Count - 1 && (increase || levels[current().index() + 1].lockable.passes(FACTIONS.player())))
            {
                time += ds;
                if (time > TIME.secondsPerDay())
                {
                    current = levels[current().index() + 1];
                    bos.clearChache();
                    new Mess(current.index).send();
                    increase = false;
                }
            }
            else
            {
                time = TIME.currentSecond();
            }
        }

        public LIST<Level> all() => new LIST<Level>(levels);

        public Level current() => current;

        public void set(int level) => this.current = levels[level];

        public class Level : INDEXED
        {
            private readonly int index;
            public readonly CharSequence male;
            public readonly CharSequence female;
            public readonly CharSequence desc;

            public readonly BoostSpecs boosters;
            public readonly Lockable<Faction> lockable;
            public readonly Lockers lockers;

            public Level(List<Level> all, string key, PATH data, PATH text)
            {
                this.index = all.Add(this);
                Json d = new Json(data.gets(key));
                Json t = new Json(text.gets(key));
                male = t.text("MALE");
                female = t.text("FEMALE");
                desc = t.text("DESC");

                lockable = GVALUES.FACTION.LOCK.push();
                lockable.push(d);
                lockers = new Lockers(Dic.¤¤Level + ": " + male, UI.icons().s.star);

                lockers.add(GVALUES.FACTION, d, new DOUBLE_O<Faction>
                {
                    getD = t =>
                    {
                        if (t == FACTIONS.player())
                        {
                            if (FACTIONS.player().level().current().index() >= index())
                                return 1.0;
                            return 0;
                        }
                        return 1;
                    }
                });

                lockers.add(GVALUES.INDU, d, new DOUBLE_O<Induvidual>
                {
                    getD = t =>
                    {
                        if (t.faction() == FACTIONS.player())
                        {
                            if (FACTIONS.player().level().current().index() >= index())
                                return 1.0;
                            return 0;
                        }
                        return 1;
                    }
                });

                lockers.add(GVALUES.REGION, d, new DOUBLE_O<Region>
                {
                    getD = t =>
                    {
                        if (t.faction() == FACTIONS.player())
                        {
                            if (FACTIONS.player().level().current().index() >= index())
                                return 1.0;
                            return 0;
                        }
                        return 1;
                    }
                });

                boosters = new BoostSpecs(Dic.¤¤Level + ": " + male, UI.icons().s.star, false);
                boosters.read(d, null);
            }

            public CharSequence name() => male;

            public override int index() => index;

            public void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                b.title(name());
                GText t = b.text();
                t.add(desc);
                b.add(t);
                b.NL(4);

                lockable.hover(text, FACTIONS.player());
                b.sep();
                lockers.hover(text);
                b.NL(8);
                boosters.hover(text, 1.0, -1);
            }
        }

        private class Mess : MessageSection
        {
            private readonly int lev;

            public Mess(int lev) : base(¤¤mTitle)
            {
                this.lev = lev;
            }

            protected override void make(GuiSection section)
            {
                Level l = FACTIONS.player().level().all().get(lev);
                paragraph(¤¤mMessage);
                section.addRelBody(16, DIR.S, new GHeader(l.name()));

                section.addRelBody(8, DIR.S, new RENDEROBJ.RenderImp(700, GBox.Dummy().maxHeight)
                {
                    render = (r, ds) =>
                    {
                        GBox.tmp.clear();
                        GBox.tmp.maxWidth = 700;
                        GBox.tmp.maxHeight = 500;
                        l.hoverInfoGet(GBox.tmp);
                        GBox.tmp.renderWithout(r, body.x1(), body.y1());
                    }
                });
            }
        }
    }
}