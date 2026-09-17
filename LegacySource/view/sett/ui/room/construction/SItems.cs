using System;
using System.Collections.Generic;
using init.sprite;
using settlement.room.main.furnisher;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.main;

namespace view.sett.ui.room.construction
{
    final class SItems
    {
        private static readonly CharSequence ¤¤Items = "¤items";
        //private final RENDEROBJ upgrade;
        private readonly RENDEROBJ title = new GHeader(¤¤Items).subify();
        private readonly GuiSection section = new GuiSection();
        private readonly RENDEROBJ table;
        private readonly GuiSection stolen = new GuiSection();
        private readonly State state;
        private readonly IButt[] butts = new IButt[8];

        static SItems()
        {
            D.ts(typeof(SItems));
        }

        public SItems(State state)
        {
            this.state = state;

            GTableBuilder b = new GTableBuilder()
            {
                NrOFEntries = () => state.b == null ? 0 : state.b.constructor().groups().size()
            };

            b.Column(null, 190, new GRowBuilder()
            {
                Build = (GETTER<int> ier) => new IButt(state, ier)
            });

            table = b.Create(4, false);

            for (int i = 0; i < butts.Length; i++)
            {
                int k = i;
                GETTER<int> g = new GETTER<int>()
                {
                    Get = () => k
                };
                butts[i] = new IButt(state, g);
            }

            // upgrade = new GTarget(32, new GText(UI.FONT().S, DicMisc.¤¤Upgrade).lablifySub(), false, true, new INTE()
            // {
            //     Min = () => 0,
            //     Max = () => state.b != null ? state.b.upgrades().max() : 0,
            //     Get = () => CLAMP.i(state.upgrade[state.b.index()], 0, Max()),
            //     Set = (int t) =>
            //     {
            //         state.upgrade[state.b.index()] = t;
            //         SETT.ROOMS().placement.placer.setUpgrade(t);
            //     }
            // });
        }

        public GuiSection Get()
        {
            section.Clear();

            section.AddRightC(0, title);
            stolen.Clear();
            stolen.Body().SetDim(1, 32);
            if (VIEW.s().tools.Is(state.placement.placer.itemPlacerCurrent()))
                VIEW.s().tools.placer.stealButtons(stolen, true);
            section.AddRightC(8, stolen);
            section.Add(table, 0, section.GetLastY2() + 4);

            // if (state.upgradeMax[state.b.index()] > 0)
            // {
            //     section.AddRelBody(2, DIR.N, upgrade);
            // }

            return section;
        }

        public GuiSection GetFlat()
        {
            section.Clear();
            section.AddRightC(0, title);
            stolen.Clear();
            stolen.Body().SetDim(1, 32);
            if (VIEW.s().tools.Is(state.placement.placer.itemPlacerCurrent()))
                VIEW.s().tools.placer.stealButtons(stolen, true);
            section.AddRightC(8, stolen);
            int y1 = section.GetLastY2() + 4;
            for (int i = 0; i < state.b.constructor().groups().size(); i++)
            {
                section.Add(butts[i], (i % 2) * butts[0].Body.Width(), y1 + (i / 2) * butts[0].Body.Height());
            }

            // if (state.upgradeMax[state.b.index()] > 0)
            // {
            //     section.AddRelBody(2, DIR.N, upgrade);
            // }

            return section;
        }

        public GuiSection GetSingle()
        {
            section.Clear();
            section.AddRightC(0, title);
            stolen.Clear();
            stolen.Body().SetDim(1, 32);
            if (VIEW.s().tools.Is(state.placement.placer.itemPlacerCurrent()))
                VIEW.s().tools.placer.stealButtons(stolen, true);
            section.AddRightC(8, stolen);
            // if (state.upgradeMax[state.b.index()] > 0)
            // {
            //     section.AddRelBody(2, DIR.N, upgrade);
            // }

            return section;
        }

        private static class IButt : GButt.ButtPanel
        {
            private readonly GETTER<int> ier;
            private readonly State state;

            public IButt(State state, GETTER<int> ier) : base(new GStat()
            {
                Update = text =>
                {
                    text.lablify().add(state.b.constructor().groups().get(ier.Get()).name());
                }
            })
            {
                this.state = state;
                this.ier = ier;
                SetDim(190, 24);
            }

            protected override void RenAction()
            {
                selectedSet(VIEW.s().tools.placer.getCurrent() == state.placement.placer.itemPlacerCurrent() && state.item() == ier.Get());
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                base.Render(r, ds, isActive, isSelected, isHovered);
                if (state.problemGroup == state.b.constructor().groups().get(ier.Get()) && state.problemTimer > VIEW.renderSecond())
                {
                    COLOR.RED100.renderFrame(r, body, 2, 3);
                    OPACITY.O25To50.bind();
                    COLOR.RED100.render(r, body);
                    OPACITY.unbind();
                }
            }

            protected override void ClickA()
            {
                if (state.placement.placer.item(ier.Get()) == null)
                    return;

                state.setItem(ier.Get());
                VIEW.s().tools.place(state.placement.placer.item(ier.Get()), state.config);
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                text.title(state.b.constructor().groups().get(ier.Get()).name());
                text.text(state.b.constructor().groups().get(ier.Get()).desc());

                GBox b = (GBox)text;
                b.NL(8);
                foreach (FurnisherStat s in state.b.constructor().stats())
                {
                    double d = state.b.constructor().groups().get(ier.Get()).stat(s.index());
                    if (d < 0)
                    {
                        b.error(s.name());
                        b.tab(6);
                        b.add(SPRITES.icons().m.minus);
                    }
                    else if (d > 0)
                    {
                        b.text(s.name());
                        b.tab(6);
                        b.add(SPRITES.icons().m.plus);
                    }
                    b.NL();
                }
            }
        }
    }
}