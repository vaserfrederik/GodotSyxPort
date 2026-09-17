using settlement.room.service.arena;
using init.sprite.UI;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room;

public class RoomArenaGui : UIRoomModule
{
    private static readonly string ¤¤Executions = "Executions";
    private static readonly string ¤¤ExecutionsD = "The amount of prisoners that are currently being executed.";

    static
    {
        D.ts(typeof(RoomArenaGui));
    }

    private readonly RoomArenaWork w;

    public RoomArenaGui(RoomArenaWork w)
    {
        this.w = w;
    }

    public override void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
    {
        section.addRelBody(8, DIR.S, new GStat()
        {
            public override void update(GText text)
            {
                GFORMAT.iofk(text, w.executions(get.get()), w.executionsMax(get.get()));
            }
        }.hh(¤¤Executions).hoverInfoSet(¤¤ExecutionsD));
    }

    public override void appendManageScr(GGrid icons, GGrid text, GuiSection extra)
    {
        icons.add(new GStat()
        {
            public override void update(GText text)
            {
                GFORMAT.iofk(text, w.executions(), w.executionsMax());
            }
        }.hh(UI.icons().s.death).hoverTitleSet(¤¤Executions).hoverInfoSet(¤¤ExecutionsD));

        // TODO Auto-generated method stub
        base.appendManageScr(icons, text, extra);
    }
}