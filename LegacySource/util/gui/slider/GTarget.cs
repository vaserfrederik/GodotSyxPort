using System;
using util.data.INT;
using util.gui.misc;
using util.gui.renderable;
using util.info;
using util.text;
using view.keyboard;

namespace util.gui.slider
{
    public class GTarget : GuiSection
    {
        private static readonly string ¤¤Prev5 = "Previous (5)";
        private static readonly string ¤¤Prev = "Previous";
        private static readonly string ¤¤Next5 = "Next (5)";
        private static readonly string ¤¤Next = "Next";

        static GTarget()
        {
            D.ts(typeof(GTarget));
        }

        public GTarget(int width, bool doubleNext, bool horizontal, GStat stat, INTE target)
            : this(width, (RENDEROBJ)null, doubleNext, horizontal, stat.r(DIR.C), target)
        {
        }

        public GTarget(int width, bool doubleNext, bool horizontal, RENDEROBJ stat, INTE target)
            : this(width, (RENDEROBJ)null, doubleNext, horizontal, stat, target)
        {
        }

        public GTarget(int width, bool doubleNext, bool horizontal, INTE target)
            : this(width, (RENDEROBJ)null, doubleNext, horizontal, new GStat
            {
                update = text =>
                {
                    GFORMAT.i(text, target.get());
                }
            }.r(DIR.C), target)
        {
        }

        public GTarget(int width, SPRITE label, bool doubleNext, bool horizontal, INTE target)
            : this(width, new RENDEROBJ.Sprite(label), doubleNext, horizontal, new GStat
            {
                update = text =>
                {
                    GFORMAT.i(text, target.get());
                }
            }.r(DIR.C), target)
        {
        }

        public GTarget(int width, SPRITE label, bool doubleNext, bool horizontal, GStat stat, INTE target)
            : this(width, new RENDEROBJ.Sprite(label), doubleNext, horizontal, stat.r(DIR.C), target)
        {
        }

        public GTarget(int width, RENDEROBJ label, bool doubleNext, bool horizontal, GStat stat, INTE target)
            : this(width, label, doubleNext, horizontal, stat.r(DIR.C), target)
        {
        }

        public GTarget(int width, RENDEROBJ label, bool doubleNext, bool horizontal, RENDEROBJ stat, INTE target)
        {
            if (doubleNext)
            {
                CLICKABLE c = new GButt.Glow(SPRITES.icons().s.minifierBig)
                {
                    clickA = () =>
                    {
                        target.inc(-5);
                        if (KEYS.MAIN().MOD.isPressed())
                            target.set(target.min());
                    },
                    renAction = () =>
                    {
                        activeSet(activeIs() && target.get() > target.min());
                    },
                    hoverInfoGet = text =>
                    {
                        base.hoverInfoGet(text);
                        text.NL();
                        GAllocator.hov(text);
                    }
                }.repetativeSet(true).hoverInfoSet(¤¤Prev5);
                addRightC(0, c);
            }

            CLICKABLE c = new GButt.Glow(SPRITES.icons().s.minifier)
            {
                clickA = () =>
                {
                    target.inc(-1);
                    if (KEYS.MAIN().MOD.isPressed())
                        target.set(target.min());
                },
                renAction = () =>
                {
                    activeSet(activeIs() && target.get() > target.min());
                },
                hoverInfoGet = text =>
                {
                    base.hoverInfoGet(text);
                    text.NL();
                    GAllocator.hov(text);
                }
            }.repetativeSet(true).hoverInfoSet(¤¤Prev);
            addRightC(0, c);

            addRightC(width / 2, stat);
            body().incrW(width / 2);
            c = new GButt.Glow(SPRITES.icons().s.magnifier)
            {
                clickA = () =>
                {
                    target.inc(1);
                    if (KEYS.MAIN().MOD.isPressed())
                        target.set(target.max());
                },
                renAction = () =>
                {
                    activeSet(activeIs() && target.get() < target.max());
                },
                hoverInfoGet = text =>
                {
                    base.hoverInfoGet(text);
                    text.NL();
                    GAllocator.hov(text);
                }
            }.repetativeSet(true).hoverInfoSet(¤¤Next);
            c.body().moveX1(body().x2()).moveCY(body().cY());
            add(c);

            if (doubleNext)
            {
                c = new GButt.Glow(SPRITES.icons().s.magnifierBig)
                {
                    clickA = () =>
                    {
                        target.inc(5);
                        if (KEYS.MAIN().MOD.isPressed())
                            target.set(target.max());
                    },
                    renAction = () =>
                    {
                        activeSet(activeIs() && target.get() < target.max());
                    },
                    hoverInfoGet = text =>
                    {
                        base.hoverInfoGet(text);
                        text.NL();
                        GAllocator.hov(text);
                    }
                }.repetativeSet(true).hoverInfoSet(¤¤Next5);
                addRightC(0, c);
            }

            if (label != null)
            {
                if (horizontal)
                {
                    label.body().moveX1(-label.body().width() - C.SG * 8);
                    label.body().moveCY(body().cY());
                }
                else
                {
                    label.body().moveCX(body().cX());
                    label.body().moveY2(body().y1() - C.SG * 2);
                }

                add(label);
            }
        }
    }
}