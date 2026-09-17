using System;
using System.Collections.Generic;
using game.faction;
using init.sprite.UI;
using settlement.room.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite;
using util.gui.misc;
using view.main;
using view.sett.ui.bottom;

namespace view.sett.ui.room.construction
{
    final class SCollection
    {
        private readonly State state;
        private readonly IButt[] butts = new IButt[32];
        private readonly IIButt[] item = new IIButt[32];

        private GuiSection section = new GuiSection();
        private GuiSection isection = new GuiSection();
        SCollection(State state)
        {
            this.state = state;

            for (int i = 0; i < butts.Length; i++)
            {
                butts[i] = new IButt(state, i);
                item[i] = new IIButt(state, i);
            }
        }

        GuiSection get()
        {
            section.clear();
            if (state.collection == null || state.collection.rooms().Count == 0)
                return section;
            for (int i = 0; i < state.collection.rooms().Count; i++)
            {
                section.addGrid(butts[i], i, 8, 0, 0);
            }
            isection.clear();

            if (state.b.constructor().groups().Count > 1)
            {
                for (int i = 0; i < state.b.constructor().groups().Count; i++)
                    isection.addGrid(item[i], i, 2, 0, 0);
            }
            section.addRelBody(2, DIR.S, isection);

            return section;
        }

        static class IButt : GButt.ButtPanel
        {
            private readonly State state;
            private readonly int k;
            IButt(State state, int k) : base(new SPRITE.Imp(init.sprite.UI.Icon.L)
            {
                public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    RoomBlueprintImp b = state.collection.rooms()[k];
                    b.iconBig().render(r, X1, X2, Y1, Y2);
                }
            })
            {
                this.state = state;
                this.k = k;
            }

            protected override void renAction()
            {
                RoomBlueprintImp b = state.collection.rooms()[k];
                activeSet(b != null && b.reqs.passes(FACTIONS.player()));
                selectedSet(state.b == state.collection.rooms()[k]);
            }

            protected override void clickA()
            {
                VIEW.s().ui.placer.init(state.collection.rooms()[k], state.collection);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                UIRoomBuild.hoverRoomBuild(state.collection.rooms()[k], text);
            }
        }

        static class IIButt : GButt.ButtPanel
        {
            private readonly State state;
            private readonly int k;
            IIButt(State state, int k) : base(new SPRITE.Imp(175, UI.FONT().S.height())
            {
                public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    RoomBlueprintImp b = state.b;
                    UI.FONT().S.renderCropped(r, b.constructor().groups().getC(k).name, X1, Y1, 175);
                }
            })
            {
                this.state = state;
                this.k = k;
                pad(2, 1);
            }

            protected override void renAction()
            {
                selectedSet(state.item() == k);
            }

            protected override void clickA()
            {
                state.setItem(k);
                VIEW.s().ui.placer.init(state.b, state.collection);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                RoomBlueprintImp b = state.b;
                text.title(b.constructor().groups().getC(k).name);
            }
        }
    }
}