using System;
using System.Collections.Generic;
using game.faction.diplomacy.deal;
using init.sprite.UI;
using init.trade;
using snake2d;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using view.main;
using world.map.regions;

namespace view.ui.diplomacy
{
    public class UIDealList : GuiSection
    {
        private static readonly string ¤¤YouGet = "You get";
        private static readonly string ¤¤FactionGets = "{0} Gets";

        private readonly object[] all = new object[256];
        private int npcStart;
        private int dealsCount;

        private readonly Deal deal;

        static UIDealList()
        {
            D.ts(typeof(UIDealList));
        }

        public UIDealList(Deal deal, int height)
        {
            this.deal = deal;

            var bu = new GTableBuilder
            {
                NrOfEntries = () => dealsCount
            };

            bu.Column(null, 340, new GRowBuilder
            {
                Build = ier => new Row(ier)
            });

            Add(bu.CreateHeight(height, false));
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            int i = 0;
            foreach (var bo in deal.bools.All())
            {
                if (bo.Is())
                {
                    i = Set(bo, i, 0);
                }
            }

            npcStart = Fill(deal.npc, i);
            dealsCount = Fill(deal.player, npcStart);

            base.Render(r, ds);
        }

        private int Fill(DealParty dp, int i)
        {
            int start = i;
            i = i + 1;

            if (dp.credits.Get() != 0)
            {
                i = Set(dp.credits, i, start);
            }

            foreach (var reg in dp.regs.All())
            {
                if (reg.Is())
                    i = Set(reg.Reg(), i, start);
            }

            foreach (var res in TR.ALL())
            {
                if (dp.resources.Get(res) > 0)
                {
                    i = Set(res, i, start);
                }
            }

            if (i != start + 1)
            {
                Set(null, start, start);
                return i;
            }

            return start;
        }

        private int Set(object o, int i, int start)
        {
            if (i >= all.Length || i - start > all.Length - 1)
                return i;
            all[i] = o;

            return i + 1;
        }

        private class Row : CLICKABLE.ClickableAbs
        {
            private readonly GETTER<int> ier;
            private readonly GText header = new GText(UI.FONT().H2, 24).Lablify();
            private readonly GText name = new GText(UI.FONT().H2, 24).Normalify();

            public Row(GETTER<int> ier)
            {
                base(340, 32);
                this.ier = ier;
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                object o = all[ier.Get()];
                DealParty p = ier.Get() >= npcStart ? deal.player : deal.npc;

                if (o == null)
                {
                    header.Clear();
                    if (p == deal.player)
                    {
                        header.Add(¤¤FactionGets);
                        header.Insert(0, deal.npc.npc().name);
                        header.AdjustWidth();
                    }
                    else
                    {
                        header.Set(¤¤YouGet);
                    }

                    header.RenderCXY2(r, body.cX(), body.y2() - 4);
                }
                else
                {
                    GButt.ButtPanel.RenderBG(r, isActive, isSelected, isHovered, body);
                    GButt.ButtPanel.RenderFrame(r, body);
                    if (o is DealBool)
                    {
                        DealBool b = (DealBool)o;
                        Render(r, b.icon, b.info.name);
                    }
                    else if (o is INT)
                    {
                        name.Clear().Add(p.credits.Get());
                        Render(r, UI.icons().s.money);
                    }
                    else if (o is Region)
                    {
                        Region rr = (Region)o;
                        if (rr.faction() != null)
                            Render(r, rr.faction().banner().MEDIUM, rr.info.name());
                    }
                    else if (o is TRADABLE)
                    {
                        TRADABLE rr = (TRADABLE)o;
                        name.Clear().Add(p.resources.Get(rr));
                        Render(r, rr.icon());
                    }
                    else
                    {
                        throw new RuntimeException("" + o);
                    }
                }
            }

            private void Render(SPRITE_RENDERER r, SPRITE icon)
            {
                icon.RenderCY(r, body().x1() + 8, body().cY());
                name.RenderCY(r, body().x1() + 40, body().cY());
            }

            private void Render(SPRITE_RENDERER r, SPRITE icon, string name)
            {
                this.name.Clear().Add(name);
                this.Render(r, icon);
            }

            protected override void ClickA()
            {
                object o = all[ier.Get()];
                DealParty p = ier.Get() >= npcStart ? deal.player : deal.npc;

                if (o == null)
                {
                    return;
                }
                else
                {
                    if (o is DealBool)
                    {
                        DealBool b = (DealBool)o;
                        b.Set(false);
                    }
                    else if (o is INT)
                    {
                        p.credits.Set(0);
                    }
                    else if (o is Region)
                    {
                        p.regs.Select((Region)o, false);
                    }
                    else if (o is TRADABLE)
                    {
                        p.resources.Set((TRADABLE)o, 0);
                    }
                }
                base.ClickA();
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                object o = all[ier.Get()];
                DealParty p = ier.Get() >= npcStart ? deal.player : deal.npc;
                if (o == null)
                    return;
                GBox b = (GBox)text;
                int value = 0;
                if (o is DealBool)
                {
                    DealBool vv = (DealBool)o;
                    b.Title(vv.info.name);
                    b.Text(vv.info.desc);
                    value = (int)vv.value();
                }
                else if (o is INT)
                {
                    value = p.credits.Get();
                }
                else if (o is Region)
                {
                    Region reg = (Region)o;
                    VIEW.world().UI.regions.Hover(reg, b);
                    value = (int)p.regs.Value(reg);
                }
                else if (o is TRADABLE)
                {
                    TRADABLE res = (TRADABLE)o;
                    b.Title(res.names);
                    value = (int)p.ValueResource(res, p.resources.Get(res));
                }
                b.NL();
                b.Add(UI.icons().s.money);
                b.Add(GFORMAT.i(b.text(), value));
                base.HoverInfoGet(text);
            }
        }
    }
}