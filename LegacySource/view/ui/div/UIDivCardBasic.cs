using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.gui.misc;
using util.info;
using util.text;

namespace view.ui.div
{
    public sealed class UIDivCardBasic : IDimension
    {
        private readonly int WIDTH;
        private readonly int HEIGHT;
        private readonly UIDiv m;
        private readonly Rec body = new Rec();

        private GuiSection sec = new GuiSection();
        private DIV_SPEC current;
        private readonly UIDivStats stat = new UIDivStats();

        public UIDivCardBasic(UIDiv m)
        {
            this.m = m;
            this.WIDTH = m.WIDTH;
            this.HEIGHT = m.HEIGHT;

            {
                GuiSection s = new GuiSection();

                foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
                {
                    SPRITE hh = new SPRITE.Imp(Icon.M)
                    {
                        public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                        {
                            if (current.equip(e) == 0)
                            {
                                OPACITY.O50.bind();
                            }
                            e.resource.icon().render(r, X1, X2, Y1, Y2);
                            OPACITY.unbind();
                        }
                    };

                    RENDEROBJ o = new GStat()
                    {
                        public override void update(GText text)
                        {
                            if (current.equip(e) == 0)
                            {
                                text.color(COLOR.WHITE50).add('-');
                            }
                            else
                            {
                                GFORMAT.f(text, ((int)(10 * (current.equip(e) * e.equipMax))) / 10.0, 1);
                            }
                        }
                    }.hh(hh);
                    s.addGrid(o, e.indexMilitary(), 4, 48, 0);
                }

                GCOLOR.T().H1.bind();
                s.add(UI.icons().s.death, 0, s.body().y2() + 2);

                s.addRightC(4, new GStat()
                {
                    public override void update(GText text)
                    {
                        GFORMAT.percGood(text, ((int)(100 * (current.experience()))) / 100.0);
                    }
                }.hh(Dic.¤¤Experience, 220));

                foreach (StatTraining tt in STATS.BATTLE().TRAINING_ALL)
                {
                    s.add(tt.room.icon.small, 0, s.body().y2() + 2);
                    s.addRightC(4, new GStat()
                    {
                        public override void update(GText text)
                        {
                            GFORMAT.percGood(text, ((int)(100 * current.training(tt))) / 100.0);
                        }
                    }.hh(tt.stat.info().name, 220));
                }
                sec.add(s);

                sec.addRelBody(8, DIR.W, new RENDEROBJ.RenderImp(WIDTH * 2, HEIGHT * 2)
                {
                    public override void render(SPRITE_RENDERER r, float ds)
                    {
                        UIDivCardBasic.this.render(r, body.x1(), body.y1(), 2, current, true, false, false);
                    }
                });
            }
        }

        public int width()
        {
            return WIDTH;
        }

        public int height()
        {
            return HEIGHT;
        }

        public void render(SPRITE_RENDERER r, int x1, int y1, int scale, DIV_SPEC d, bool isActive, bool isSelected, bool isHovered)
        {
            if (d == null)
                return;

            body.set(x1, x1 + WIDTH * scale, y1, y1 + HEIGHT * scale);
            GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);

            m.renderBasics(r, x1, y1, scale, d);

            int cx = body.cX();

            COLOR.BLACK.bind();
            UI.FONT().S.renderC(r, cx + 1, body.y2() - 9 * scale, Str.TMP.clear().add(d.men()), scale);
            COLOR.unbind();
            UI.FONT().S.renderC(r, cx, body.y2() - 10 * scale, Str.TMP.clear().add(d.men()), scale);

            if (d.men() == 0 || !isActive)
            {
                OPACITY.O50.bind();
                COLOR.BLACK.render(r, body);
                OPACITY.unbind();
            }

            GCOLOR.UI().border().renderFrame(r, body, 0, 1);
        }

        public void hover(DIV_SPEC d, GUI_BOX box)
        {
            if (d == null)
                return;
            GBox b = (GBox)box;

            b.title(d.name());

            current = d;
            b.add(sec);

            b.sep();

            b.add(stat.get(d));
        }
    }
}