using System;
using System.Collections.Generic;
using System.Text;
using Init.Race;
using Init.Sprite;
using Init.Type;
using Settlement.Stats;
using Settlement.Stats.Stat;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.Gui;
using Snake2D.Util.Sprite;
using Util.Colors;
using Util.Gui.Misc;
using Util.Gui.Table;
using Util.Info;
using Util.Text;

namespace Settlement.Stats.Util
{
    public class StatHoverer
    {
        private static readonly CharSequence ¤¤Liked = "¤This is liked by your {0}. Higher value = more fulfillment.";
        private static readonly CharSequence ¤¤Dislike = "¤This is disliked by your {0}. Higher value = less fulfillment.";
        private static readonly CharSequence ¤¤DontCare = "¤Your {0} don't care about this and its value has no effect on fulfillment";

        private static readonly CharSequence ¤¤ValueCurrent = "¤Current value: ";
        private static readonly CharSequence ¤¤FulfillmentValue = "¤Current Fulfillment: ";

        private static readonly CharSequence ¤¤HistoryValue = "¤History Value (days)";
        private static readonly CharSequence ¤¤HistoryFulfillment = "¤History Fulfillment (days)";
        private static readonly CharSequence ¤¤toReachMAx = "¤To reach max fulfillment, value needs to be : {0}%";

        static StatHoverer()
        {
            D.ts(typeof(StatHoverer));
        }

        private static UtilGraph h1 = new UtilGraph();
        private static UtilGraph h2 = new UtilGraph();

        public static void hover(GUI_BOX text, STAT s)
        {
            GBox b = (GBox)text;
            b.title(s.info().name);
            b.text(s.info().desc);
            b.NL();
        }

        public static void hover(GUI_BOX text, STAT s, HCLASS cl, Race type)
        {
            GBox b = (GBox)text;
            double max = s.standing().max(cl, type);

            {
                double m = s.standing().get(cl, type, 0);
                double mm = s.standing().get(cl, type, 1);
                if (m == mm)
                {
                    GText t = b.text();
                    t.add(¤¤DontCare);
                    t.insert(0, cl.names);
                    b.add(t);
                }
                else if (m > mm)
                {
                    b.add(SPRITES.icons().m.arrow_down);
                    GText t = b.text();
                    t.add(¤¤Dislike);
                    t.insert(0, cl.names);
                    t.errorify();
                    b.add(t);
                }
                else
                {
                    b.add(SPRITES.icons().m.arrow_up);
                    GText t = b.text();
                    t.add(¤¤Liked);
                    t.insert(0, cl.names);
                    t.normalify2();
                    b.add(t);
                }
            }

            b.NL(8);

            b.textL(¤¤ValueCurrent);
            b.tab(7);

            b.add(format(b.text(), s, s.data(cl).getD(type), cl, type));
            if (s.info().isInt())
            {
                b.add(b.text().add('(').add((int)(s.data(cl).getD(type) * 100)).add('%').add(')'));
            }

            {
                double d = s.data(cl).getD(type) - s.data().getD(type, 1);
                b.tab(11);
                b.add(GFORMAT.percInc(b.text(), d));
            }

            b.NL();

            if (max > 0)
            {
                b.textL(¤¤FulfillmentValue);
                b.tab(7);
                b.add(GFORMAT.fofkInv(b.text(), s.standing().get(cl, type), max));
                double d = s.standing().get(cl, type) - s.standing().getHistoric(cl, type, 1);
                b.tab(11);
                b.add(GFORMAT.f0(b.text(), d));
                b.NL();

                if (type != null)
                {
                    GText t = b.text();
                    t.add(¤¤toReachMAx);
                    if (!s.standing().definition(type).inverted)
                    {
                        double e = s.standing().definition(type).exp == null ? 1 : s.standing().definition(type).exp.pow;
                        e = Math.Pow(1, -e) / s.standing().definition(type).mul;
                        t.insert(0, (int)(100 * e));
                    }
                    else
                        t.insert(0, 0);
                    b.add(t);
                    b.NL();
                }
            }

            b.NL(16);
            b.textLL(¤¤HistoryValue);
            if (max > 0)
            {
                b.tab(8);
                b.textLL(¤¤HistoryFulfillment);
            }

            b.NL(4);
            b.add(h1.init(cl, s, s.info().isInt(), type, true));
            if (max > 0)
            {
                b.tab(8);
                b.add(h2.init(cl, s, s.info().isInt(), type, false));
            }

            if (s.boosters.all().size() > 0)
            {
                b.sep();
                s.boosters.hover(text, HCLASS_RACE.clP(type, cl));
                b.NL();
            }
        }

        public static void hover(GUI_BOX text, STAT s, Induvidual indu)
        {
            HCLASS cl = indu.clas();
            Race type = indu.race();
            GBox b = (GBox)text;

            b.NL(8);

            double max = s.standing().max(cl, type);

            {
                double m = s.standing().get(cl, type, 0);
                double mm = s.standing().get(cl, type, 1);

                if (m == mm)
                {
                    GText t = b.text();
                    t.add(¤¤DontCare);
                    t.insert(0, cl.names);
                    b.add(t);
                }
                else if (m > mm)
                {
                    b.add(SPRITES.icons().m.arrow_down);
                    GText t = b.text();
                    t.add(¤¤Dislike);
                    t.insert(0, cl.names);
                    t.errorify();
                    b.add(t);
                }
                else
                {
                    b.add(SPRITES.icons().m.arrow_up);
                    GText t = b.text();
                    t.add(¤¤Liked);
                    t.insert(0, cl.names);
                    t.normalify2();
                    b.add(t);
                }
            }

            b.NL();

            b.textL(¤¤ValueCurrent);
            b.tab(7);

            b.add(format(b.text(), s, s.data(cl).getD(type), cl, type));
            if (s.info().isInt())
            {
                b.add(b.text().add('(').add((int)(s.data(cl).getD(type) * 100)).add('%').add(')'));
            }

            {
                double d = s.data(cl).getD(type) - s.data().getD(type, 1);
                b.tab(11);
                b.add(GFORMAT.percInc(b.text(), d));
            }

            b.NL();

            if (max > 0)
            {
                b.textL(¤¤FulfillmentValue);
                b.tab(7);
                b.add(GFORMAT.fofkInv(b.text(), s.standing().get(cl, type), max));
                double d = s.standing().get(cl, type) - s.standing().getHistoric(cl, type, 1);
                b.tab(11);
                b.add(GFORMAT.f0(b.text(), d));
                b.NL();

                if (type != null)
                {
                    GText t = b.text();
                    t.add(¤¤toReachMAx);
                    if (!s.standing().definition(type).inverted)
                    {
                        double e = s.standing().definition(type).exp == null ? 1 : s.standing().definition(type).exp.pow;
                        e = Math.Pow(1, -e) / s.standing().definition(type).mul;
                        t.insert(0, (int)(100 * e));
                    }
                    else
                        t.insert(0, 0);
                    b.add(t);
                    b.NL();
                }
            }

            b.NL(16);
            b.textLL(¤¤HistoryValue);
            if (max > 0)
            {
                b.tab(8);
                b.textLL(¤¤HistoryFulfillment);
            }

            b.NL(4);
            b.add(h1.init(cl, s, s.info().isInt(), type, true));
            if (max > 0)
            {
                b.tab(8);
                b.add(h2.init(cl, s, s.info().isInt(), type, false));
            }

            if (s.boosters.all().size() > 0)
            {
                b.sep();
                s.boosters.hover(text, HCLASS_RACE.clP(type, cl));
                b.NL();
            }
        }

        public static SPRITE chart(HCLASS cl, SETT_STATISTICS s, Race race, bool isInt, bool isValue)
        {
            h1.init(cl, s, isInt, race, isValue);
            return h1;
        }

        private static GText format(GText text, STAT s, double value, HCLASS cl, Race race)
        {
            double max = s.standing().max(cl, race);

            if (!max.Equals(0))
            {
                double v = value / max;
                if (v > 1) v = 1;
                if (v < 0) v = 0;
                text.add(v * 100).add('%');
            }
            else
            {
                text.add(value);
            }

            return text;
        }

        private static class UtilGraph : SPRITE
        {
            private readonly GStaples staples = new GStaples(STATS.DAYS_SAVED)
            {
                hover = (box, stapleI) => { },
                getValue = stapleI =>
                {
                    int fromZero = STATS.DAYS_SAVED - stapleI - 1;
                    if (!valuev && global is STAT)
                    {
                        STAT s = (STAT)global;
                        double m = s.standing().max(c, race);
                        if (m <= 0) return 0;
                        if (s.standing().max(c, race, fromZero) > 0)
                            return s.standing().get(c, race, global.data(c).getD(race, fromZero)) / m;
                    }

                    return global.data(c).getD(race, fromZero);
                },
                setColor = (col, stapleI, value) =>
                {
                    int fromZero = STATS.DAYS_SAVED - stapleI - 1;

                    if (!valuev && global is STAT)
                    {
                        STAT s = (STAT)global;
                        if (s.standing().max(c, race, fromZero) > 0)
                        {
                            col.set(GCOLOR.UI().NEUTRAL.normal);
                            return;
                        }
                    }
                    col.set(COLOR.WHITE65);
                },
                setColorBg = (col, stapleI, value) =>
                {
                    col.set(COLOR.WHITE05);
                }
            };
            private SETT_STATISTICS global;
            private HCLASS c;
            private Race race;
            private bool valuev;

            public UtilGraph()
            {
                staples.body().setDim(250, 64);
                staples.normalize(false);
            }

            public SPRITE init(HCLASS c, SETT_STATISTICS global, bool isInt, Race race, bool isValue)
            {
                valuev = isValue;
                this.c = c;
                this.global = global;
                this.race = race;
                return this;
            }

            public override int width()
            {
                return staples.body().width();
            }

            public override int height()
            {
                return staples.body().height();
            }

            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                staples.body().moveX1Y1(X1, Y1);
                staples.render(r, 0);
            }

            public override void renderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
            {
                // TODO Auto-generated method stub
            }
        }
    }
}