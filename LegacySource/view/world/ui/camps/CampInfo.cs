using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snake2D;
using Snake2D.Util.Gui;
using Util.Gui.Misc;
using Util.Info;
using Util.Text;
using World;
using World.Entity.Haven;

namespace View.World.UI.Camps
{
    internal sealed class CampInfo : GuiSection
    {
        private readonly WHavenType type;

        private static readonly CharSequence ¤¤notFull = "¤Fulfill this species requirements to unlock the help of the havens that are within your realm.";
        private static readonly CharSequence ¤¤full = "¤The requirements have been met and the havens on your lands are at your service.";
        private static readonly CharSequence ¤¤Replenish = "Replenish";

        private static readonly CharSequence ¤¤unlocked = "This haven is on your lands and at your service.";
        private static readonly CharSequence ¤¤onLands = "This haven is on your lands, but the requirements are not met for them to join your cause.";
        private static readonly CharSequence ¤¤distant = "This haven is not on your lands and can not serve you.";

        private static readonly CharSequence ¤¤max = "Haven Max Population";
        private static readonly CharSequence ¤¤unlockedAm = "Unlocked Population";

        static CampInfo()
        {
            D.ts(typeof(CampInfo));
        }

        public CampInfo(WHavenType type)
        {
            this.type = type;
            Add(type.race.appearance().iconBig, 0, 0);
            Add(new GHeader(type.race.info.names, UI.FONT().S), GetLastX2() + 8, 0);

            Add(new GStat
            {
                Update = text =>
                {
                    GFORMAT.iofkInv(text, WORLD.camps().current(FACTIONS.player(), type), WORLD.camps().max(FACTIONS.player(), type));
                }
            }.Hh(SPRITES.icons().s.human), GetLastX1(), GetLastY2());

            AddRightC(64, new GStat
            {
                Update = text =>
                {
                    GFORMAT.f0(text, WORLD.camps().replenishPerDay(FACTIONS.player(), type));
                }
            }.Hh(SPRITES.icons().s.clock));

            AddRightC(64, new GStat
            {
                Update = text =>
                {
                    GFORMAT.i(text, WORLD.camps().camps(FACTIONS.player(), type));
                }
            }.Hh(SPRITES.icons().s.house));

            Body().IncrW(48);

            AddRelBody(4, DIR.S, new SPRITE.Imp(Body().Width(), 12)
            {
                Render = (r, X1, X2, Y1, Y2) =>
                {
                    double d = type.reqsFrom.Progress(null);
                    if (d >= 1)
                    {
                        GMeter.Render(r, GMeter.C_BLUE, type.amount(), X1, X2, Y1, Y2);
                    }
                    else
                    {
                        GMeter.Render(r, GMeter.C_ORANGE, d, X1, X2, Y1, Y2);
                    }
                }
            });

            Pad(16, 8);
        }

        public override void HoverInfoGet(GUI_BOX text)
        {
            GBox b = (GBox)text;
            b.Title(type.race.info.names);
            b.Text(type.race.info.desc);

            b.NL(4);
            if (type.reqsFrom.Passes(null))
                b.Add(b.Text().Normalify2().Add(¤¤full));
            else
                b.Add(b.Text().Warnify().Add(¤¤notFull));

            b.NL(8);
            b.TextLL(Dic.¤¤havens);
            b.Tab(6);
            b.Add(GFORMAT.i(b.Text(), WORLD.camps().camps(FACTIONS.player(), type)));

            b.NL();
            b.TextLL(¤¤max);
            b.Tab(6);
            b.Add(GFORMAT.i(b.Text(), WORLD.camps().max(FACTIONS.player(), type)));

            b.NL();
            b.TextLL(¤¤unlockedAm);
            b.Tab(6);
            b.Add(GFORMAT.i(b.Text(), WORLD.camps().current(FACTIONS.player(), type)));

            b.NL();
            b.TextLL(¤¤Replenish);
            b.Tab(6);
            b.Add(GFORMAT.f0(b.Text(), WORLD.camps().replenishPerDay(FACTIONS.player(), type)));

            b.NL(8);
            type.reqsFrom.Hover(text, null);
            b.NL(8);

            b.Sep();
            b.TextLL(Dic.¤¤Progress);
            b.NL();
            type.HoverProgress(b);

            base.HoverInfoGet(text);
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            GButt.ButtPanel.RenderBG(r, true, false, HoveredIs(), Body());
            base.Render(r, ds);
            GButt.ButtPanel.RenderFrame(r, Body());
        }

        public static void Hover(GUI_BOX box, WHaven ins)
        {
            GBox b = (GBox)box;
            b.Title(ins.name);
            b.NL();
            b.TextL(ins.type().race.info.names);
            b.Tab(5);
            b.Add(GFORMAT.i(b.Text(), ins.pop()));
            b.NL(8);

            if (ins.faction() != FACTIONS.player())
                b.Add(b.Text().Warnify().Add(¤¤distant));
            else if (ins.type().reqsFrom.Passes(null))
                b.Add(b.Text().Normalify2().Add(¤¤unlocked));
            else
                b.Add(b.Text().Warnify().Add(¤¤onLands));
        }
    }
}