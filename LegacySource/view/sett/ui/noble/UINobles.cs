using System;
using System.Collections.Generic;
using game;
using init.sprite.UI;
using init.type;
using snake2d.util.datatypes;
using snake2d.util.gui.GUI_BOX;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.gui.table;

namespace view.sett.ui.noble
{
    public sealed class UINobles : ISidePanel
    {
        private static readonly string ¤¤expla = "To assign another noble you must click a subject and elevate them from there.";

        static UINobles()
        {
            D.ts(typeof(UINobles));
        }

        private readonly NobleAssigns assigns = new NobleAssigns();

        public UINobles()
        {
            TitleSet(HCLASSES.NOBLE().names);

            Section.AddRelBody(0, DIR.S, new GStat
            {
                Update = (GText text) =>
                {
                    GFORMAT.iofkInv(text, GAME.NOBLE().active().size(), (int)GAME.NOBLE().MAX.get(HCLASS_RACE.clP()));
                },
                HoverInfoGet = (GBox b) =>
                {
                    b.Text(¤¤expla);
                    b.NL();
                    GAME.NOBLE().MAX.hoverDetailed(b, HCLASS_RACE.clP(), null, true);
                }
            }.Hv(HCLASSES.NOBLE().names));

            Section.AddRelBody(80, DIR.E, new GStat
            {
                Update = (GText text) =>
                {
                    GFORMAT.iofkInv(text, GAME.NOBLE().ranksAllocated(), (int)GAME.NOBLE().MAX_RANKS.get(HCLASS_RACE.clP()));
                },
                HoverInfoGet = (GBox b) =>
                {
                    b.Text(GAME.NOBLE().MAX_RANKS.desc);
                    b.NL();
                    GAME.NOBLE().MAX_RANKS.hoverDetailed(b, HCLASS_RACE.clP(), null, true);
                }
            }.Hv(GAME.NOBLE().MAX_RANKS.name));

            Section.AddRelBody(80, DIR.E, new GButt.ButtPanel(UI.icons().m.plus)
            {
                HoverInfoGet = (GUI_BOX text) =>
                {
                    GBox b = text as GBox;

                    for (int si = 0; si < GAME.NOBLE().boosters.all().Count; si++)
                    {
                        BoostSpec s = GAME.NOBLE().boosters.all()[si];
                        double v = s.get(HCLASS_RACE.clP());
                        if (v > 0)
                        {
                            GAME.NOBLE().boosters.hover(b, s, v, 0);
                            b.Tab(8);

                            b.NL();
                        }
                    }
                }
            });

            GTableBuilder bu = new GTableBuilder
            {
                NrOFEntries = () => GAME.NOBLE().active().size()
            };

            bu.Column(null, NobleRow.width, new GRowBuilder
            {
                Build = (GETTER<int> ier) => new NobleRow(ier)
            });

            Section.AddRelBody(8, DIR.S, bu.CreateHeight(HEIGHT - 32 - Section.Body().Height(), false));
        }
    }
}