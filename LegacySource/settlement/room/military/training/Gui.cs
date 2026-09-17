using System;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.info;
using util.text;
using view.sett.ui.room;

namespace settlement.room.military.training
{
    public class Gui : UIRoomModule
    {
        private static readonly CharSequence ¤¤Limit = "¤Recruits limit";
        private static readonly CharSequence ¤¤LimitD = "¤The number of recruits that you allow to train simultaneously.";

        private static readonly CharSequence ¤¤speed = "¤Training Speed";
        private static readonly CharSequence ¤¤speedD = "¤The speed at which subjects are trained.";
        private static readonly CharSequence ¤¤maxLevel = "¤Days to reach max level: ";

        static
        {
            D.ts(typeof(Gui));
        }

        private readonly ROOM_M_TRAINER<?> blueprint;

        protected Gui(ROOM_M_TRAINER<?> blueprint)
        {
            this.blueprint = blueprint;
        }

        public override void appendPanel(GuiSection section, GETTER<RoomInstance> getter, int x1, int y1)
        {
            INTE t = new INTE()
            {
                public int min()
                {
                    return 0;
                }

                public int max()
                {
                    return getter.get().employees().max();
                }

                public int get()
                {
                    return getter.get().employees().target();
                }

                public void set(int t)
                {
                    getter.get().employees().neededSet(t);
                }
            };

            GGaugeMutable m = new GGaugeMutable(t, 220)
            {
                protected override int setInfo(DOUBLE d, GText text)
                {
                    GFORMAT.i(text, t.get());
                    return 48;
                }
            };
            m.hoverInfoSet(¤¤LimitD);

            section.addRelBody(8, DIR.S, new GHeader(¤¤Limit).hoverInfoSet(¤¤LimitD));
            section.addRelBody(4, DIR.S, m);

            GStat ss = new GStat()
            {
                public void update(GText text)
                {
                    GFORMAT.perc(text, get());
                }

                double get()
                {
                    return IndustryUtil.calcProductionRate(1, null, blueprint.bonus(), getter.get());
                }

                public void hoverInfoGet(GBox b)
                {
                    b.title(¤¤speed);
                    b.text(¤¤speedD);

                    IndustryUtil.hoverProductionRate(b, 1, null, blueprint.bonus(), getter.get());

                    b.NL(8);
                    b.textLL(¤¤maxLevel);

                    double d = get();
                    int am = (int)Math.Ceiling(blueprint.TRAINING_DAYS / d);

                    b.add(GFORMAT.i(b.text(), am));
                }
            };

            section.addRelBody(8, DIR.S, ss.hv(¤¤speed));
        }

        public override void appendManageScr(GGrid icons, GGrid text, GuiSection extra)
        {
            GuiSection s = new GuiSection();

            INTE t = new INTE()
            {
                public int min()
                {
                    return 0;
                }

                public int max()
                {
                    return ENTETIES.MAX;
                }

                public int get()
                {
                    return blueprint.trainingLimit;
                }

                public void set(int t)
                {
                    blueprint.trainingLimit = t;
                }
            };

            s.addRelBody(0, DIR.S, new GHeader(¤¤Limit).hoverInfoSet(¤¤LimitD));
            s.addRelBody(0, DIR.S, new GSliderInt(t, 200, true));

            GStat ss = new GStat()
            {
                public void update(GText text)
                {
                    GFORMAT.perc(text, get());
                }

                double get()
                {
                    double d = blueprint.bonus().get(HCLASS_RACE.clP(null, null));
                    return d;
                }

                public void hoverInfoGet(GBox b)
                {
                    b.title(¤¤speed);
                    b.text(¤¤speedD);
                    b.NL(4);
                    blueprint.bonus().hover(b, HCLASS_RACE.clP(null, null), true);

                    b.NL(8);
                    b.textLL(¤¤maxLevel);

                    double d = get();
                    int am = (int)Math.Ceiling(blueprint.TRAINING_DAYS / d);

                    b.add(GFORMAT.i(b.text(), am));
                }
            };

            s.addRelBody(8, DIR.S, ss.hv(¤¤speed));

            text.section.addRelBody(0, DIR.S, s);

            base.appendManageScr(icons, text, extra);
        }
    }
}