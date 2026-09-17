using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using util.gui.misc;
using util.gui.panel;
using util.text;
using view.keyboard;

namespace util.gui.common
{
    public class BitmapSpriteEditor : GuiSection
    {
        private BitmapSprite sprite;
        private static readonly string ¤¤hovInfo = "¤Hold left mouse button to draw. Hold ({0}) to erase.";

        static BitmapSpriteEditor()
        {
            D.ts(typeof(BitmapSpriteEditor));
        }

        public BitmapSpriteEditor(BitmapSprite s)
        {
            this.sprite = s;

            ColorImp col = new ColorImp();
            int pixelDim = 24;
            for (int y = 0; y < BitmapSprite.HEIGHT; y++)
            {
                for (int x = 0; x < BitmapSprite.WIDTH; x++)
                {
                    int x1 = x;
                    int y1 = y;

                    CLICKABLE c = new ClickableAbs(pixelDim, pixelDim)
                    {
                        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                        {
                            int i = 80;
                            if (sprite != null)
                            {
                                i = sprite.is(x1, y1) ? 20 : 80;
                                if (isHovered && MButt.LEFT.isDown())
                                {
                                    sprite.set(x1, y1, !KEYS.MAIN().MOD.isPressed());
                                }
                                if (isHovered)
                                    i += 30;
                            }

                            col.set(i, i, i);
                            col.render(r, body());
                        }

                        public override void hoverInfoGet(GUI_BOX text)
                        {
                            GBox b = (GBox)text;
                            GText t = b.text();
                            t.add(¤¤hovInfo);
                            t.insert(0, KEYS.MAIN().MOD.repr());
                            b.add(t);
                        }
                    };
                    c.hoverSoundSet(null);
                    add(c, x * pixelDim, y * pixelDim);
                }
            }

            GFrame f = new GFrame(body());
            add(f);
        }

        public BitmapSpriteEditor() : this(null) { }

        public void spriteSet(BitmapSprite s)
        {
            this.sprite = s;
        }

        public BitmapSprite spriteGet()
        {
            return sprite;
        }
    }
}