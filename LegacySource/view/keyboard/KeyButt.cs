using System;
using System.Text;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using util.text;

namespace view.keyboard
{
    public static class KeyButt
    {
        private static readonly CharSequence ¤¤assign = "¤To assign a new hotkey to this function, press: ";

        static KeyButt()
        {
            D.ts(typeof(KeyButt));
        }

        private KeyButt()
        {
        }

        public static CLICKABLE wrap(CLICKABLE baseClickable, Key key)
        {
            return new CLICKABLE.ClickWrap(baseClickable)
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.title(key.name);
                    text.text(key.desc);
                    text.NL(4);
                    base.hoverInfoGet(text);
                    text.NL(8);
                    key.setMapping(text);
                    text.NL(2);
                    if (key.rebindable)
                    {
                        text.text(¤¤assign);
                        text.text(KEYS.MAIN().ASSIGN_HOTKEY.repr());
                    }
                    text.NL(8);
                }

                public override bool hover(COORDINATE mCoo)
                {
                    if (base.hover(mCoo))
                    {
                        if (key.rebindable)
                            KEYS.get().toChange = key;
                        return true;
                    }
                    return false;
                }

                protected override RENDEROBJ pget()
                {
                    return baseClickable;
                }
            };
        }

        public static CLICKABLE wrap(ACTION a, CLICKABLE baseClickable, KeyPage page, string code, CharSequence name, CharSequence desc)
        {
            return wrap(a, baseClickable, page, code, name, desc, -1, -1);
        }

        public static CLICKABLE wrap(ACTION a, CLICKABLE baseClickable, KeyPage page, string code, CharSequence name, CharSequence desc, int mod, int key)
        {
            foreach (Key k in page.all)
            {
                if (k.key.Equals(code))
                {
                    k.action = a;
                    return wrap(baseClickable, k);
                }
            }

            Key k = new Key(code, name, desc, page, mod, key, true);
            k.action = a;
            return wrap(baseClickable, k);
        }

        public static void hover(Key key, GUI_BOX text)
        {
            text.title(key.name);
            text.text(key.desc);
            text.NL(8);
            key.setMapping(text);
            text.NL(2);
            if (key.rebindable)
            {
                text.text(¤¤assign);
                text.text(KEYS.MAIN().ASSIGN_HOTKEY.repr());
            }
            text.NL(8);
        }

        public static Key key(ACTION a, KeyPage page, string code, CharSequence name, CharSequence desc, int mod, int key)
        {
            Key k = new Key(code, name, desc, page, mod, key, true);
            return k;
        }
    }
}