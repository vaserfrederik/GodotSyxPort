using settlement.room.service.nursery;
using settlement.room.industry.module;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.data;
using util.gui.misc;
using util.info;
using view.sett.ui.room;

class Gui : UIRoomModuleImp<NurseryInstance, ROOM_NURSERY>
{
    public Gui(ROOM_NURSERY s) : base(s)
    {
    }

    protected override void appendPanel(GuiSection section, GGrid grid, GETTER<NurseryInstance> getter, int x1, int y1)
    {
        GuiSection s = new GuiSection();

        s.add(new GStat()
        {
            public void update(GText text)
            {
                GFORMAT.f0(text, IndustryUtil.calcProductionRate(blueprint.ChildPErE, blueprint.rate, blueprint.bonus(), getter.get()));
            }

            public void hoverInfoGet(GBox b)
            {
                b.title(blueprint.bonus().name);
                b.text(blueprint.bonus().desc);
                b.NL();
                IndustryUtil.hoverProductionRate(b, blueprint.ChildPErE, blueprint.rate, blueprint.bonus(), getter.get());
            }
        }.hh(blueprint.bonus().icon));

        section.addRelBody(8, DIR.S, s);
    }
}