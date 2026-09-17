using System;
using System.Collections.Generic;
using System.Linq;
using util.gui.misc;
using util.text;

namespace view.keyboard
{
    public class Key : INDEXED
    {
        private static Str stmp = new Str(64);
        private static CharSequence ¤¤none = "---";
        private static CharSequence ¤¤hotkey = "¤Hotkey: ";
        static Key()
        {
            D.ts(typeof(Key));
        }

        private readonly int modCodeDef;
        private readonly int keyCodeDef;
        private int modCode = -1;
        private int keyCode = -1;
        private readonly KeyPage map;
        public readonly string key;
        public readonly CharSequence name;
        public readonly CharSequence desc;
        public ACTION action;
        public bool isDown;
        public bool pressed;
        public readonly bool rebindable;

        public Key(string key, CharSequence name, CharSequence desc, KeyPage map)
            : this(key, name, desc, map, -1, -1)
        {
        }

        public Key(string key, CharSequence name, CharSequence desc, KeyPage map, int defCode)
            : this(key, name, desc, map, -1, defCode)
        {
        }

        public Key(string key, CharSequence name, CharSequence desc, KeyPage map, int defMod, int defCode)
            : this(key, name, desc, map, defMod, defCode, true)
        {
        }

        public KeyPage page()
        {
            return map;
        }

        public Key(string key, CharSequence name, CharSequence desc, KeyPage map, int defMod, int defCode, bool bindable)
        {
            this.key = key;
            this.name = name;
            this.desc = desc;
            this.map = map;
            modCodeDef = defMod;
            keyCodeDef = defCode;
            assign(defMod, defCode);
            rebindable = bindable;
            foreach (Key k in map.all)
                if (k.key.Equals(key))
                    throw new RuntimeException("" + k.key);
            map.all.Add(this);
        }

        public void read(Json json)
        {
            if (json.has(key))
            {
                int i = json.i(key);
                if (i == -1)
                    assign(-1, -1);
                else
                {
                    int mod = i / KEYCODES.lastCode();
                    mod -= 1;
                    int code = i % KEYCODES.lastCode();
                    assign(mod, code);
                }
            }
        }

        public void save(JsonE json)
        {
            int i = keyCode == -1 ? -1 : index();
            json.add(key, i);
        }

        public bool assign(int mod, int key)
        {
            if (mod == modCode && key == keyCode)
                return true;

            if (key != -1)
            {
                if (map != KEYS.MAIN() && KEYS.MAIN() != null && KEYS.MAIN().map.Contains(hash(mod, key)))
                {
                    if (!KEYS.MAIN().map[hash(mod, key)].rebindable)
                        return false;
                    KEYS.MAIN().map[hash(mod, key)].assign(-1, -1);
                }
                if (map == KEYS.MAIN() && KEYS.pages() != null)
                {
                    foreach (KeyPage p in KEYS.pages())
                    {
                        if (p == KEYS.MAIN())
                            continue;
                        if (p.map.Contains(hash(mod, key)))
                            p.map[hash(mod, key)].assign(-1, -1);
                    }
                }

                if (map.map.Contains(hash(mod, key)))
                {
                    if (!map.map[hash(mod, key)].rebindable)
                        return false;
                    map.map[hash(mod, key)].assign(-1, -1);
                }
            }

            if (keyCode != -1)
                map.map.Remove(index());
            this.modCode = mod;
            this.keyCode = key;
            if (keyCode != -1)
                map.map.Add(this);
            return true;
        }

        public void reset()
        {
            assign(modCodeDef, keyCodeDef);
        }

        public int modCode()
        {
            return modCode;
        }

        public int keyCode()
        {
            return keyCode;
        }

        public CharSequence repr()
        {
            if (keyCode == -1)
                return ¤¤none;
            stmp.clear();
            if (modCode != -1)
            {
                CharSequence code = KEYS.names.getCode(modCode);
                stmp.add(code);
                stmp.s().add('+').s();
            }
            CharSequence code = KEYS.names.getCode(keyCode);
            stmp.add(code);
            return stmp;
        }

        public void setMapping(GUI_BOX box)
        {
            GBox b = (GBox)box;
            b.textLL(¤¤hotkey);
            b.text(repr());
        }

        private static int hash(int modCode, int keyCode)
        {
            if (keyCode == -1)
                throw new RuntimeException();
            if (modCode == -1)
                return keyCode;
            return (modCode + 1) * KEYCODES.lastCode() + keyCode;
        }

        public bool hasMapping()
        {
            return keyCode >= 0;
        }

        public override int index()
        {
            return hash(modCode, keyCode);
        }

        public bool isPressed()
        {
            return isDown;
        }

        public bool consumeClick()
        {
            if (pressed)
            {
                pressed = false;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return map.key + " " + key;
        }
    }
}