using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Room.Military.Supply
{
    using static Settlement.Room.Infra.Logistics.MoveDic.¤¤fetch;
    using static Settlement.Room.Infra.Logistics.MoveDic.¤¤fetchD;
    using static Settlement.Room.Infra.Logistics.MoveDic.¤¤fetching;

    using Init.Resources;
    using Init.Settings;
    using Init.Sprite.UI;
    using Settlement.Main;
    using Settlement.Room.Infra.Logistics;
    using Settlement.Room.Military.Supply;
    using Snake2D.Util.Color;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Gui;
    using Snake2D.Util.Gui.Clickable;
    using Snake2D.Util.Sets;
    using Snake2D.Util.Sprite.Text;
    using Util;

    public class Gui : Settlement.Room.Military.Supply.Gui
    {
        private static readonly Str ¤¤fetching = new Str("Fetching");
        private static readonly Str ¤¤livestock = new Str("Livestock");
        private static readonly Str ¤¤carts = new Str("Carts");
        private static readonly Str ¤¤needed = new Str("Needed");
        private static readonly Str ¤¤possible = new Str("Possible");
        private static readonly Str ¤¤possibleD = new Str("Not enough possible deliveries");
        private static readonly Str ¤¤closedD = new Str("Entry is closed");

        private int[] ready;
        private int livestock;
        private int carts;
        private int upI = -1;

        public Gui(Blueprint blueprint) : base(blueprint)
        {
            ready = new int[RESOURCES.ALL().Count];
        }

        protected override void AppendMain(GGrid icons, GGrid gridtext, GuiSection sExtra)
        {
            GuiSection s = new GuiSection();

            int k = 0;

            foreach (var res in AD.Supplies().Reses())
            {
                var c = new ClickableAbs(64, 40)
                {
                    Render = (r, ds, isActive, isSelected, isHovered) =>
                    {
                        double stored = 0;
                        double max = 0;

                        foreach (var s in AD.Supplies().Get(res))
                        {
                            max += s.TargetAmount(FACTIONS.Player());
                            stored += s.Current().Faction(FACTIONS.Player());
                        }

                        double d = stored / max;
                        stored += Blueprint.Tally.Amount.Total(res) + Blueprint.Tally.SpaceReserved.Total(res);
                        double d2 = stored / max;
                        if (max > 0)
                            GMeter.RenderDelta(r, d, d2, Body.X1() + 5, Body.X2() - 5, Body.Y1() + 5, Body.Y2() - 5, false, false);

                        res.Icon().RenderC(r, Body.CX(), Body.CY());
                        GButt.ButtPanel.RenderFrame(r, Body);
                    },
                    Sel = () =>
                    {
                        for (int i = 0; i < Blueprint.InstancesSize(); i++)
                        {
                            if (Blueprint.GetInstance(i).Allowed().Has(res))
                                return true;
                        }
                        return false;
                    },
                    Click = () =>
                    {
                        bool sel = Sel();
                        for (int i = 0; i < Blueprint.InstancesSize(); i++)
                        {
                            SupplyInstance ins = Blueprint.GetInstance(i);
                            if (sel == ins.Allowed().Has(res))
                            {
                                ins.AllowedToggle(res);
                                ins.Reset();
                            }
                        }
                    },
                    HoverInfoGet = (text) =>
                    {
                        var b = (GBox)text;
                        b.Title(res.Name);

                        HoverNeeded(b, res);

                        b.TextLL(¤¤underway);
                        b.Tab(7);
                        b.Add(GFORMAT.I(b.Text(), Blueprint.Tally.Amount.Total(res) + Blueprint.Tally.SpaceReserved.Total(res)));
                        b.NL();

                        if (S.Get().Developer)
                        {
                            foreach (var d in Blueprint.Tally.Datas)
                            {
                                b.TextLL(d.Name);
                                b.Tab(7);
                                b.Add(GFORMAT.I(b.Text(), d.Total(res)));
                                b.NL();
                            }
                            b.Add(UI.Icons().Question);
                            b.Add(GFORMAT.I(b.Text(), Blueprint.Cache.Deliverable(res)));
                        }
                    }
                };

                s.AddGrid(c, k++, 5, 0, 0);
            }

            gridtext.Add(s);
        }

        protected override void AppendMain(GGrid icons, GGrid gridtext, GuiSection sExtra)
        {
            GuiSection s = new GuiSection();

            int k = 0;

            foreach (var res in AD.Supplies().Reses())
            {
                var c = new ClickableAbs(64, 40)
                {
                    Render = (r, ds, isActive, isSelected, isHovered) =>
                    {
                        double stored = 0;
                        double max = 0;

                        foreach (var s in AD.Supplies().Get(res))
                        {
                            max += s.TargetAmount(FACTIONS.Player());
                            stored += s.Current().Faction(FACTIONS.Player());
                        }

                        double d = stored / max;
                        stored += Blueprint.Tally.Amount.Total(res) + Blueprint.Tally.SpaceReserved.Total(res);
                        double d2 = stored / max;
                        if (max > 0)
                            GMeter.RenderDelta(r, d, d2, Body.X1() + 5, Body.X2() - 5, Body.Y1() + 5, Body.Y2() - 5, false, false);

                        res.Icon().RenderC(r, Body.CX(), Body.CY());
                        GButt.ButtPanel.RenderFrame(r, Body);
                    },
                    Sel = () =>
                    {
                        for (int i = 0; i < Blueprint.InstancesSize(); i++)
                        {
                            if (Blueprint.GetInstance(i).Allowed().Has(res))
                                return true;
                        }
                        return false;
                    },
                    Click = () =>
                    {
                        bool sel = Sel();
                        for (int i = 0; i < Blueprint.InstancesSize(); i++)
                        {
                            SupplyInstance ins = Blueprint.GetInstance(i);
                            if (sel == ins.Allowed().Has(res))
                            {
                                ins.AllowedToggle(res);
                                ins.Reset();
                            }
                        }
                    },
                    HoverInfoGet = (text) =>
                    {
                        var b = (GBox)text;
                        b.Title(res.Name);

                        HoverNeeded(b, res);

                        b.TextLL(¤¤underway);
                        b.Tab(7);
                        b.Add(GFORMAT.I(b.Text(), Blueprint.Tally.Amount.Total(res) + Blueprint.Tally.SpaceReserved.Total(res)));
                        b.NL();

                        if (S.Get().Developer)
                        {
                            foreach (var d in Blueprint.Tally.Datas)
                            {
                                b.TextLL(d.Name);
                                b.Tab(7);
                                b.Add(GFORMAT.I(b.Text(), d.Total(res)));
                                b.NL();
                            }
                            b.Add(UI.Icons().Question);
                            b.Add(GFORMAT.I(b.Text(), Blueprint.Cache.Deliverable(res)));
                        }
                    }
                };

                s.AddGrid(c, k++, 5, 0, 0);
            }

            gridtext.Add(s);
        }

        private void Cache(SupplyInstance i)
        {
            if (upI == GAME.UpdateI())
                return;
            upI = GAME.UpdateI();
            ready.Fill(0);
            livestock = 0;
            carts = 0;

            for (int ji = 0; ji < i.Jobs.Count; ji++)
            {
                COORDINATE j = i.Jobs[ji];
                Crate cr = Crate.Get(j.X(), j.Y());
                carts++;
                if (cr.AnimalHas())
                    livestock++;
                RESOURCE res = Crate.Get(j.X(), j.Y()).RealResource();
                if (res != null)
                {
                    ready[res.Index()] += cr.ResAmount() * (cr.GoIsReady() == 0 ? 1 : 0);
                }
            }
        }

        private int Ready(RESOURCE res, SupplyInstance i)
        {
            Cache(i);
            return ready[res.Index()];
        }
    }
}