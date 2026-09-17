using System;
using System.Collections.Generic;
using snake2d;
using util.gui.common;
using util.gui.misc;
using util.text;
using view.main;

namespace view.ui.div
{
    public class UIDivBannerEditor
    {
        private int bannerI = 0;
        GuiSection pop = new GuiSection();

        RENDEROBJ prevPop;
        CLICKABLE prevPopC;

        public UIDivBannerEditor()
        {
            BitmapSpriteEditor ee = new BitmapSpriteEditor();
            ee.spriteSet(GAME.ARMIES().banners.get(0).sprite);

            for (int i = 0; i < GAME.ARMIES().banners.size(); i++)
            {
                int k = i;

                GButt.ButtPanel bu = new GButt.ButtPanel(GAME.ARMIES().banners.get(i))
                {
                    protected override void clickA()
                    {
                        bannerISet(k);
                        ee.spriteSet(GAME.ARMIES().banners.get(k).sprite);
                    }

                    protected override void renAction()
                    {
                        selectedSet(bannerI == k);
                    }
                };
                bu.pad(2, 0);
                pop.add(bu, (i % 10) * bu.body.width(), (i / 10) * bu.body.height());
            }

            pop.addRelBody(8, DIR.S, ee);

            GuiSection c = new GuiSection();

            c.addRelBody(8, DIR.S, new GColorPicker(false)
            {
                public ColorImp color()
                {
                    return GAME.ARMIES().banners.get(bannerI).col;
                }
            });

            c.addRelBody(8, DIR.E, new GColorPicker(false)
            {
                public ColorImp color()
                {
                    return GAME.ARMIES().banners.get(bannerI).bg;
                }
            });

            pop.addRelBody(8, DIR.S, c);

            pop.addRelBody(8, DIR.S, new GButt.ButtPanel(Dic.¤¤Accept)
            {
                protected override void clickA()
                {
                    VIEW.inters().popup.pop();
                }
            });
        }

        public int bannerI()
        {
            return bannerI;
        }

        public void bannerISet(int i)
        {
            bannerI = i;
        }

        public RENDEROBJ view()
        {
            return pop;
        }

        public CLICKABLE butt()
        {
            SPRITE sp = new SPRITE.Imp(32)
            {
                public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    GAME.ARMIES().banners.get(bannerI()).render(r, X1 + 2, Y1 + 2);
                }
            };

            return new GButt.ButtPanel(sp)
            {
                protected override void clickA()
                {
                    VIEW.inters().popup.push(view(), this);
                }
            }.hoverTitleSet(Dic.¤¤Banner);
        }
    }
}