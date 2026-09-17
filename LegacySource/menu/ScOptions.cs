using System;
using System.Collections.Generic;
using init.settings;
using menu.GUI;
using snake2d.util.gui;
using util.gui.misc;
using util.text;
using view.menu;

namespace menu
{
    final class ScOptions : Shadower, SC
    {
        private readonly LinkedList<Option> options = new LinkedList<Option>();
        private readonly CLICKABLE revert;

        static CharSequence ¤¤name = "¤settings";
        static CharSequence ¤¤revert = "¤revert";
        static CharSequence ¤¤default = "¤restore";

        static
        {
            D.ts(typeof(ScOptions));
        }

        ScOptions(Menu menu)
        {
            MenuScreen screen = new MenuScreen(¤¤name, GUI.labelColor)
            {
                protected override void back()
                {
                    menu.switchScreen(menu.main);
                }
            };

            revert = new MenuScreen.ScreenButton(¤¤revert)
            {
                protected override void clickA()
                {
                    revert();
                }
            };

            screen.addButt(revert);

            add(screen);

            GuiSection keys = new GuiSection()
            {
                public override bool click()
                {
                    if (base.click())
                    {
                        revert.activeSet(true);
                    }
                    return false;
                }
            };
            keys.body().moveY1(left.y1());
            int x1 = 0;

            foreach (init.settings.S.Setting s in S.get().all())
            {
                Option kc = new Option(s);
                if (keys.getLastY2() > left.y2())
                {
                    x1 += margin * 2 + margin * 0.2;
                    kc.body().moveY1(keys.body().y1());
                }
                else
                {
                    kc.body().moveY1(keys.getLastY2());
                }
                kc.body().moveX2(x1);
                keys.add(kc);
            }
            keys.body().centerIn(bounds);

            keys.body().centerIn(this);

            add(keys);
        }

        void make(Setting s, CharSequence name, GuiSection keys, int i)
        {
        }

        private class Option : OptionLine
        {
            private readonly Setting sett;

            protected Option(Setting s) : base(s, s.name)
            {
                sett = s;
                options.Add(this);
            }

            protected override void setValue(GText str)
            {
                sett.getValue(str);
            }

            public override bool click()
            {
                if (base.click())
                {
                    S.get().applyRuntimeConfigs();
                    return true;
                }
                return false;
            }
        }

        private void revert()
        {
            S.get().revert();
            S.get().applyRuntimeConfigs();
        }

        public void hoverInfoGet(GUI_BOX text)
        {
        }

        public bool back(Menu menu)
        {
            menu.switchScreen(menu.main);
            return true;
        }
    }
}