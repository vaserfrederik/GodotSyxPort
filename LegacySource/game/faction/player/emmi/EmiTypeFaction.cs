using game.faction;
using game.faction.npc;
using snake2d.util.sprite;

namespace game.faction.player.emmi
{
    public abstract class EmiTypeFaction : EmiType<FactionNPC>
    {
        public EmiTypeFaction(SPRITE icon, string name, string desc) : base(icon, name, desc, FACTIONS.MAX(), 1000)
        {
        }

        public override int Index(FactionNPC t)
        {
            return t.Index();
        }
    }
}