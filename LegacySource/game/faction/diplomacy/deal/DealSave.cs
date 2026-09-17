using System;
using System.Collections.Generic;

namespace Game.Faction.Diplomacy.Deal
{
    [Serializable]
    public sealed class DealSave : Serializable
    {
        private static readonly long SerialVersionUID = 1L;
        private readonly int fi;
        private readonly int fii;
        public readonly bool[] bools;
        public readonly Party player;
        public readonly Party npc;

        public DealSave(Deal deal)
        {
            fi = deal.npc.npc().index();
            fii = deal.npc.npc().iteration();
            bools = new bool[deal.bools.all().Count];
            for (int i = 0; i < bools.Length; i++)
            {
                bools[i] = deal.bools.all()[i].is();
            }
            player = new Party(deal.player);
            npc = new Party(deal.npc);
        }

        private static readonly string¤¤Faction = "The faction of this agreement does no longer exist.";
        private static readonly string¤¤You = "You currently do not have the means to fulfill this agreement. ({0})";
        private static readonly string¤¤Other = "The faction currently does not have the means to fulfill this agreement.";

        static DealSave()
        {
            D.ts(typeof(DealSave));
        }

        public string set(Deal deal)
        {
            FactionNPC npc = f();
            if (npc == null)
                return ¤¤Faction;
            deal.setFactionAndClear(npc);
            for (int i = 0; i < bools.Length; i++)
            {
                deal.bools.all()[i].set(bools[i]);
            }
            if (player.set(deal.player) != null)
                return Str.TMP.clear().add(¤¤You).insert(0, player.set(deal.player));
            if (this.npc.set(deal.npc) != null)
                return ¤¤Other;
            return null;
        }

        public FactionNPC f()
        {
            Faction f = FACTIONS.getByIndex(fi);
            if (f == null || !f.isActive() || !(f is FactionNPC))
                return null;
            FactionNPC npc = (FactionNPC)f;
            if (npc.iteration() != fii)
                return null;
            return npc;
        }

        [Serializable]
        public sealed class Party : Serializable
        {
            private static readonly long SerialVersionUID = 1L;
            public readonly int creditsP;
            public readonly int[] regsP;
            public int[] resP;

            public Party(DealParty p)
            {
                creditsP = p.credits.get();
                regsP = Alloc.ii(p.regs.all().Count);
                for (int i = 0; i < regsP.Length; i++)
                {
                    regsP[i] = p.regs.all()[i].is() ? p.regs.all()[i].reg().index() : -1;
                }
                resP = Alloc.ii(TR.ALL().Count);
                for (int i = 0; i < resP.Length; i++)
                {
                    resP[i] = p.resources.get(TR.ALL()[i]);
                }
            }

            private object readResolve()
            {
                int[] resP = Alloc.ii(TR.ALL().Count);
                for (int i = 0; i < this.resP.Length; i++)
                {
                    TRADABLE res = TR.MAP().loader().get(i);
                    if (res != null)
                        resP[res.index()] = this.resP[i];
                }
                this.resP = resP;
                return this;
            }

            public string set(DealParty p)
            {
                p.credits.set(0);
                foreach (TRADABLE res in TR.ALL())
                    p.resources.set(res, 0);
                p.regs.clear();

                if (creditsP > p.credits.max())
                    return Dic.¤¤Curr;
                p.credits.set(creditsP);
                foreach (int i in regsP)
                {
                    if (i != -1)
                    {
                        Region reg = WORLD.REGIONS().getByIndex(i);
                        if (!reg.active() || reg.faction() != p.f())
                            return Dic.¤¤Region;
                        p.regs.add(reg);
                    }
                }
                for (int i = 0; i < resP.Length; i++)
                {
                    if (resP[i] > 0 && resP[i] > p.resources.max(TR.ALL()[i]))
                        return TR.ALL()[i].names;
                    p.resources.set(TR.ALL()[i], resP[i]);
                }

                return null;
            }
        }
    }
}