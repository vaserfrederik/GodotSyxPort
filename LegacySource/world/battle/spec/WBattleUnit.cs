using snake2d.util.gui;
using snake2d.util.sprite;

namespace world.battle.spec
{
    public interface WBattleUnit
    {
        public CharSequence Name { get; }
        public int Men { get; }
        public int Losses { get; }
        public int LossesRetreat { get; }
        public SPRITE Icon { get; }
        public void Hover(GUI_BOX box);
        public double Defences { get; }
    }
}