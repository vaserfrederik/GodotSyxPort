using System;
using System.Collections.Generic;
using snake2d;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.text;
using view.interrupter;
using view.main;
using view.subview;
using view.tool;
using view.ui.top;
using view.world.generator;
using view.world.panel;
using world;

namespace view.world.editor
{
    internal class TopPanel : Interrupter
    {
        private readonly GuiSection section;
        private readonly GuiSection buttons;
        private readonly Clickable current;
        private readonly Clickable primeButton;
        private readonly WorldResource primeResource;
        private readonly Map map;

        public TopPanel(WorldResource primeResource, WorldEditor worldEditor, UiManager uiManager)
        {
            this.primeResource = primeResource;
            section = new GuiSection();
            buttons = new GuiSection();
            current = new Clickable();
            primeButton = new PrimeButton(primeResource);
            map = new Map(worldEditor);

            SetupPanel(uiManager);
        }

        private void SetupPanel(UiManager uiManager)
        {
            buttons.Add(new ZoomButton("+", ZoomIn, () => worldEditor.Zoomout > 0));
            buttons.Add(new ZoomButton("-", ZoomOut, () => worldEditor.Zoomout < 3));

            buttons.Body().MoveX2(C.WIDTH() - 4);
            buttons.Body().MoveY1(0);

            section.Add(buttons);
            section.Add(map);

            Show(uiManager);
        }

        private void ZoomIn()
        {
            if (worldEditor.Zoomout > 0)
                worldEditor.SetZoomout(worldEditor.Zoomout - 1);
        }

        private void ZoomOut()
        {
            if (worldEditor.Zoomout < 3)
                worldEditor.SetZoomout(worldEditor.Zoomout + 1);
        }

        public override bool Render(Renderer r, float ds)
        {
            if (manager().ViewPort().Y2() >= C.HEIGHT() - section.Body().Height())
            {
                manager().ViewPort().SetHeight(C.HEIGHT() - section.Body().Height());
            }

            map.Render(r, ds);
            GCOLOR.UI().PanBG.Render(r, section.Body());
            section.Render(r, ds);

            GCOLOR.UI().Border(r, 0, C.WIDTH(), section.Body().Y1(), section.Body().Y1() + 3);
            return true;
        }

        public void Hide(bool yes)
        {
            section.VisableSet(yes);
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return section.Hover(mCoo) | map.Hover(mCoo) || mCoo.TouchesRec(section);
        }

        protected override void MouseClick(MButt button)
        {
            if (button == MButt.LEFT)
            {
                section.Click();
                map.Click();
            }
        }

        protected override void HoverTimer(GBox text)
        {
            section.HoverInfoGet(text);
            map.HoverInfoGet(text);
        }

        protected override bool Update(float ds)
        {
            return true;
        }

        private class Map : GuiSection
        {
            private readonly UIMinimapW map;

            public Map(WorldEditor worldEditor)
            {
                map = new UIMinimapW(worldEditor);
                Add(map);
            }

            public override void Render(Renderer r, float ds)
            {
                base.Render(r, ds);
                map.Render(r, ds);
            }
        }

        private class PrimeButton : Clickable
        {
            private readonly WorldResource res;

            public PrimeButton(WorldResource res)
            {
                this.res = res;
            }

            protected override void RenAction()
            {
                selectedSet(current.current() == row);
                if (selectedIs())
                    res.saver().AddDebugView();
            }

            protected override void ClickA()
            {
                current.set(row);
                // Assuming WorldEditor has a method to place a tool
                worldEditor.Tools.Place(res.saver().MakePlacers(worldEditor.Tools)[0]);
            }
        }

        private class ZoomButton : Clickable
        {
            private readonly Action action;
            private readonly Func<bool> activeCondition;

            public ZoomButton(string label, Action action, Func<bool> activeCondition)
            {
                this.action = action;
                this.activeCondition = activeCondition;
                hoverTitleSet(label);
            }

            protected override void RenAction()
            {
                activeSet(activeCondition());
            }

            protected override void ClickA()
            {
                action();
            }
        }
    }
}