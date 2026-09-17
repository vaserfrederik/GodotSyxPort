using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Faction.Diplomacy.Deal
{
    public sealed class Deal
    {
        public readonly DealParty Player;
        public readonly DealParty NPC;
        public readonly DealBools Bools;
        public bool ClearDeal;

        public Deal()
        {
            DealRegs.RegData data = new DealRegs.RegData();
            Player = new DealParty(this, data);
            NPC = new DealParty(this, data);
            Bools = new DealBools(Player, NPC);
        }

        public void SetFactionAndClear(FactionNPC faction)
        {
            SetFactionAndClear(faction, true);
        }

        public void SetFactionAndClear(FactionNPC faction, bool clearDeal)
        {
            SetFactionAndClear(faction, clearDeal, Debugger.Dummy);
        }

        public void SetFactionAndClear(FactionNPC faction, bool clearDeal, Debugger d)
        {
            this.ClearDeal = clearDeal;
            Player.Init(FACTIONS.Player(), faction, faction);
            NPC.Init(faction, FACTIONS.Player(), faction);
            Bools.Init(true, clearDeal, d);
            DupI = -1;
        }

        public bool CanBeAccepted()
        {
            return HasDeal() && ((int)ValueCredits() >= 0 || Can);
        }

        public double Execute(bool changeOpinion)
        {
            double v = OpinionChange();
            Player.Execute();
            NPC.Execute();
            Bools.Execute();

            if (changeOpinion && Player.F() == FACTIONS.Player())
                ROPINION.GIFTS().MakeDeal(NPC.NPC(), v);
            if (NPC.NPC().IsActive())
                SetFactionAndClear(NPC.NPC(), ClearDeal);

            return v;
        }

        int DupI = -1;
        private int CValue;
        private bool Can = false;

        public double ValueCredits()
        {
            Can = false;
            DupI = VIEW.RI();

            CValue = (int)Bools.Value();
            CValue += Player.Value();
            CValue -= NPC.Value();

            return CValue;
        }

        public bool HasDeal()
        {
            foreach (DealBool b in Bools.All())
                if (b.Is())
                    return true;
            return Has(NPC) || Has(Player);
        }

        public bool Has(DealParty p)
        {
            if (p.Credits.Get() != 0)
                return true;
            foreach (DealReg r in Player.Regs.All())
            {
                if (r.Is())
                    return true;
            }
            foreach (TRADABLE r in TR.ALL())
                if (p.Resources.Get(r) != 0)
                    return true;

            return false;
        }

        public double OpinionChange()
        {
            return OpinionChangeD();
        }

        public double OpinionChangeD()
        {
            double c = 25 * ValueCredits();
            c /= NPC.SelfWorth();

            return c;
        }

        public double GetWorthOfOpinion(double opinion)
        {
            return opinion * NPC.SelfWorth() / 25.0;
        }

        public double Betrayal()
        {
            return Bools.Betrayal();
        }

        public void HoverBetrayal(GBox b)
        {
            Bools.BetrayalHover(b);
        }
    }
}