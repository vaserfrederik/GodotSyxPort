using System;
using System.Collections.Generic;
using System.Linq;

namespace View.UI.Goods
{
    using Game.Faction;
    using Game.Faction.Diplomacy;
    using Game.Faction.NPC;
    using Game.Faction.Royalty.Opinion;
    using Init.Sprite.UI;
    using Snake2D;
    using Snake2D.Util.Gui;
    using Snake2D.Util.Gui.Clickable;
    using Snake2D.Util.Gui.Renderable;
    using Util.Data;
    using Util.Gui.Misc;
    using Util.Gui.Table;
    using Util.Info;
    using Util.Text;
    using View.Main;
    using World.Region;

    public abstract class UIGoodsTraders : GuiSection
    {
        private GText buttPrice = new GText(UI.FONT().S, 64);
        private FactionNPC[] facs = new FactionNPC[FACTIONS.MAX()];
        private int max = 0;

        private static readonly CharSequence ¤¤TradeYes = "¤You have a trade agreement with this faction, and trade is possible.";
        private static readonly CharSequence ¤¤TradeNo = "¤You do not have a trade agreement with this faction. Trade is not possible.";
        private static readonly CharSequence ¤¤Click = "¤Click to go to the diplomacy screen for this faction.";

        static
        {
            D.ts(typeof(UIGoodsTraders));
        }

        public UIGoodsTraders(int hi)
        {
            var bu = new GTableBuilder
            {
                NrOFEntries = () => max
            };

            bu.Column(null, new Butt(null).Body().Width(), new GRowBuilder
            {
                Build = ier => new Butt(ier)
            });

            Add(bu.Create(hi, false));
        }

        private class Butt : ClickableAbs
        {
            private readonly GETTER<int> ier;

            public Butt(GETTER<int> ier)
            {
                Body.SetDim(96, 32);
                this.ier = ier;
            }

            protected override void Render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                var f = facs[ier.Get()];

                isSelected |= DIP.Get(f).Trades;

                GButt.ButtPanel.RenderBG(r, isActive, isSelected, isHovered, Body);
                GButt.ButtPanel.RenderFrame(r, Body);

                f.Banner().MEDIUM.RenderCY(r, Body.X1() + 4, Body.CY());

                buttPrice.Clear();
                GFORMAT.I(buttPrice, Price(f));
                buttPrice.AdjustWidth();

                buttPrice.RenderCY(r, Body.X1() + 4 + 24 + 4, Body.CY());
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                var f = facs[ier.Get()];
                VIEW.World().UI.Factions.Hover(text, f);
                var b = (GBox)text;

                b.Sep();

                b.TextLL(Dic.¤¤Price);
                b.Tab(6).Add(GFORMAT.I(b.Text(), Price(f)));
                b.NL(8);
                if (DIP.Get(f).Trades)
                    b.Text(¤¤TradeYes);
                else
                    b.Text(¤¤TradeNo);
                b.NL(4);
                b.Text(¤¤Click);
            }

            protected override void ClickA()
            {
                var f = facs[ier.Get()];
                if (!DIP.Get(f).Trades && ROPINION.Get(f) >= DIP.TRADE().OpinionNeeded)
                    VIEW.World().UI.Factions.OpenTrade(f);
                else
                    VIEW.World().UI.Factions.OpenDip(f);
                VIEW.UI().Manager.Close();
            }
        }

        private readonly IComparer<FactionNPC> comp = new Comparator<FactionNPC>();

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            max = 0;

            foreach (var f in RD.DIST().Neighs())
            {
                if (Price(f) > 0)
                {
                    facs[max++] = f;
                }
            }

            Array.Sort(facs, 0, max, comp);

            base.Render(r, ds);
        }

        protected abstract int Price(FactionNPC f);

        protected abstract int SortValue(FactionNPC f);
    }

    internal class Comparator<FactionNPC> : IComparer<FactionNPC>
    {
        public int Compare(FactionNPC o1, FactionNPC o2)
        {
            var uiGoodsTraders = (UIGoodsTraders)typeof(UIGoodsTraders).GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
            return uiGoodsTraders.SortValue(o1) - uiGoodsTraders.SortValue(o2);
        }
    }
}