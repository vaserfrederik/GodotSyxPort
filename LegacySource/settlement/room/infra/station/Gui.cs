using System;
using System.Collections.Generic;
using System.Linq;
using game;
using init.resources;
using init.settings;
using init.sprite.UI;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.infra.transport;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.renderable;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using view.sett.ui.room;

class Gui : UIRoomModuleImp<StationInstance, ROOM_STATION>
{
    private static readonly CharSequence ¤Accepting = "Accepting";
    private static readonly CharSequence ¤Capacity = "Capacity";
    private static readonly CharSequence ¤Crates = "Crates";
    private static readonly CharSequence ¤Incoming = "Incoming";
    private static readonly CharSequence ¤Reserved = "Reserved";
    private static readonly CharSequence ¤Resources = "Resources";
    private static readonly CharSequence ¤Stored = "Stored";

    private int bonusAccepting;
    private int bonusIncoming;

    public Gui() : base(new GuiSection())
    {
        var g = new GuiSection();
        g.add(new GHeader(¤Resources));
        g.add(new GScrollRows(GenerateResourceLines(), 350));
        g.add(new GStat(() => $"Total Crates: {g.get().crates.size()}"));
        g.pad(8, 8);

        body().add(g);
    }

    private List<RENDEROBJ> GenerateResourceLines()
    {
        var lines = new List<RENDEROBJ>();

        foreach (var res in RESOURCES.ALL())
        {
            lines.Add(new ResLine(res, g => g.get().tally(res)));
        }

        return lines;
    }

    private class ResLine : HoverableAbs
    {
        private readonly RESOURCE res;
        private readonly Func<StationInstance, StationTally> tallyGetter;

        public ResLine(RESOURCE res, Func<StationInstance, StationTally> tallyGetter) : base(300, 32)
        {
            this.res = res;
            this.tallyGetter = tallyGetter;
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
        {
            res.icon().renderCY(r, body.x1(), body.cY());
            var ins = g.get();
            var t = tallyGetter(ins);

            int x2 = body.x2() - 48;
            GMeter.render(r, GMeter.C_REDGREEN, (double)t.stored() / t.space(), (double)(t.stored() + ins.incoming(res)) / t.space(), body.x1() + 30, x2, body.y1() + 4, body.y2() - 4);

            Str.TMP.clear().add(t.stored());
            int w = UI.FONT().S.width(Str.TMP);
            OPACITY.O50.bind();
            COLOR.BLACK.render(r, x2 - w - 8, x2, body.y1() + 5, body.y2() - 5);
            OPACITY.unbind();

            UI.FONT().S.renderCY(r, x2 - w - 4, body.cY(), Str.TMP);

            if (ins.accepting(res))
            {
                GCOLOR.T().IGOOD.bind();
            }
            else
            {
                GCOLOR.T().IBAD.bind();
            }
            UI.icons().s.storage.renderCY(r, x2 + 8, body.cY());

            if (SETT.ROOMS().TRANSPORT.hasActive(res))
            {
                GCOLOR.T().IGOOD.bind();
            }
            else
            {
                GCOLOR.T().IBAD.bind();
            }
            UI.icons().s.chevron(DIR.E).renderCY(r, x2 + 24, body.cY());
            COLOR.unbind();
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            GBox b = (GBox)text;
            b.title(res.name);

            b.textLL(¤Stored);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), tallyGetter(g.get()).stored()));
            b.NL();

            b.textLL(¤Capacity);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), tallyGetter(g.get()).space()));
            b.NL();

            b.textLL(¤Reserved);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), tallyGetter(g.get()).reserved()));
            b.NL();

            b.textLL(¤Incoming);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), g.get().incoming(res)));
            b.NL();

            if (g.get().accepting(res))
            {
                b.add(b.text().normalify2().add(¤Accepting));
            }
            else
            {
                b.error(¤AcceptingNot);
            }
            if (S.get().developer)
                b.add(b.text().add(g.get().blueprintI().tally(res).accepting()));
            b.NL();

            if (SETT.ROOMS().TRANSPORT.hasActive(res))
            {
                b.add(b.text().normalify2().add(¤Has));
            }
            else
            {
                b.error(¤HasNot);
            }
            b.NL();

            base.hoverInfoGet(text);
        }
    }
}