using System.Collections.Generic;
using Snake2D.Util.Gui;
using View.Interrupter;
using View.Main;
using World.Entity.Army;

namespace View.World.Ui.Army
{
    public sealed class UIArmies
    {
        private readonly List list = new List();
        public readonly Army army = new Army();
        private readonly Hoverer hoverer = new Hoverer();

        public void OpenList(WArmy f)
        {
            OpenList(f, VIEW.World().Panels);
        }

        public void Open(WArmy f)
        {
            VIEW.World().Panels.Add(army.Get(f), true);
        }

        public void OpenList(WArmy f, ISidePanels m)
        {
            m.Add(list, true);
            if (f != null)
            {
                list.Set(f);
                m.Add(army.Get(f), false);
                VIEW.World().Window.Centerer.Set(f.Body().CX(), f.Body().CY());
            }
            else
            {
                m.Add(list, true);
            }
        }

        public bool ListIsOpen(ISidePanels m)
        {
            return m.Added(list);
        }

        public void Close(ISidePanels m)
        {
            m.Remove(list);
        }

        public void Hover(GUI_BOX box, WArmy a)
        {
            hoverer.Hover(box, a);
        }
    }
}