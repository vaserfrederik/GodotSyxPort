using System;
using System.Collections.Generic;
using snake2d;
using util.data;
using util.gui.misc;
using view.interrupter;
using view.main;
using view.tool;

namespace view.sett
{
    public class IDebugPanelSett : IDebugPanelAbs
    {
        private static readonly SortedDictionary<string, CLICKABLE> hash = new SortedDictionary<string, CLICKABLE>();

        static IDebugPanelSett()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    hash.Clear();
                }
            };
        }

        private static CLICKABLE Get(string name, BOOLEAN_MUTABLE toggle)
        {
            GButt.Checkbox c = new GButt.Checkbox(UI.FONT().S.GetText(name))
            {
                protected override void ClickA()
                {
                    SelectedToggle();
                    toggle.Set(SelectedIs());
                }

                protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                {
                    SelectedSet(toggle.Is());
                    base.Render(r, ds, isActive, isSelected, isHovered);
                }
            };
            return c;
        }

        private static CLICKABLE Get(string name, ACTION action)
        {
            CLICKABLE c = new GButt.Glow(UI.FONT().S.GetText(name))
            {
                protected override void ClickA()
                {
                    VIEW.S().debug.hide();
                }
            };
            c.ClickActionSet(action);
            return c;
        }

        public static void Add(string name, BOOLEAN_MUTABLE toggle)
        {
            Put(name, Get(name, toggle));
        }

        public static void Add(string name, params PLACABLE[] placables)
        {
            foreach (PLACABLE p in placables)
            {
                Put(name + ": " + p.Name(), Get(name + ": " + p.Name(), new ACTION
                {
                    public override void Exe()
                    {
                        VIEW.S().tools.place(p);
                    }
                }));
            }
        }

        public static void Add(string name, IEnumerable<PLACABLE> placables)
        {
            foreach (PLACABLE p in placables)
            {
                Put(name + ": " + p.Name(), Get(name + ": " + p.Name(), new ACTION
                {
                    public override void Exe()
                    {
                        VIEW.S().tools.place(p);
                    }
                }));
            }
        }

        public static void Add(PLACABLE placable)
        {
            Put(placable.Name(), Get(placable.Name(), new ACTION
            {
                public override void Exe()
                {
                    VIEW.S().tools.place(placable);
                }
            }));
        }

        public static void Add(string key, ACTION action)
        {
            Put(key, Get(key, action));
        }

        public static void Add(string prefix, PLACABLE placable)
        {
            Put(prefix + ": " + placable.Name(), Get(prefix + ": " + placable.Name(), new ACTION
            {
                public override void Exe()
                {
                    VIEW.S().tools.place(placable);
                }
            }));
        }

        private static void Put(string key, CLICKABLE obj)
        {
            string s = key;

            while (hash.ContainsKey(s))
                s += s + 'I';

            hash.Add(s.ToLower(), obj);
        }

        public IDebugPanelSett(InterManager m) : base(m, Init())
        {
            hash.Clear();
        }

        private static SortedDictionary<CharSequence, CLICKABLE> Init()
        {
            return hash;
        }
    }
}