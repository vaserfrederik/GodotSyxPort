using System;
using System.Collections.Generic;
using System.IO;

namespace World.Army.AI
{
    using Game.Faction;
    using Game.Faction.Diplomacy;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;
    using World.Entity.Army;

    public sealed class WArmyAI
    {
        private readonly War war = new War();
        private readonly Rebel rebel = new Rebel();
        private bool upAll = false;
        private readonly ArrayList<Faction> fas = new ArrayList<Faction>(FACTIONS.MAX());

        public WArmyAI()
        {
            new DIP.DipActivityListener()
            {
                Change = (faction, other, old, nn) =>
                {
                    if (nn == DIP.WAR() || old == nn)
                    {
                        if (!fas.Contains(faction))
                            fas.Add(faction);
                        if (!fas.Contains(other))
                            fas.Add(other);
                    }
                }
            };
        }

        public SAVABLE Saver => new SAVABLE
        {
            Save = file => {
                war.Save(file);
                rebel.Save(file);
            },
            Load = file => {
                war.Load(file);
                rebel.Load(file);
            },
            Clear = () => {
                war.Clear();
                rebel.Clear();
            }
        };

        public void Update(WArmy a)
        {
            rebel.UpdateRebel(a);
        }

        public void Update(double ds)
        {
            foreach (Faction f in fas)
            {
                war.PlanForWar(f);
            }
            fas.ClearSloppy();
            war.Update(ds);
        }

        public void Init(Faction f)
        {
            war.Init(f);
        }
    }
}