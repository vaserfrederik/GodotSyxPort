using System;
using snake2d.util.sprite;
using world.map.regions;

namespace game.faction.player.emmi
{
    public abstract class EmiTypeReg : EmiType<Region>
    {
        protected EmiTypeReg(SPRITE icon, string name, string desc) : base(icon, name, desc, WREGIONS.MAX, 1000)
        {
        }

        public override int Index(Region reg)
        {
            return reg.Index();
        }
    }
}