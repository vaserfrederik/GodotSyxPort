using System;
using System.Collections.Generic;
using snake2d.util.gui;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.data;
using util.gui.misc;
using util.info;
using view.sett.ui.room;

namespace settlement.room.service.hygine.bath
{
    class Gui : UIRoomModuleImp<BathInstance, ROOM_BATH>
    {
        public Gui(ROOM_BATH s) : base(s)
        {
        }

        protected override void problem(BathInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
        {
            if (i.GetHeat() < 1)
            {
                errors.Add(blueprint.sHeatingProblem);
            }

            // if (i.water < 1)
            // {
            //     errors.Add(blueprint.sWaterProblem);
            // }

            base.problem(i, free, errors, warnings);
        }

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<BathInstance> getter, int x1, int y1)
        {
            grid.Add(new GStat()
            {
                public override void Update(GText text)
                {
                    GFORMAT.Perc(text, getter.Get().GetHeat());
                }
            }.Hh(blueprint.sHeating).HoverInfoSet(blueprint.sHeatingDesc));

            // grid.Add(new GStat()
            // {
            //     public override void Update(GText text)
            //     {
            //         GFORMAT.Perc(text, getter.Get().water);
            //     }
            // }.Hh(SETT.ENV().environment.WATER_SWEET.name).HoverInfoSet(SETT.ENV().environment.WATER_SWEET.desc));
        }
    }
}