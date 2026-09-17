using settlement.room.main;
using util.gui.misc;

namespace view.sett.ui.room
{
    public abstract class UIRoomBulkApplier
    {
        protected readonly string name;

        public UIRoomBulkApplier(string name)
        {
            this.name = name;
        }

        protected abstract void Apply(RoomInstance t);

        protected void Hover(GBox b)
        {
            
        }
    }
}