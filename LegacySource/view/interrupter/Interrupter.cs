using System;
using snake2d;
using snake2d.util.datatypes;
using util.gui.misc;
using view.main.VIEW;

namespace view.interrupter
{
    public abstract class Interrupter
    {
        protected bool persistent;
        protected bool desturbingfuck;
        private bool pinned;

        private bool last;

        protected InterManager addManager;

        protected Interrupter()
        {
            this(false, false, false);
        }

        protected Interrupter(bool persistent, bool pinned)
        {
            this(persistent, pinned, true);
        }

        protected Interrupter(bool persistent, bool pinned, bool desturber)
        {
            this.persistent = pinned || persistent;
            this.pinned = pinned;
            desturbingfuck = desturber;
        }

        protected final bool Show(ViewSub view)
        {
            return Show(view.uiManager);
        }

        protected final bool Show(InterManager manager)
        {
            if (addManager == null)
            {
                manager.Add(this);
                return true;
            }
            return false;
        }

        protected void Hide()
        {
            if (addManager != null)
            {
                addManager.Remove(this);
            }
        }

        protected void DeactivateAction()
        {
        }

        protected void OtherAdd(Interrupter other)
        {
        }

        protected bool DoWhateverAndAllowOthersToDoWhatever()
        {
            return true;
        }

        protected abstract bool Hover(COORDINATE mCoo, bool mouseHasMoved);

        protected abstract void MouseClick(MButt button);

        protected bool OtherClick(MButt button)
        {
            return false;
        }

        protected abstract void HoverTimer(GBox text);

        protected abstract bool Render(Renderer r, float ds);

        protected abstract bool Update(float ds);

        protected void AfterTick()
        {
        }

        public final bool Last()
        {
            return last;
        }

        public Interrupter LastSet()
        {
            this.last = true;
            return this;
        }

        public Interrupter PersistentSet()
        {
            this.persistent = true;
            return this;
        }

        public Interrupter DesturberSet()
        {
            this.desturbingfuck = true;
            return this;
        }

        internal final bool IsPersistent()
        {
            return persistent;
        }

        internal final bool Pinned()
        {
            return pinned;
        }

        public final Interrupter Pin()
        {
            pinned = true;
            return this;
        }

        public final bool IsActivated()
        {
            return addManager != null;
        }

        public InterManager Manager()
        {
            return addManager;
        }

        public bool CanSave()
        {
            return true;
        }
    }
}