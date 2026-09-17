using System;
using System.Collections.Generic;
using System.Linq;

using init.race;
using init.sprite.UI;
using init.type;
using settlement.main;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.Hoverable;
using util.data;
using util.gui.misc;
using util.info;
using util.text;

public abstract class CatPopulationGrowth : GuiSection
{
    private static readonly CharSequence ¤¤immi = "¤Aspiring Immigrants";
    private static readonly CharSequence ¤¤Athorize = "¤Authorize";
    private static readonly CharSequence ¤¤Auto = "¤Auto";
    private static readonly CharSequence ¤¤AutoDesc = "¤Automatically authorize new immigrants up until this amount.";
    private static readonly CharSequence ¤¤Children = "Children";
    private static readonly CharSequence ¤¤Limit = "¤Limit";
    private static readonly CharSequence ¤¤LimitD = "¤Sets the max population desired from breeding. When breeding stops based on this limit, your population might be unhappy about it.";

    private static readonly CharSequence ¤¤ForcedBreeding = "¤Forces your subjects to breed harder and faster.";

    static
    {
        D.ts(typeof(CatPopulationGrowth));
    }

    private readonly HCLASS cl;
    private readonly GETTER<Race> race;

    public CatPopulationGrowth(HCLASS cl, GETTER<Race> race)
    {
        this.cl = cl;
        this.race = race;

        GuiSection b = breed();
        int w = b.body().width();
        if (cl == HCLASSES.CITIZEN())
        {
            GuiSection a = immi();
            w = Math.Max(w, a.body().width());
            a.body().setWidth(w);
            add(a);
        }
        b.body().setWidth(w);
        addDown(2, b);
    }

    public override void render(SPRITE_RENDERER r, float ds)
    {
        visableSet(race.get() != null);
        if (race.get() != null)
            base.render(r, ds);
    }

    private GuiSection immi()
    {
        GuiSection s = new GuiSection
        {
            render = (r, ds) =>
            {
                GButt.ButtPanel.renderBG(r, cl == HCLASSES.CITIZEN(), false, hoveredIs(), body());
                base.render(r, ds);
                GButt.ButtPanel.renderFrame(r, body());
            },

            hoverInfoGet = text =>
            {
                base.hoverInfoGet(text);
                if (text.emptyIs())
                {
                    SETT.ENTRY().immi().hoverImmigrants(text, HCLASS_RACE.clP(race.get(), cl));
                }
            }
        };

        s.add(new GStat
        {
            update = text =>
            {
                if (cl == HCLASSES.CITIZEN())
                    GFORMAT.i(text, SETT.ENTRY().immi().wanted(race.get()));
                if (race.get() != null)
                    text.s().add('/').add(SETT.ENTRY().immi().auto(race.get()).get());
                else
                    text.add(0);
            }
        }.hh(¤¤immi));

        INTE m = new INTE
        {
            d = new double[RACES.all().size()],

            min = () => 0,

            max = () =>
            {
                if (cl == HCLASSES.CITIZEN())
                    return SETT.ENTRY().immi().wanted(race.get());
                return 0;
            },

            get = () =>
            {
                if (race.get() != null)
                    return (int)(max() * d[race.get().index]);
                return 0;
            },

            set = t =>
            {
                d[race.get().index] = (double)t / max();
            }
        };

        s.addDown(4, new GInputInt(m, true, true));

        s.addRightC(8, new GButt.ButtPanel(¤¤Athorize)
        {
            clickA = () =>
            {
                if (m.get() > 0)
                    SETT.ENTRY().immi().admit(race.get(), m.get());
            },

            renAction = () =>
            {
                activeSet(m.get() > 0);
                base.renAction();
            }
        });

        s.add(new HOVERABLE.Sprite(new GText(UI.FONT().S, ¤¤Auto)).hoverInfoSet(¤¤AutoDesc), 0, s.body().y2() + 10);

        INTE a = new INTE
        {
            min = () => 0,

            max = () =>
            {
                if (cl == HCLASSES.CITIZEN() && race.get() != null)
                    return SETT.ENTRY().immi().auto(race.get()).max();
                return 0;
            },

            get = () =>
            {
                if (cl == HCLASSES.CITIZEN() && race.get() != null)
                    return SETT.ENTRY().immi().auto(race.get()).get();
                return 0;
            },

            set = t =>
            {
                if (cl == HCLASSES.CITIZEN())
                    SETT.ENTRY().immi().auto(race.get()).set(t);
            }
        };

        s.addRightCAbs(80, new GInputInt(a, true, true));

        s.addRelBody(16, DIR.W, UI.icons().s.arrow_right.scaled(2.0));

        s.pad(16, 8);

        return s;
    }

    private GuiSection breed()
    {
        GuiSection s = new GuiSection
        {
            render = (r, ds) =>
            {
                GButt.ButtPanel.renderBG(r, STATS.POP().reproduction.propagates(cl, race.get()), false, hoveredIs(), body());
                base.render(r, ds);
                GButt.ButtPanel.renderFrame(r, body());
            },

            hoverInfoGet = text =>
            {
                base.hoverInfoGet(text);
                GBox b = (GBox)text;
                if (text.emptyIs())
                {
                    STATS.POP().reproduction.hover(b, cl, race.get());
                }
            }
        };

        s.add(new GStat
        {
            update = text =>
            {
                GFORMAT.i(text, STATS.POP().reproduction.kidsIncoming(cl, race.get()));

                text.s().add('+').add(STATS.POP().reproduction.kidsPerYear(cl, race.get()), 2);
            }
        }.hh(¤¤Children));

        s.add(new HOVERABLE.Sprite(new GText(UI.FONT().S, ¤¤Limit).lablify()).hoverInfoSet(¤¤LimitD), 0, s.body().y2() + 10);

        INTE a = new INTE
        {
            min = () => 0,

            max = () =>
            {
                return STATS.POP().reproduction.propagates(cl, race.get()) ? STATS.POP().reproduction.limit.max(HCLASS_RACE.clP(race.get(), cl)) : 0;
            },

            get = () =>
            {
                return STATS.POP().reproduction.propagates(cl, race.get()) ? STATS.POP().reproduction.limit.get(HCLASS_RACE.clP(race.get(), cl)) : 0;
            },

            set = t =>
            {
                STATS.POP().reproduction.limit.set(HCLASS_RACE.clP(race.get(), cl), t);
            }
        };

        s.addRightCAbs(80, new GInputInt(a, true, true));

        a = new INTE
        {
            min = () => 0,

            max = () => 100,

            get = () => STATS.POP().reproduction.settings.get(HCLASS_RACE.clP(race.get(), cl).index),

            set = t => STATS.POP().reproduction.settings.set(HCLASS_RACE.clP(race.get(), cl).index, t)
        };

        s.addRelBody(2, DIR.S, new GSliderInt(a, 150, true)
        {
            hoverInfoGet = text =>
            {
                GBox b = (GBox)text;

                b.text(¤¤ForcedBreeding);
                b.NL();
                b.add(GFORMAT.percInc(b.text(), in.getD() - 0.5));
            }
        });

        s.addRelBody(16, DIR.W, UI.icons().s.reproduction.scaled(2.0));

        s.pad(16, 6);

        return s;
    }

    public abstract HCLASS_RACE pop();
}