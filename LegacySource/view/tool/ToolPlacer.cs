using System;
using System.Collections.Generic;
using init.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.text;
using view.keyboard;
using view.main;
using view.subview;

namespace view.tool
{
    public sealed class ToolPlacer : Tool
    {
        private bool pressed;

        private placeFunc current;
        private placeFunc normal;

        private PLACABLE placer;
        private PLACABLE origional;
        private PLACABLE undo;

        private bool buttonsStolen;
        private readonly GameWindow window;

        static ToolPlacer()
        {
            D.gInit(typeof(ToolPlacer));
        }

        public static AREA Area => PlacerArea.Self;

        private GButt.Panel buttUndo = new GButt.Panel(SPRITES.icons().m.cancel)
        {
            ClickA = () =>
            {
                if (placer != undo)
                {
                    placer = undo;
                    current = Get(placer);
                    current.Activate(placer, window);
                    SelectedSet(true);
                }
                else
                {
                    placer = origional;
                    current = Get(placer);
                    current.Activate(placer, window);
                    SelectedSet(true);
                }
            },
            RenAction = () =>
            {
                if (placer == undo)
                    SelectTmp();
            },
            HoverInfoGet = text =>
            {
                text.Text(undo.Name());
                text.Text(KEYS.MAIN().UNDO.Repr());
            }
        };

        private GButt.Panel buttExit = new GButt.Panel(SPRITES.icons().m.exit, D.g("Close"))
        {
            ClickA = () => Deactivate()
        };

        private readonly ToolConfig configDefault = new ToolConfig
        {
            AddUI = uis =>
            {
                if (!buttonsStolen)
                {
                    AddStandardButtons(uis, true);
                }

                buttonsStolen = false;
            }
        };

        private readonly GButtablePanel panel = new GButtablePanel();

        public void AddStandardButtons(LISTE<RENDEROBJ> uis, bool exitAlso)
        {
            panel.Clear();
            LIST<CLICKABLE> ps = normal.Gui();

            int w = 0;
            if (origional.GetAdditionalButt() != null)
            {
                foreach (CLICKABLE b in origional.GetAdditionalButt())
                {
                    w += b.Body().Width();
                }
            }

            if (w > 5 * Icon.M)
            {
                if (origional.GetAdditionalButt() != null)
                {
                    foreach (CLICKABLE b in origional.GetAdditionalButt())
                    {
                        panel.AddButton(b);
                    }
                }

                panel.Nl();

                if (ps != null)
                {
                    foreach (CLICKABLE b in ps)
                        panel.AddButton(b);
                }

                if (undo != null)
                {
                    panel.AddButton(buttUndo);
                }
            }
            else
            {
                if (ps != null)
                {
                    foreach (CLICKABLE b in ps)
                        panel.AddButton(b);
                }

                if (undo != null)
                {
                    panel.AddButton(buttUndo);
                }
                if (origional.GetAdditionalButt() != null)
                {
                    foreach (CLICKABLE b in origional.GetAdditionalButt())
                    {
                        panel.AddButton(b);
                    }
                }
            }

            panel.AddTitle(placer.Name());
            if (exitAlso)
                panel.AddButton(buttExit);

            if (exitAlso)
                panel.AddButton(buttExit);

            uis.Add(panel);
        }

        private readonly placeFunc multi = new PlacableMultiTool();

        private readonly placeFunc fixed = new PlacableFixedTool();

        private readonly placeFunc single2 = new PlacableSingleTool();

        private readonly placeFunc simple = new PlacableSimpleTool();

        private readonly placeFunc simpleTile = new PlacableSimpleTileTool();

        public ToolPlacer(ToolManager manager, GameWindow window) : base(manager)
        {
            this.window = window;
        }

        public void Activate(PLACABLE placer)
        {
            buttUndo.SelectedSet(false);

            this.placer = placer;
            this.origional = placer;
            this.undo = placer.GetUndo();

            normal = Get(placer);
            normal.Activate(placer, window);
            current = normal;

            pressed = false;
        }

        private placeFunc Get(PLACABLE placer)
        {
            if (placer is PlacableFixed)
                return fixed;
            if (placer is PlacableSingle)
                return single2;
            if (placer is PlacableMulti)
            {
                return multi;
            }
            else if (placer is PlacableSimple)
                return simple;
            else if (placer is PlacableSimpleTile)
                return simpleTile;
            else
            {
                throw new RuntimeException();
            }
        }

        public override void Click(GameWindow window)
        {
            pressed = true;
            current.Click(window);
        }

        public override void UpdateHovered(float ds, GameWindow window)
        {
            if (pressed && !MButt.LEFT.IsDown())
            {
                current.ClickRelease(window);
            }
            Update(ds, window);

            current.UpdateHovered(ds, window, pressed);
        }

        protected override void Update(float ds, GameWindow window)
        {
            if (KEYS.MAIN().UNDO.IsPressed() && placer == origional && placer.GetUndo() != null)
            {
                if (placer is PlacableMulti && placer.GetUndo() is PlacableMulti)
                {
                    ((PlacableMulti)placer.GetUndo()).Previous = ((PlacableMulti)placer).Previous;
                }
                placer = placer.GetUndo();
                if (placer == origional)
                    throw new RuntimeException("" + placer);
                current = Get(placer);

                current.Activate(placer, window);
            }
            else if (!buttUndo.SelectedIs() && !KEYS.MAIN().UNDO.IsPressed() && placer != origional)
            {
                placer = origional;
                current = Get(placer);
                current.Activate(placer, window);
            }

            if (placer == origional.GetUndo())
                VIEW.Mouse().SetReplacement(SPRITES.icons().m.cancel);

            current.Update(ds, window, pressed);
            pressed = pressed & MButt.LEFT.IsDown();
        }

        public override void RenderHovered(SPRITE_RENDERER r, float ds, GameWindow window, GBox box)
        {
            current.Render(r, ds, window);
        }

        protected override void Render(SPRITE_RENDERER r, float ds, GameWindow window)
        {
        }

        public void StealButtons(GuiSection s)
        {
            StealButtons(s, true);
        }

        public void StealButtons(GuiSection s, bool undo)
        {
            buttonsStolen = true;

            LIST<CLICKABLE> ps = normal.Gui();
            if (ps != null)
                foreach (CLICKABLE c in ps)
                {
                    s.AddRightC(0, c);
                    c.ActiveSet(true);
                }

            if (undo && this.undo != null)
            {
                s.AddRightC(0, buttUndo);
            }
        }

        public override bool RightClick()
        {
            if (pressed)
            {
                pressed = false;
                return false;
            }
            return true;
        }

        private abstract class placeFunc
        {
            protected placeFunc() { }

            public abstract void UpdateHovered(float ds, GameWindow window, bool pressed);
            public virtual void Update(float ds, GameWindow window, bool pressed) { }
            public abstract void Render(SPRITE_RENDERER r, float ds, GameWindow window);
            public abstract void Click(GameWindow window);
            public abstract void ClickRelease(GameWindow window);
            public abstract void Activate(PLACABLE placer, GameWindow window);
            public abstract LIST<CLICKABLE> Gui();
        }

        public PLACABLE GetCurrent()
        {
            if (!IsActivated())
                return null;
            return placer;
        }

        protected override ToolConfig DefaultConfig() => configDefault;
    }
}