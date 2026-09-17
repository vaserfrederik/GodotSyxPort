using game.faction;
using game.faction.diplomacy.deal;
using game.faction.npc;
using game.faction.royalty;
using init.constant;
using init.trade;
using snake2d.util.gui;
using util.data.GETTER;
using view.interrupter;
using view.main;
using world;

namespace view.world.ui.faction
{
    public sealed class UIFactions
    {
        private readonly Deal deal = new Deal();

        private readonly GETTER_IMP<FactionNPC> getter = new GETTER_IMP<FactionNPC>()
        {
            public override void Set(FactionNPC t)
            {
                base.Set(t);
                if (Get() != null)
                    deal.SetFactionAndClear(t, true);
            }
        };

        private readonly Hoverer hov = new Hoverer();
        private readonly UIFactionList list = new UIFactionList(ISidePanel.HEIGHT);
        private readonly UIFaction detail = new UIFaction(getter, deal, C.MIN_WIDTH - list.Section().Body().Width() - 16, ISidePanel.HEIGHT);

        public UIFactions()
        {
        }

        public void Open(FactionNPC r)
        {
            VIEW.World().Activate();
            getter.Set(r);

            VIEW.World().Panels.Add(list, true);
            if (r != null)
            {
                VIEW.World().Panels.Add(detail, false);
                deal.SetFactionAndClear(getter.Get(), true);
                VIEW.World().Window.CentererTile.Set(r.Cx(), r.Cy());
            }
        }

        public bool OpenIs()
        {
            return VIEW.World().Panels.Added(list);
        }

        public bool OpenIs(Faction f)
        {
            return VIEW.World().Panels.Added(detail) && detail.F() == f;
        }

        public bool DiploIs(Faction f)
        {
            return VIEW.World().Panels.Added(detail) && detail.F() == f && detail.DipIS();
        }

        public void OpenPeace(FactionNPC other)
        {
            deal.SetFactionAndClear(other, true);
            Open(other);
            DealDrawfter.DraftPeace(deal, other, true);
            detail.Dip();
        }

        public void OpenTrade(FactionNPC other)
        {
            Open(other);
            deal.SetFactionAndClear(other, true);
            deal.bools.TRADE.SetOn();
            DealDrawfter.Draft(deal, true, true);
            detail.Dip();
        }

        public void OpenBuy(FactionNPC other, TRADABLE res)
        {
            Open(other);
            deal.SetFactionAndClear(other, true);
            deal.npc.resources.Set(res, 1);
            detail.Dip();
        }

        public void OpenSell(FactionNPC other, TRADABLE res)
        {
            Open(other);
            deal.SetFactionAndClear(other, true);
            if (deal.player.resources.Max(res) > 0)
                deal.player.resources.Set(res, 1);
            detail.Dip();
        }

        public void OpenDip(FactionNPC other)
        {
            Open(other);
            deal.SetFactionAndClear(other, true);
            detail.Dip();
        }

        public void Hover(GUI_BOX b, Faction r)
        {
            hov.Hover(b, r);
        }

        public void Hover(int tx, int ty)
        {
            Faction f = WORLD.REGIONS().Faction.Get(tx, ty);
            list.Hover(f);
        }

        public void Hover(GUI_BOX b, Royalty r)
        {
            Court.Hover(b, r);
        }
    }
}