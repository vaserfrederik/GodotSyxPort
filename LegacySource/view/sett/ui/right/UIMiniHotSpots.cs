using System;
using System.Collections.Generic;
using init.constant;
using init.sprite;
using init.sprite.UI;
using settlement.main;
using settlement.tilemap.SettMarks;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.clickable;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.data.INT;
using util.gui.misc;
using util.gui.panel;
using util.text;
using view.main;
using view.sett.ui.right;
using view.subview;
using view.tool;

namespace view.sett.ui.right
{
    final class UIMiniHotSpots : Expansion
    {
        private readonly int width = (int)(Icon.M * 1.5);
        private readonly GText text = new GText(UI.FONT().S, 20);
        private readonly ColorImp colorImp = new ColorImp();

        private readonly GameWindow window;
        private readonly Panel panel = new Panel();
        private static readonly CharSequence ¤¤order = "¤ORDER";
        private static readonly CharSequence ¤¤set = "¤Set Hotspot";
        private static readonly CharSequence ¤¤setLong = "¤Sets a hotspot that can easily be navigated to with a single click.";
        private static readonly CharSequence ¤¤setExp = "¤Left click to go to hotspot. Right click to edit.";

        static
        {
            D.ts(UIMiniHotSpots.class);
        }

        protected UIMiniHotSpots(int index, int y1, GameWindow window) : base(index)
        {
            this.window = window;
            add(new View(y1));
        }

        private readonly PlacableSingle placer = new PlacableSingle(¤¤set)
        {
            public override void placeFirst(int tx, int ty)
            {
                SettMark d = SETT.TILE_MAP().marks.make();
                if (d != null)
                    d.set(tx, ty);
                VIEW.s().tools.placer.deactivate();
            }

            public override CharSequence isPlacable(int tx, int ty)
            {
                return null;
            }

            public override SPRITE getIcon()
            {
                return SPRITES.icons().m.crossair;
            }
        };

        private class Button : CLICKABLE.ClickableAbs
        {
            private SettMark d;

            public Button(int i)
            {
                body.setWidth(width).setHeight(Icon.L);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                COLOR.WHITE25.render(r, body().x1(), body.x2(), body().y1(), body().y1() + 1);
                colorImp.set(d.color);
                if (isHovered || isSelected)
                    colorImp.shadeSelf(1.5);
                colorImp.render(r, body().x1(), body.x2(), body().y1(), body.y2() - 1);
                COLOR.WHITE25.render(r, body().x1(), body.x2(), body().y2() - 1, body.y2());

                if (isHovered || isSelected)
                    COLOR.WHITE15.render(r, body().x1(), body.x2(), body().y1() + 6, body().y2() - 6);
                else
                    COLOR.WHITE10.render(r, body().x1(), body.x2(), body().y1() + 6, body().y2() - 6);

                if (d.name.length() > 0)
                {
                    text.clear();
                    text.add(d.name, 0, 2);
                    text.adjustWidth();
                    text.renderC(r, body);
                }
            }

            public void set(SettMark d)
            {
                this.d = d;
            }

            protected override void clickA()
            {
                if (MButt.RIGHT.isDown())
                {
                    panel.init(d);
                    VIEW.inters().popup.show(panel, this);
                }
                window.centererTile.set(d.tile);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                b.textLL(d.name);
                b.text(¤¤setExp);
                base.hoverInfoGet(text);
            }
        }

        public CLICKABLE get(int y1)
        {
            return new View(y1);
        }

        private class View : GuiSection
        {
            private readonly GuiSection section = new GuiSection();
            private readonly CLICKABLE toggle;
            private int bi = 0;
            private readonly Button[] buttons;

            public View(int y1)
            {
                buttons = new Button[SETT.TILE_MAP().marks.max];
                for (int i = 0; i < SETT.TILE_MAP().marks.max; i++)
                    buttons[i] = new Button(i);

                body().setWidth(Icon.M * 1.5 + GFrame.MARGIN * 2).setHeight(C.HEIGHT() - y1);
                body().moveX2(C.WIDTH());
                body().moveY1(y1);

                section.merge(section);

                toggle = new GButt.Panel(SPRITES.icons().m.crossair)
                {
                    protected override void clickA()
                    {
                        if (SETT.TILE_MAP().marks.active().size() < SETT.TILE_MAP().marks.max)
                        {
                            VIEW.s().tools.place(placer);
                            return;
                        }
                    }
                }.hoverInfoSet(¤¤setLong);

                toggle.body().moveY1(body().y1() + 10);
                toggle.body().centerX(this);
                add(toggle);
                section.body().moveY1(toggle.body().y1());
                section.body().centerX(body());
                add(section);
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                if (bi != SETT.TILE_MAP().marks.state())
                {
                    section.clear();
                    int i = 0;
                    foreach (SettMark b in SETT.TILE_MAP().marks.active())
                    {
                        if (b.active)
                        {
                            buttons[i].set(b);
                            section.addDownC(0, buttons[i]);
                            i++;
                        }
                    }
                    section.body().centerX(toggle);
                    section.body().moveY1(toggle.body().y2() + 8);
                    bi = SETT.TILE_MAP().marks.state();
                }
                if (visableIs())
                {
                    GCOLOR.UI().panBG.render(r, body());
                    base.render(r, ds);
                    GCOLOR.UI().borderH(r, body(), 0);
                }
            }
        }

        private class Panel : GuiSection
        {
            GInput name;
            private SettMark data;

            public void init(SettMark data)
            {
                this.data = data;
                name.text().clear().add(data.name);
                //name.focus();
            }

            public Panel()
            {
                name = new GInput(new StringInputSprite(20, UI.FONT().M)
                {
                    protected override void change()
                    {
                        data.name.clear().add(text());
                    }
                });
                add(name, 0, 0);

                addRightC(20, new GButt.Panel(SPRITES.icons().m.trash)
                {
                    protected override void clickA()
                    {
                        data.remove();
                        VIEW.inters().popup.close();
                    }
                });

                addRelBody(C.SG * 8, DIR.S, new GColorPicker(false)
                {
                    public override ColorImp color()
                    {
                        return data.color;
                    }
                });

                INTE order = new INTE()
                {
                    public override int min()
                    {
                        return 0;
                    }

                    public override int max()
                    {
                        int i = 0;
                        foreach (SettMark d in SETT.TILE_MAP().marks.active())
                        {
                            if (d.active)
                                i++;
                        }
                        return i - 1;
                    }

                    public override int get()
                    {
                        int i = 0;
                        foreach (SettMark d in SETT.TILE_MAP().marks.active())
                        {
                            if (d == data)
                                return i;
                            if (d.active)
                                i++;
                        }
                        return -1;
                    }

                    public override void set(int t)
                    {
                        data.setPosition(t);
                        foreach (SettMark d in SETT.TILE_MAP().marks.active())
                        {
                            if (t == 0 && d.active)
                            {
                                init(d);
                                break;
                            }
                            if (d.active)
                                t--;
                        }
                    }
                };

                GTarget t = new GTarget(40, false, true, order);

                addRelBody(C.SG * 8, DIR.S, new GText(UI.FONT().H2, ¤¤order).toUpper().lablify());
                addRelBody(C.SG * 2, DIR.S, t);
            }
        }
    }
}