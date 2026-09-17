using game.battle.util;
using init.constant;

namespace game.battle.formation
{
    public class DivPositionCopyable : DivPositionImp, Copyable<DivPositionCopyable>
    {
        public DivPositionCopyable() : base(Config.battle().MEN_PER_DIVISION)
        {
        }

        public void Copy(DivPositionCopyable pos)
        {
            CopyPosition(pos);
        }
    }
}