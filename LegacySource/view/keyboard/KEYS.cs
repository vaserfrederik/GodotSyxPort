using System;
using System.Collections.Generic;
using init.paths;
using snake2d;
using snake2d.KeyBoard;
using snake2d.util.file;
using snake2d.util.sets;
using view.main;

namespace view.keyboard
{
    public class KEYS : KeyPoller
    {
        static readonly KeyNames names = new KeyNames();
        private static KEYS self;

        private readonly KeyPageMain MAIN;
        private readonly KeyPageSett SETT;
        private readonly KeyPageWorld WORLD;
        private readonly KeyPageBattle BATTLE;
        private readonly IChange iii = new IChange();
        Key toChange;

        private readonly LIST<KeyPage> views;
        private bool anyDown;

        private KEYS()
        {
            self = this;

            MAIN = new KeyPageMain();
            SETT = new KeyPageSett();
            WORLD = new KeyPageWorld();
            BATTLE = new KeyPageBattle();
            views = new ArrayList<KeyPage>(
                MAIN, SETT, WORLD, BATTLE
            );
        }

        public static LIST<KeyPage> pages()
        {
            return self.views;
        }

        public static KeyPageMain MAIN()
        {
            return self.MAIN;
        }

        public static KeyPageSett SETT()
        {
            return self.SETT;
        }

        public static KeyPageWorld WORLD()
        {
            return self.WORLD;
        }

        public static KeyPageBattle BATTLE()
        {
            return self.BATTLE;
        }

        public static KEYS get()
        {
            return self;
        }

        public static KeyPoller init()
        {
            self = new KEYS();
            return self;
        }

        public override void poll(LIST<KeyEvent> keys)
        {
            KeyPage other = MAIN;
            if (VIEW.s().isActive())
                other = SETT;
            else if (VIEW.world().isActive())
                other = WORLD;
            else if (VIEW.s().battle.isActive() || VIEW.b().isActive())
                other = BATTLE;

            foreach (KeyPage m in self.views)
            {
                if (m == MAIN || m == other)
                {
                    foreach (Key k in m.all)
                    {
                        if (k.action != null && k.consumeClick())
                        {
                            k.action.exe();
                        }
                    }
                }
                foreach (Key k in m.all)
                {
                    k.isDown = false;
                    k.pressed = false;
                }
            }
            self.anyDown = false;

            int mod = -1;

            foreach (Key k in MAIN.all)
            {
                k.isDown = false;
                k.pressed = false;
                if (mod == -1 && k.modCode() != -1 && CORE.getInput().getKeyboard().isPressed(k.modCode()))
                {
                    mod = k.modCode();
                    anyDown = true;
                }
            }
            if (other != null)
            {
                foreach (Key k in other.all)
                {
                    k.isDown = false;
                    k.pressed = false;
                    if (mod == -1 && k.modCode() != -1 && CORE.getInput().getKeyboard().isPressed(k.modCode()))
                    {
                        mod = k.modCode();
                        anyDown = true;
                    }
                }
            }

            foreach (KeyEvent e in keys)
            {
                if (e.action() == KEYACTION.PRESS)
                {
                    Key k = check(mod, e.code(), other);
                    if (k != null)
                    {
                        k.pressed = true;
                        anyDown = true;
                    }
                }
            }

            foreach (Key k in MAIN.all)
            {
                if (k.keyCode() != -1 && (k.modCode() == mod || (k.modCode() == -1 && k.keyCode() == mod)) && CORE.getInput().getKeyboard().isPressed(k.keyCode()))
                {
                    k.isDown = true;
                    anyDown = true;
                }
            }
            if (other != null)
            {
                foreach (Key k in other.all)
                {
                    if (k.keyCode() != -1 && (k.modCode() == mod || (k.modCode() == -1 && k.keyCode() == mod)) && CORE.getInput().getKeyboard().isPressed(k.keyCode()))
                    {
                        k.isDown = true;
                        anyDown = true;
                    }
                }
            }

            if (toChange != null && MAIN.ASSIGN_HOTKEY.isDown)
            {
                iii.show(toChange);

                clear();
            }
        }

        private Key check(int mod, int code, KeyPage other)
        {
            if (mod != -1)
            {
                int c = Key.hash(mod, code);
                if (MAIN.map.contains(c))
                {
                    return MAIN.map.get(c);
                }
                else if (other != null && other.map.contains(c))
                {
                    return other.map.get(c);
                }
            }

            if (MAIN.map.contains(code))
            {
                return MAIN.map.get(code);
            }
            else if (other != null && other.map.contains(code))
            {
                return other.map.get(code);
            }
            return null;
        }

        public void restore()
        {
            foreach (KeyPage m in views)
            {
                foreach (Key k in m.all)
                {
                    k.reset();
                }
            }
        }

        public static bool anyDown()
        {
            return self.anyDown;
        }

        public static bool moveDown()
        {
            return self.MAIN.SCROLL_LEFT.isDown || self.MAIN.SCROLL_RIGHT.isDown || self.MAIN.SCROLL_UP.isDown || self.MAIN.SCROLL_DOWN.isDown;
        }

        public static void bind(Key key)
        {
            clear();
            self.iii.show(key);
        }

        public static void clear()
        {
            foreach (KeyPage m in self.views)
            {
                foreach (Key k in m.all)
                {
                    k.isDown = false;
                    k.pressed = false;
                }
            }
            self.anyDown = false;
            self.toChange = null;
        }

        public void readSettings()
        {
            restore();
            if (!PATHS.local().SETTINGS.exists("Keyboard"))
                return;

            try
            {
                Json json = new Json(PATHS.local().SETTINGS.gets("Keyboard"));

                for (int ii = 0; ii < views.size(); ii++)
                {
                    KeyPage m = views.get(ii);

                    if (json.has(m.key))
                    {
                        Json j = json.json(m.key);
                        for (int ki = 0; ki < m.all.size(); ki++)
                        {
                            Key k = m.all.get(ki);
                            k.read(j);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                e.printStackTrace(Console.Out);
                restore();
                save();
            }
        }

        public void save()
        {
            JsonE json = new JsonE();
            foreach (KeyPage m in views)
            {
                JsonE j = new JsonE();

                foreach (Key k in m.all)
                {
                    k.save(j);
                }

                json.add(m.key, j);
            }

            if (!PATHS.local().SETTINGS.exists("Keyboard"))
                PATHS.local().SETTINGS.create("Keyboard");
            json.save(PATHS.local().SETTINGS.get("Keyboard"));
        }
    }
}