using System;
using System.Collections.Generic;
using Snake2D;

namespace View.Interrupter
{
    public class InterManager
    {
        private readonly Collection inters = new Collection();
        private Interrupter hovered = null;
        private Rec viewPort = new Rec();

        public InterManager()
        {
        }

        public void Add(Interrupter i)
        {
            if (i.AddManager != null)
                throw new RuntimeException("" + i);

            foreach (var inItem in inters)
                inItem.OtherAdd(i);

            if (i.DesturbingFuck)
            {
                foreach (var inItem in inters)
                {
                    if ((!inItem.IsPersistent() && !inItem.Pinned()) && inItem != i)
                        inItem.Hide();
                }
            }
            if (i.Last())
            {
                inters.Add(i);
            }
            else
            {
                inters.AddFirst(i);
            }

            i.AddManager = this;
        }

        public void Disturb()
        {
            foreach (var inItem in inters)
            {
                if (!inItem.Pinned() && !inItem.IsPersistent())
                    inItem.Hide();
            }
        }

        public void Remove(Interrupter interrupter)
        {
            inters.Remove(interrupter);
            interrupter.DeactivateAction();
            if (hovered == interrupter)
            {
                hovered = null;
            }
            interrupter.AddManager = null;
        }

        /**
         * 
         * @param mouseStillTime
         * @return false if should hover next
         */
        public bool HoverTimer(double mouseStillTime, GBox text)
        {
            if (hovered != null)
            {
                hovered.HoverTimer(text);
                return false;
            }
            return true;
        }

        /**
         * 
         * @param ds
         * @return false if there should be no more rendering
         */
        public bool Render(Renderer r, float ds)
        {
            r.NewLayer(true, 0);
            foreach (var i in inters)
            {
                if (!i.Render(r, ds))
                {
                    r.NewLayer(true, 0);
                    return false;
                }
                r.NewLayer(true, 0);
            }
            return true;
        }

        /**
         * 
         * @param button
         * @return false if nothing more should be clicked
         */
        public bool Click(MButt button)
        {
            foreach (var i in inters)
            {
                if (i == hovered)
                {
                    hovered.MouseClick(button);
                    return false;
                }

                if (i.OtherClick(button))
                    return false;
            }

            return true;
        }

        /**
         * 
         * @return if the next should update
         */
        public bool Update(float ds)
        {
            bool ret = true;
            foreach (var i in inters)
            {
                if (!i.Update(ds))
                    ret = false;
            }
            foreach (var i in inters)
            {
                if (!i.DoWhateverAndAllowOthersToDoWhatever())
                    break;
            }

            return ret;
        }

        public void AfterTick()
        {
            foreach (var i in inters)
            {
                i.AfterTick();
            }
            viewPort.Set(C.DIM());
        }

        /**
         * 
         * @param mCoo
         * @param mouseHasMoved
         * @return false if shouldn't hover
         */
        public bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            hovered = null;
            foreach (var i in inters)
            {
                if (i.Hover(mCoo, mouseHasMoved))
                {
                    hovered = i;
                    return false;
                }
            }
            return true;
        }

        public void Clear()
        {
            foreach (var i in inters)
            {
                if (!i.Pinned())
                {
                    i.Hide();
                    if (hovered == i)
                        hovered = null;
                }
            }
        }

        public bool IsHovered()
        {
            return hovered != null;
        }

        public Rec ViewPort()
        {
            return viewPort;
        }

        private class Collection : IEnumerable<Interrupter>, IEnumerator<Interrupter>
        {
            private int i;
            private readonly ArrayList<Interrupter> all = new ArrayList<Interrupter>(64);

            public bool HasNext => i < all.Size;

            public Interrupter Next => all.Get(i++);

            public void Reset()
            {
                i = 0;
            }

            public IEnumerator<Interrupter> GetEnumerator()
            {
                Reset();
                return this;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                Reset();
                return this;
            }

            public void Add(Interrupter i)
            {
                all.Add(i);
            }

            public void AddFirst(Interrupter i)
            {
                all.Insert(0, i);
                if (this.i > 0)
                    this.i--;
            }

            public void Remove(Interrupter i)
            {
                int index = all.RemoveOrdered(i);
                if (index < 0)
                    throw new RuntimeException();
                if (index <= this.i)
                    this.i--;
            }
        }

        public bool IsGoodTimeToSave()
        {
            foreach (var i in inters)
            {
                if (i is ToolManager)
                {
                    var t = (ToolManager)i;
                    if (t.Current() is ToolPlacer)
                        return false;
                }
            }
            return true;
        }

        public bool CanSave()
        {
            foreach (var i in inters)
            {
                if (!i.CanSave())
                    return false;
            }
            return true;
        }
    }
}