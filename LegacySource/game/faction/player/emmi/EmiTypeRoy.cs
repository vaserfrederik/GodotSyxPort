using System;
using System.Linq;

namespace Game.Faction.Player.Emmi
{
    public abstract class EmiTypeRoy : EmiType<Royalty>
    {
        private readonly int[] ftot;

        public EmiTypeRoy(SPRITE icon, string name, string desc) : base(icon, name, desc, FACTIONS.MAX() * NPCCourt.MAX, 1000)
        {
            ftot = new int[FACTIONS.MAX()];
        }

        public int Total(FactionNPC f)
        {
            return ftot[f.Index()];
        }

        protected override void Count(int index, int am)
        {
            ftot[index / NPCCourt.MAX] += am;
            base.Count(index, am);
        }

        protected override void Clear()
        {
            ftot.Fill(0);
            base.Clear();
        }

        protected override int Index(Royalty t)
        {
            return t.Court.Faction.Index() * NPCCourt.MAX + t.SuccessionI();
        }

        void Clear(FactionNPC f)
        {
            int k = f.Index() * NPCCourt.MAX;
            for (int i = 0; i < NPCCourt.MAX; i++)
            {
                Set(k + i, 0);
            }
        }
    }
}