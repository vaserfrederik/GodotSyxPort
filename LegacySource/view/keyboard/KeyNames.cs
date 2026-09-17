using System;
using System.Collections.Generic;
using snake2d;
using util.text;

namespace view.keyboard
{
    class KeyNames
    {
        private readonly string[] names;
        private static readonly string none = "---";
        private static readonly string unknown = "???";

        public KeyNames()
        {
            D.t(this);

            names = new string[KEYCODES.lastCode() + 1];

            foreach (int i in KEYCODES.all)
            {
                names[i] = CORE.getInput().getKeyboard().translate(i);
            }

            names[KEYCODES.KEY_SPACE] = D.g("space");
            names[KEYCODES.KEY_ESCAPE] = D.g("escape");
            names[KEYCODES.KEY_CAPS_LOCK] = D.g("caps-lock");
            names[KEYCODES.KEY_SCROLL_LOCK] = D.g("scroll-lock");
            names[KEYCODES.KEY_NUM_LOCK] = D.g("num-lock");
            names[KEYCODES.KEY_PAUSE] = D.g("pause");
            names[KEYCODES.KEY_ENTER] = D.g("enter");
            names[KEYCODES.KEY_TAB] = D.g("tab");
            names[KEYCODES.KEY_BACKSPACE] = D.g("backspace");
            names[KEYCODES.KEY_INSERT] = D.g("insert");
            names[KEYCODES.KEY_DELETE] = D.g("delete");
            names[KEYCODES.KEY_RIGHT] = D.g("right");
            names[KEYCODES.KEY_LEFT] = D.g("left");
            names[KEYCODES.KEY_DOWN] = D.g("down");
            names[KEYCODES.KEY_UP] = D.g("up");
            names[KEYCODES.KEY_PAGE_UP] = D.g("page-up");
            names[KEYCODES.KEY_PAGE_DOWN] = D.g("page-down");
            names[KEYCODES.KEY_HOME] = D.g("home");
            names[KEYCODES.KEY_END] = D.g("end");
            names[KEYCODES.KEY_PRINT_SCREEN] = D.g("print-screen");
            names[KEYCODES.KEY_LEFT_SHIFT] = D.g("left-shift");
            names[KEYCODES.KEY_LEFT_CONTROL] = D.g("left-ctrl");
            names[KEYCODES.KEY_RIGHT_SHIFT] = D.g("right-shift");
            names[KEYCODES.KEY_RIGHT_CONTROL] = D.g("right-ctrl");
            names[KEYCODES.KEY_KP_ENTER] = D.g("pad-enter");
            names[KEYCODES.KEY_KP_EQUAL] = D.g("pad-equals");
            names[KEYCODES.KEY_LEFT_ALT] = D.g("left-alt");
            names[KEYCODES.KEY_LEFT_SUPER] = D.g("left-super");
            names[KEYCODES.KEY_MENU] = D.g("menu");
            names[KEYCODES.KEY_RIGHT_ALT] = D.g("right-alt");
            names[KEYCODES.KEY_RIGHT_SUPER] = D.g("right-super");

            string ff = D.g("F", "F{0}");
            for (int i = KEYCODES.KEY_F1; i <= KEYCODES.KEY_F25; i++)
            {
                int k = (i - KEYCODES.KEY_F1 + 1);
                names[i] = $"{new Str(ff).insert(0, k)}";
            }

            foreach (int i in KEYCODES.all)
            {
                if (names[i] == null || names[i].Length == 0)
                    LOG.ln(i);
            }
        }

        public string getCode(int code)
        {
            if (code < 0)
            {
                return none;
            }
            if (code >= names.Length || names[code] == null)
            {
                return unknown;
            }
            return names[code];
        }
    }
}