using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game;
using game.battle.div;
using game.tourism;
using init.constant;
using init.race.appearence;
using init.settings;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.types.prisoner;
using settlement.entity.humanoid.ai.types.tourist;
using settlement.main;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.info;
using util.text;
using world.army;

class UISubjectHoverer
{
    private Induvidual indu;
    private Humanoid hum;

    private static CharSequence ¤¤Sentenced = "Sentenced to be:";
    private static CharSequence ¤¤ClickToChange = "Click to change punishment.";
    private static CharSequence ¤¤JudgedNo = "Pleads innocence. Wants to try case in court.";
    private static CharSequence ¤¤Judged = "Has been found guilty in a court.";
    private static CharSequence ¤¤Soldier = "Serving in your armies abroad.";
    private static CharSequence ¤¤SoldierDiv = "Enlisted in the division: {0}, in the army {1}.";
    private static CharSequence ¤¤SoldierReturn = "Is currently returning to the capitol, will arrive in home in {0} days.";
    private static CharSequence ¤¤yearsOld = "{0} years of age";
    private static CharSequence ¤¤Attraction = "Attraction";
    private static CharSequence ¤¤Service = "Service";
    private static CharSequence ¤¤none = "---";

    static
    {
        D.ts(UISubjectHoverer.class);
    }

    private GuiSection s = new GuiSection();

    public UISubjectHoverer()
    {
        s.addRightC(8, new GStat()
        {
            public override void update(GText text)
            {
                text.lablify();
                text.clear().add(STATS.APPEARANCE().name(indu));
                text.setMaxWidth(300);
                text.setMultipleLines(false);
            }
        }.increase());

        s.add(new GStat()
        {
            public override void update(GText text)
            {
                text.add(indu.race().info.namePosessive);
                text.s().add(hum == null ? HTYPES.SOLDIER().name : hum.title());
                if (hum == null)
                {
                    text.add(',').add(¤¤yearsOld);
                    text.insert(0, (int)STATS.POP().age.years.getD(indu));
                }
                CharSequence extra = null;
                HTYPE t = indu.hType();
                if (t == HTYPES.SLAVE())
                    extra = indu.clas().name;
                else if (t == HTYPES.PRISONER())
                {
                    if (STATS.LAW().prisonerType.get(indu).cl == HCLASSES.SLAVE())
                    {
                        extra = Str.TMP.clear().s().add('(').add(HCLASSES.SLAVE().name).add(')');
                    }
                }
                else if (t.parent() != t)
                {
                    extra = Str.TMP.clear().add(STATS.POP().age.years.getD(indu), 1).s().add(DicTime.¤¤Years);
                }

                if (extra != null)
                    text.s().add('(').add(extra).add(')');
            }
        }, 0, s.body().y2() + 2);

        s.addDown(2, new GStat()
        {
            public override void update(GText text)
            {
                if (hum == null)
                    text.add(¤¤Soldier);
                else
                    hum.ai().getOccupation(hum, text);
            }
        });

        s.addRelBody(8, DIR.W, new SPRITE()
        {
            public override void renderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
            {
            }

            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                STATS.APPEARANCE().portraitRender(r, indu, X1, Y1, 2);
            }

            public override int width()
            {
                return RPortrait.P_WIDTH * 2;
            }

            public override int height()
            {
                return RPortrait.P_HEIGHT * 2;
            }
        });

        s.body().setWidth(550);
    }

    void hover(Humanoid h, GBox text)
    {
        if (h == null)
            return;
        this.hum = h;
        this.indu = h.indu();

        if (h.indu().hostile() && !S.get().developer)
        {
            text.error(HTYPES.ENEMY().name);
            return;
        }

        text.add(s);
        text.NL();

        if (SProblem.problem(h) != null)
        {
            text.add(text.text().errorify().add(SProblem.problem(h)));
            text.NL();
        }
        else if (SProblem.warning(h) != null)
        {
            text.add(text.text().warnify().add(SProblem.warning(h)));
            text.NL();
        }

        if (h.indu().hType() == HTYPES.PRISONER())
        {
            text.text(¤¤Sentenced);
            PUNISHMENT p = AIModule_Prisoner.punishment(h, h.ai());
            text.textLL(p.name);
            if (p == CRIME_PUNISHMENTS.PRISON())
            {
                GText t = text.text();
                t.add('(');
                DicTime.setDays(t, AIModule_Prisoner.DATA().prisonTimeLeft.get(h.ai()));
                t.add(')');
                text.add(t);
            }

            text.NL(4);

            if (AIModule_Prisoner.DATA().judged.get(h.ai()) == 0 && STATS.LAW().prisonerType.get(h.indu()).isJudged)
            {
                if (AIModule_Prisoner.DATA().judged.get(h.ai()) == 0)
                    text.error(¤¤JudgedNo);
                else
                    text.text(¤¤Judged);
                text.NL(4);
            }

            text.textL(¤¤ClickToChange);
        }
        else if (h.indu().hType() == HTYPES.TOURIST())
        {
            text.textLL(SETT.ROOMS().INN.info.name);
            text.NL();
            text.add(text.text().add(AIModule_Tourist.inn(h) == null ? ¤¤none : AIModule_Tourist.inn(h).name()));
            text.NL(8);

            text.textLL(¤¤Attraction);
            text.NL();
            text.text(TOURISM.attraction(h.indu()).info.name);
            text.NL(8);

            text.textLL(¤¤Service);
            text.NL();
            text.text(TOURISM.service(h.indu()) == null ? "?" : TOURISM.service(h.indu()).name);
            text.NL(8);

            text.textLL(Dic.¤¤Curr);
            text.NL();
            text.add(GFORMAT.iBig(text.text(), TOURISM.credits(h.race())));
            text.NL(8);
        }

        h.ai().hoverInfoSet(h, text);

        text.NL(8);
    }

    void hover(Induvidual h, GBox text)
    {
        if (h == null)
            return;
        this.hum = null;
        this.indu = h;

        text.add(s);
        text.NL();

        for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
        {
            int m = AD.cityDivs().soldiers(di);
            for (int i = 0; i < m; i++)
            {
                Induvidual ii = AD.cityDivs().getSoldier(i, di);
                if (ii == h)
                {
                    Div div = GAME.ARMIES().player().divisions().get(di);
                    GText t = text.text();
                    t.add(¤¤SoldierDiv);
                    t.insert(0, div.info.name());
                    t.insert(1, AD.cityDivs().attachedArmy(div).name);
                    text.textL(t);

                    if (AD.cityDivs().daysToReturn(div) > 0)
                    {
                        t = text.text();
                        t.add(¤¤SoldierReturn);
                        t.insert(0, AD.cityDivs().daysToReturn(div), 1);
                        text.textL(t);
                    }
                }
            }
        }
    }
}