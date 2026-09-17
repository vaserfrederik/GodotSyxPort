using System;
using System.Collections.Generic;
using game.faction;
using game.faction.FactionProfileFlusher;
using game.faction.player.PlayerColors;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.interrupter;
using view.main;

namespace view.ui.profile
{
    class ColorPop : ISidePanel
    {
        static readonly CharSequence ¤¤name = "Color Masks";
        static
        {
            D.ts(typeof(ColorPop));
        }
        private PlayerColor color;

        ColorPop()
        {
            titleSet(¤¤name);

            section.add(new GColorPicker(true)
            {
                public ColorImp color()
                {
                    return color.color;
                }
            });

            {
                GButt bb = new GButt.ButtPanel(Dic.¤¤save)
                {
                    protected override void clickA()
                    {
                        FactionProfileFlusher.flush(FACTIONS.player());
                    }
                };
                bb.body.setWidth(200);
                section.addRelBody(16, DIR.S, bb);

                bb = new GButt.ButtPanel(Dic.¤¤Reset)
                {
                    protected override void clickA()
                    {
                        PlayerColors.saver.clear();
                    }
                };
                bb.body.setWidth(200);
                section.addRelBody(0, DIR.S, bb);
            }

            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            foreach (string cat in PlayerColors.cats().keysSorted())
            {
                rows.add(new GTextR(new GText(UI.FONT().S, cat).lablify()));
                foreach (PlayerColor c in PlayerColors.cats().get(cat))
                {
                    Text t = new Text(UI.FONT().S, c.name);
                    t.setMaxChars(16);
                    if (color == null)
                        color = c;
                    GButt bb = new GButt.ButtPanel((SPRITE)t)
                    {
                        public override void hoverInfoGet(GUI_BOX text)
                        {
                            text.text(c.name);
                        }

                        protected override void clickA()
                        {
                            color = c;
                        }

                        protected override void renAction()
                        {
                            selectedSet(color == c);
                        }
                    };

                    bb.body.setWidth(220);
                    rows.add(bb);
                }
            }

            section.addRelBody(4, DIR.N, new GScrollRows(rows, HEIGHT - section.body().height() - 8).view());
        }

        public CLICKABLE butt()
        {
            SPRITE s = new SPRITE()
            {
                public int width()
                {
                    return 32;
                }

                public int height()
                {
                    return 32 + 16 * 3 + 4;
                }

                public void renderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
                {
                    // TODO Auto-generated method stub
                }

                public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    int x1 = X1 + 4;
                    UI.icons().m.place_brush.render(r, x1, Y1 + 4);
                    COLOR.RED100.render(r, x1, X2 - 4, Y1 + 32, Y1 + 32 + 16);
                    COLOR.GREEN100.render(r, x1, X2 - 4, Y1 + 32 + 16, Y1 + 32 + 16 + 16);
                    COLOR.BLUE100.render(r, x1, X2 - 4, Y1 + 32 + 16 + 16, Y1 + 32 + 16 + 16 + 16);
                }
            };

            return new GButt.ButtPanel(s)
            {
                protected override void clickA()
                {
                    VIEW.s().activate();
                    VIEW.s().panels.add(this, true);
                    VIEW.inters().manager.clear();
                }
            }.hoverInfoSet(¤¤name);
        }
    }
}