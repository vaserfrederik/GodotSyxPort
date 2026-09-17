using System;
using System.Collections.Generic;

using Game.Faction;
using Game.Faction.Diplomacy.Deal;
using Game.Faction.Trade;
using Init.Sprite.UI;
using Init.Trade;
using Snake2D;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui;
using Util.Colors;
using Util.Data;
using Util.Gui.Common;
using Util.Gui.Misc;
using Util.Info;
using Util.Text;
using View.Main;
using World.Map.Regions;
using World.Region;

namespace View.Ui.Diplomacy
{
    public sealed class UIDealConfig : GuiSection
    {
        private const string ¤¤offer = "¤Offer";
        private const string ¤¤demand = "¤Demand";
        private const string ¤¤item = "¤/Item";

        private static readonly int BW = 345;
        private static readonly int BH = 34;

        public UIDealConfig(Deal deal, int height)
        {
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            foreach (DealBool b in deal.bools.All())
                rows.Add(new Bool(b));

            rows.Add(h(¤¤offer));
            rows.Add(new Regionlist(deal, deal.Player, deal.Npc));
            rows.Add(new Reslist(deal, deal.Player));
            rows.Add(new Sum(deal.Player));

            rows.Add(h(¤¤demand));
            rows.Add(new Regionlist(deal, deal.Npc, deal.Npc));
            rows.Add(new Reslist(deal, deal.Npc));
            rows.Add(new Sum(deal.Npc));

            Add(new GScrollRows(rows, height).View());
        }

        private RENDEROBJ h(string t)
        {
            return new RENDEROBJ.RenderImp(BW, BH)
            {
                Render = (SPRITE_RENDERER r, float ds) =>
                {
                    GCOLOR.T().H1.Bind();
                    UI.FONT().H2.RenderCX(r, Body().CX(), Body().Y2() - UI.FONT().H2.Height() - 4, t);
                }
            };
        }

        private class Bool : GButt.ButtPanel
        {
            private readonly DealBool _bool;

            public Bool(DealBool b) : base(b.Info.Name)
            {
                _bool = b;
                Icon(b.Icon);
                Body().SetDim(BW, BH);
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                isActive = _bool.Problem() == null;
                isSelected = _bool.Is();
                base.Render(r, ds, isActive, isSelected, isHovered);
            }

            protected override void ClickA()
            {
                if (_bool.Problem() == null)
                    _bool.Toggle();
                base.ClickA();
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                _bool.Hover(b);

                string p = _bool.Problem();
                if (p != null)
                    b.Error(p);
            }
        }

        private class Regionlist : GButt.ButtPanel
        {
            private readonly GuiSection _pop;
            private readonly DealParty _p;

            public Regionlist(Deal deal, DealParty p, DealParty ff) : base(Dic.¤¤Regions)
            {
                _p = p;
                Icon(UI.Icons().S.World);
                Body().SetDim(BW, BH);

                GETTER<Faction> gg = new GETTER<Faction>
                {
                    Get = () => _p.F()
                };

                _pop = new UIPickerRegion(gg, 400)
                {
                    Toggle = (Region reg) =>
                    {
                        _p.Regs.Select(reg, !_p.Regs.Selected(reg));
                    },
                    Active = (Region reg) =>
                    {
                        return _p.Regs.SelecteCan(reg);
                    },
                    Selected = (Region reg) =>
                    {
                        return _p.Regs.Selected(reg);
                    },
                    HoverInfo = (GBox b, Region reg) =>
                    {
                        b.Add(UI.Icons().S.Money);
                        b.Add(GFORMAT.I(b.Text(), (long)_p.Regs.Value(reg)));
                        b.NL(8);
                        base.HoverInfo(b, reg);
                    }
                };
            }

            protected override void ClickA()
            {
                VIEW.Inters().Popup.Show(_pop, this);
            }

            protected override void RenAction()
            {
                ActiveSet(_p.F().Realm().Regions() > 1);
            }
        }

        private class Reslist : GButt.ButtPanel
        {
            private readonly GuiSection _pop;

            public Reslist(Deal deal, DealParty p) : base(Dic.¤¤Resource)
            {
                Icon(UI.Icons().S.Storage);
                Body().SetDim(BW, BH);
                _pop = new UIPicker<TRADABLE>(_p.Resources, 16, TR.ALL())
                {
                    AddToRow = (GuiSection row, GETTER<TRADABLE> g) =>
                    {
                        GuiSection s = new GuiSection()
                        {
                            HoverInfoSelf = (GUI_BOX box) =>
                            {
                                GBox b = (GBox)box;
                                b.Title(g.Get().Name);
                                b.TextLL(_p.F() == FACTIONS.Player() ? Dic.¤¤BuyPrice : Dic.¤¤SellPrice);
                                b.Tab(6);
                                b.Add(GFORMAT.I(b.Text(), _p.F() == FACTIONS.Player() ? _p.Npc().Buyer(g.Get()).AddPrice(1) : _p.Npc().Seller(g.Get()).RemovePrice(1)));
                                b.NL();
                                b.TextLL(Dic.¤¤Tariff);
                                b.Tab(6);
                                b.Add(GFORMAT.I(b.Text(), TradeManager.TotalFee(_p.F() == FACTIONS.Player() ? _p.F() : _p.Npc(), _p.F() == FACTIONS.Player() ? _p.Npc() : _p.F(), RD.DIST().Distance(_p.Npc()), g.Get(), 1)));
                                b.NL();
                                base.HoverInfoSelf(box);
                            }
                        };

                        s.AddRelBody(8, DIR.E, new GStat()
                        {
                            Update = (GText text) =>
                            {
                                text.Add('/');
                                GFORMAT.I(text, _p.Resources.Max(g.Get()));
                            }
                        });
                        s.Body().IncrW(64);
                        s.AddRelBody(8, DIR.E, new GStat()
                        {
                            Update = (GText text) =>
                            {
                                GFORMAT.I(text, _p.ValueResource(g.Get(), _p.Resources.Get(g.Get())));
                            }
                        }.Hh(UI.Icons().S.Money));
                        s.AddRelBody(128, DIR.E, new GStat()
                        {
                            Update = (GText text) =>
                            {
                                int am = _p.Resources.Get(g.Get());
                                if (am == 0)
                                    am = 1;
                                GFORMAT.I(text, _p.ValueResource(g.Get(), am) / am);
                                text.S();
                                text.Add(¤¤item);
                            },
                            HoverInfoGet = (GBox b) =>
                            {

                            }
                        }.R(DIR.E));
                        row.AddRelBody(8, DIR.E, s);
                    }
                };
            }

            protected override void ClickA()
            {
                VIEW.Inters().Popup.Show(_pop, this);
            }

            protected override void RenAction()
            {
                ActiveSet(true);
            }
        }

        private class Sum : GInputInt
        {
            public Sum(DealParty party) : base(party.Credits, true, true)
            {
                Body().SetHeight(BH);
                AddRelBody(4, DIR.W, UI.Icons().S.Money);
                AddRelBody(8, DIR.E, new GStat()
                {
                    Update = (GText text) =>
                    {
                        text.Add('(');
                        GFORMAT.I(text, party.Credits.Max());
                        text.Add(')');
                    }
                });
                Body().SetWidth(BW);
            }
        }
    }
}