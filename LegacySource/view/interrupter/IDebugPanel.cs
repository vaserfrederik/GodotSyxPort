using System;
using System.Collections.Generic;
using System.Linq;

namespace View.Interrupter
{
    using Game;
    using Init.Sprite.UI;
    using Snake2D;
    using Snake2D.Util.Gui.Clickable;
    using Snake2D.Util.Misc;
    using Util;
    using Util.Data.Boolean;
    using Util.Gui.Misc;
    using View.Main;
    using View.Ui.Util;

    public class IDebugPanel : IDebugPanelAbs
    {
        static IDebugPanel()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    hash.Clear();
                }
            };
        }

        private static readonly SortedDictionary<string, IClickable> hash = new SortedDictionary<string, IClickable>(StringComparer.OrdinalIgnoreCase);

        private static IClickable Get(string name, BOOLEAN_MUTABLE toggle)
        {
            GButt.Checkbox c = new GButt.Checkbox(UI.FONT().S.GetText(name))
            {
                protected override void ClickA()
                {
                    SelectedToggle();
                    toggle.Set(SelectedIs());
                }

                protected override void Render(SpriteRenderer r, float ds, bool isActive, bool isSelected, bool isHovered)
                {
                    SelectedSet(toggle.Is());
                    base.Render(r, ds, isActive, isSelected, isHovered);
                }
            };
            return c;
        }

        private static IClickable Get(string name, ACTION action)
        {
            IClickable c = new GButt.Glow(UI.FONT().S.GetText(name))
            {
                protected override void ClickA()
                {
                    VIEW.Inters().Debugpanel.Hide();
                }
            };
            c.ClickActionSet(action);
            return c;
        }

        private static void Put(string key, IClickable obj)
        {
            while (hash.ContainsKey(key))
                key += 'I';
            hash[key.ToLower()] = obj;
        }

        public static void Add(string name, BOOLEAN_MUTABLE toggle)
        {
            Put(name, Get(name, toggle));
        }

        public static void Add(string name, ACTION a)
        {
            Put(name, Get(name, a));
        }

        public IDebugPanel(InterManager manager) : base(AddStaticStuff(manager), hash)
        {
        }

        protected override void AddMisc()
        {
            // Add("Timeoff", new ACTION()
            // {
            //     final GuiSection ss = TIME.debugSection();
            //     public override void Exe()
            //     {
            //         VIEW.GetInterrupters().Message.Activate(ss);
            //     }
            // });
        }

        static InterManager AddStaticStuff(InterManager manager)
        {
            Add("show stats", new BOOLEAN_MUTABLE()
            {
                public override BOOLEAN_MUTABLE Set(bool boolValue)
                {
                    GUTIL.Debugger().Toggle();
                    return this;
                }

                public override bool Is()
                {
                    return GUTIL.Debugger().IsToggled();
                }
            });

            Add("crash", new ACTION()
            {
                public override void Exe()
                {
                    throw new Exception("Crash");
                }
            });

            Add("hideUI(cancel with esc)", new ACTION()
            {
                public override void Exe()
                {
                    VIEW.Hide();
                }
            });

            Add("garbage Collect", new ACTION()
            {
                public override void Exe()
                {
                    new CORE.GlJob()
                    {
                        protected override void DoJob()
                        {
                            GC.Collect();
                        }
                    }.Perform();
                }
            });

            new VideoMaker();

            return manager;
        }
    }
}