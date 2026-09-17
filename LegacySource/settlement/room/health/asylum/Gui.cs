using System;
using System.Text;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room;

namespace settlement.room.health.asylum
{
    class Gui : UIRoomModuleImp<AsylumInstance, ROOM_ASYLUM>
    {
        private static string ¤¤Treatment = "¤Treatment";
        private static string ¤¤TreatmentD = "¤Treatment factor is determined my the number of employed wards and degrade of the room. Keep rooms fully employed for the best recover rates.";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_ASYLUM s) : base(s)
        {
        }

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<AsylumInstance> g, int x1, int y1)
        {
            section.addRelBody(8, DIR.S, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iofkNoColor(text, g.get().prisoners(), g.get().prisonersMax());
                }
            }.hv(HTYPES.DERANGED().names));

            section.addRelBody(8, DIR.S, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.perc(text, blueprint.treatmentFactor(g.get()));
                }
            }.hv(¤¤Treatment).hoverInfoSet(¤¤TreatmentD));
        }

        protected override void appendMain(GGrid grid, GGrid text, GuiSection sExtra)
        {
            RENDEROBJ r = null;

            r = new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iofk(text, blueprint.prisoners(), blueprint.prisonersMax());
                }
            }.hh(SPRITES.icons().s.crazy).hoverInfoSet(HTYPES.DERANGED().desc);

            text.add(r);
        }

        protected override void hover(GBox box, AsylumInstance i)
        {
            box.NL();
            box.text(HTYPES.DERANGED().names);
            box.add(GFORMAT.iofk(box.text(), i.prisoners(), i.prisonersMax()));
        }
    }
}