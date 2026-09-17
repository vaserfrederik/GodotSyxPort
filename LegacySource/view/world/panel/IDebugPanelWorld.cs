using System;
using System.Collections.Generic;

namespace View.World.Panel
{
    using Game;
    using Init.Sprite.UI;
    using Snake2D;
    using Snake2D.Util.Datatypes;
    using Snake2D.Util.Gui.Clickable;
    using Snake2D.Util.Map;
    using Snake2D.Util.Misc;
    using Util.Data.Boolean;
    using Util.Gui.Misc;
    using View.Interrupter;
    using View.Main;
    using View.Tool;

    public class IDebugPanelWorld : IDebugPanelAbs
    {
        private static readonly SortedDictionary<string, IClickable> hash = new SortedDictionary<string, IClickable>();

        static IDebugPanelWorld()
        {
            GameDisposable.Add(() =>
            {
                hash.Clear();
            });
        }

        private static IClickable Get(CharSequence name, BOOLEAN_MUTABLE toggle)
        {
            GButt.Checkbox c = new GButt.Checkbox(UI.FONT().S.GetText(name))
            {
                ClickAction = () =>
                {
                    SelectedToggle();
                    toggle.Set(SelectedIs());
                },
                Render = (r, ds, isActive, isSelected, isHovered) =>
                {
                    SelectedSet(toggle.Is());
                    base.Render(r, ds, isActive, isSelected, isHovered);
                }
            };
            return c;
        }

        private static IClickable Get(CharSequence name, ACTION action)
        {
            IClickable c = new GButt.Glow(UI.FONT().S.GetText(name))
            {
                ClickAction = () =>
                {
                    VIEW.World().Debug.Hide();
                }
            };
            c.ClickActionSet(action);
            return c;
        }

        private static void Put(CharSequence key, IClickable obj)
        {
            string s = "" + key;
            while (hash.ContainsKey(s))
                s += 'I';
            hash.Add(s.ToLower(), obj);
        }

        public static void Add(CharSequence name, BOOLEAN_MUTABLE toggle)
        {
            Put(name, Get(name, toggle));
        }

        public static void Add(CharSequence name, ACTION action)
        {
            Put(name, Get(name, action));
        }

        public static void Add(PLACABLE placable)
        {
            Put(placable.Name(), Get(placable.Name(), new ACTION
            {
                Exe = () =>
                {
                    VIEW.World().Tools.Place(placable);
                }
            }));
        }

        public static void Add(PLACABLE placable, string prefix)
        {
            Put(placable.Name(), Get(prefix + " " + placable.Name(), new ACTION
            {
                Exe = () =>
                {
                    VIEW.World().Tools.Place(placable);
                }
            }));
        }

        public static void Add(MAP_PLACER placable, string name)
        {
            Add(new PlacableMulti(name)
            {
                Place = (tx, ty, area, type) =>
                {
                    placable.Set(tx, ty);
                },
                IsPlacable = (tx, ty, area, type) =>
                {
                    // TODO Auto-generated method stub
                    return null;
                }
            });
        }

        public static void AddClear(MAP_PLACER placable, string name)
        {
            Add(new PlacableMulti(name)
            {
                Place = (tx, ty, area, type) =>
                {
                    placable.Clear(tx, ty);
                },
                IsPlacable = (tx, ty, area, type) =>
                {
                    // TODO Auto-generated method stub
                    return null;
                }
            });
        }

        public static void Add(string name, ACTION a)
        {
            Put(name, Get(name, a));
        }

        public IDebugPanelWorld(InterManager m) : base(m, Init())
        {
            hash.Clear();
        }

        private static SortedDictionary<CharSequence, IClickable> Init()
        {
            return hash;
        }
    }
}