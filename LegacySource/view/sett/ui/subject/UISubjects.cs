using System;
using init.type;
using settlement.entity.humanoid;
using settlement.room.main;
using settlement.stats;
using util.gui.misc;
using view.main;

namespace view.sett.ui.subject
{
    public class UISubjects
    {
        public readonly UISubjectsList list = new UISubjectsList();
        private readonly UISubject subject = new UISubject();
        public readonly UISubjectHoverer hoverer = new UISubjectHoverer();

        public UISubjects()
        {
        }

        public Humanoid Current()
        {
            Humanoid a = subject.Showing();
            if (a != null)
                return a;
            return null;
        }

        public void HoverInfo(Humanoid h, GBox text)
        {
            hoverer.Hover(h, text);
        }

        public void HoverInfoSoldier(Induvidual h, GBox text)
        {
            hoverer.Hover(h, text);
        }

        public void Show()
        {
            list.Show();
        }

        public bool ListActive()
        {
            return VIEW.S().Panels.Added(list);
        }

        public bool Shows(Humanoid h)
        {
            return Current() == h;
        }

        public bool Shows(HTYPE t)
        {
            return Current() != null && Current().Indu().HType() == t;
        }

        public void Show(Humanoid h)
        {
            list.Show(h);
            subject.Activate(h, false);
        }

        public void ShowSingle(Humanoid h)
        {
            subject.Activate(h, true);
        }

        public void ShowProfession(RoomInstance work)
        {
            list.ShowProfession(work);
        }

        public bool CanShow(Humanoid a)
        {
            return true;
        }
    }
}