using settlement.room.health.physician;
using snake2d.util.gui;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using view.sett.ui.room;

namespace Settlement.Room.Health.Physician
{
    class Gui : UIRoomModuleImp<Instance, ROOM_PHYSICIAN>
    {
        public Gui(ROOM_PHYSICIAN s) : base(s)
        {
        }

        public override void hover(GBox box, Instance i)
        {
            base.hover(box, i);
        }

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<Instance> getter, int x1, int y1)
        {
        }

        protected override void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
        {
        }
    }
}