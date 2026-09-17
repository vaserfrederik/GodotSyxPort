using System;
using System.Collections.Generic;
using System.Linq;

using Game;
using Game.Battle.Div;
using Init.Constant;
using Init.Sprite.UI;
using Init.Type;
using Settlement.Entity;
using Settlement.Main;
using Settlement.Room.Military.Training;
using Settlement.Stats;
using Settlement.Stats.Colls.StatsBattle;
using Settlement.Stats.Equip;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.Color;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Sprite;
using Snake2D.Util.Sprite.Text;
using Util.Colors;
using Util.Gui.Misc;
using Util.Info;
using Util.Text;
using World.Army;

namespace View.Ui.Div
{
    public class UIDivCardSett : IDimension
    {
        private static readonly string ¤¤needs = "¤Needs to Train";
        private static readonly string ¤¤fully = "¤Fully Trained";
        private static readonly string ¤¤currently = "¤Currently Training";

        private static readonly string ¤¤army = "¤Division is currently attached to the world army '{0}'. It must be recalled in order to be edited;";
        private static readonly string ¤¤armyTime = "¤Division is returning home to our capital. The soldiers will arrive in {0} days.";
        static UIDivCardSett()
        {
            D.ts(typeof(UIDivCardSett));
        }

        private readonly Rect body = new Rect();
        private readonly int WIDTH;
        private readonly int HEIGHT;
        private readonly UIDiv m;
        private readonly TrainingSpec spec = new TrainingSpec();

        private GuiSection sec = new GuiSection();
        private readonly UIDivStats stat = new UIDivStats();
        private Div current;

        public UIDivCardSett(UIDiv m)
        {
            this.m = m;
            WIDTH = m.WIDTH;
            HEIGHT = m.HEIGHT + 20;

            {
                GuiSection s = new GuiSection();

                foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
                {
                    SPRITE hh = new SPRITE.Imp(Icon.M)
                    {
                        public void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                        {
                            if (current.info.equipI(e) == 0)
                            {
                                OPACITY.O50.Bind();
                            }
                            e.resource.icon().Render(r, X1, X2, Y1, Y2);
                            OPACITY.Unbind();
                        }
                    };

                    RENDEROBJ o = new GStat()
                    {
                        public void Update(GText text)
                        {
                            if (current.info.equipI(e) == 0)
                            {
                                text.color(COLOR.WHITE50).add('-');
                            }
                            else
                            {
                                GFORMAT.f(text, ((int)(10 * (current.info.equipI(e)))) / 10.0);
                            }
                        }
                    }.hh(hh);
                    s.addGrid(o, e.indexMilitary(), 4, 48, 0);
                }

                GCOLOR.T().H1.Bind();
                s.add(UI.icons().s.death, 0, s.body().y2() + 2);

                s.addRightC(4, new GStat()
                {
                    public void Update(GText text)
                    {
                        GFORMAT.percGood(text, ((int)(100 * (current.info.experience()))) / 100.0);
                    }
                }.hh(Dic.¤¤Experience, 220));

                foreach (StatTraining tt in STATS.BATTLE().TRAINING_ALL)
                {
                    s.add(tt.room.icon.small, 0, s.body().y2() + 2);
                    s.addRightC(4, new GStat()
                    {
                        public void Update(GText text)
                        {
                            int target = ((int)(100 * current.info.training(tt)));
                            int cu = (int)Math.Round(100 * tt.stat.div().getD(current));

                            text.add(cu).add('/').add(target).add('%');
                            if (target > 0)
                                text.color(ColorImp.TMP.interpolate(GCOLOR.T().IBAD, GCOLOR.T().IGOOD, (double)cu / target));
                            else
                                text.color(GCOLOR.T().INACTIVE);
                        }
                    }.hh(tt.stat.info().name, 200));
                }

                s.add(UI.icons().s.sword, 0, s.body().y2() + 8);
                s.addRightC(4, new GStat()
                {
                    public void Update(GText text)
                    {
                        GFORMAT.i(text, training(current));
                    }
                }.hh(¤¤currently, 220));

                s.addRightC(4, new GStat()
                {
                    public void Update(GText text)
                    {
                        GFORMAT.i(text, needsTraining(current));
                    }
                }.hh(¤¤needs, 220));

                s.add(UI.icons().s.sword, 0, s.body().y2() + 8);
                s.addRightC(4, new GStat()
                {
                    public void Update(GText text)
                    {
                        GFORMAT.i(text, current.info.men() - needsTraining(current));
                    }
                }.hh(¤¤fully, 220));

                sec.add(s);
            }
        }

        public void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            body.set(X1, X2, Y1, Y2);
            if (current != null)
            {
                renderDivision(r, current);
            }
        }

        private void renderDivision(SPRITE_RENDERER r, Div div)
        {
            body.set(X1, X2, Y1, Y2);
            if (div != null)
            {
                // Implementation for rendering division
            }
        }

        public void hover(GUI_BOX box, Div div)
        {
            GBox b = (GBox)box;

            b.title(div.info.name());

            current = div;
            b.add(sec);

            b.sep();

            b.add(stat.get(div.info));
            b.sep();

            if (AD.cityDivs().attachedArmy(div) != null)
            {
                GText t = b.text().warnify();
                t.add(¤¤army);
                t.insert(0, AD.cityDivs().attachedArmy(div).name);
                b.add(t);
                b.NL(8);
            }
            else if (AD.cityDivs().daysToReturn(div) >= 0)
            {
                GText t = b.text().warnify();
                t.add(¤¤armyTime);
                t.insert(0, (int)Math.Ceiling(AD.cityDivs().daysToReturn(div)));
                b.add(t);
                b.NL(8);
            }

            b.sep();

            b.textLL(¤¤needs);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), needsTraining(div)));
            b.NL();

            b.textLL(¤¤currently);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), training(div)));
            b.NL();

            b.textLL(¤¤fully);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), div.info.men() - needsTraining(div)));
            b.NL();

            b.textLL(SETT.ROOMS().GUARD.activeDuty.info().name);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), STATS.POP().pop(HTYPES.GUARD(), div)));
            b.NL();
        }

        private class TrainingSpec
        {
            private int upI = -1;

            private readonly int[] needsTraining = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY);
            private readonly int[] training = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY);

            private readonly EntityIterator.Humans iter = new EntityIterator.Humans()
            {
                protected override bool processAndShouldBreakH(Humanoid h, int ie)
                {
                    if (h.indu().clas().player)
                        count(h);
                    return false;
                }

                private void count(Humanoid h)
                {
                    Div div = STATS.BATTLE().DIV.get(h);
                    if (div != null)
                    {
                        if (h.indu().hType() == HTYPES.RECRUIT())
                        {
                            training[div.indexArmy()]++;
                            needsTraining[div.indexArmy()]++;
                        }
                        else
                        {
                            foreach (ROOM_M_TRAINER<?> tra in ROOM_M_TRAINER.ALL())
                            {
                                if (tra.training().shouldTrain(h.indu(), div.info.training(tra.training()), false))
                                {
                                    needsTraining[div.indexArmy()]++;
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        div = STATS.BATTLE().RECRUIT.get(h);
                        if (div != null)
                        {
                            if (h.indu().hType() == HTYPES.RECRUIT())
                            {
                                training[div.indexArmy()]++;
                                needsTraining[div.indexArmy()]++;
                            }
                        }
                    }
                }
            };

            private void init()
            {
                if (GAME.updateI() == upI)
                    return;
                //int mul = ROOM_M_TRAINER.ALL().size();
                for (int di = 0; di < GAME.ARMIES().player().divisions().size(); di++)
                {
                    Div d = GAME.ARMIES().player().divisions().get(di);

                    needsTraining[di] = d.info.men() - (STATS.BATTLE().DIV.stat().div().get(d) + STATS.BATTLE().RECRUIT.inDiv(d));
                    if (AD.cityDivs().attachedArmy(d) != null)
                    {
                        needsTraining[di] -= AD.cityDivs().get(d).men();
                    }
                }

                Arrays.fill(training, 0);
                iter.iterate();
                upI = GAME.updateI();
            }
        }

        public int training(Div div)
        {
            spec.init();
            return spec.training[div.indexArmy()];
        }

        public int needsTraining(Div div)
        {
            spec.init();
            return spec.needsTraining[div.indexArmy()];
        }
    }
}