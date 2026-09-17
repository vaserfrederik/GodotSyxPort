using System;

namespace World.Battle.Spec
{
    using Init.Race;
    using Init.Resources;
    using Snake2D.Util.File;

    public abstract class WBattleResult
    {
        public WBattleSide Player;
        public WBattleSide Enemy;
        public BATTLE_RESULT Result;
        public int[] CapturedRaces = Alloc.Ii(RACES.All().Size());
        public int[] LostResources = Alloc.Ii(RESOURCES.ALL().Size());

        public abstract void Accept(int[] enslave, int[] resources);
    }
}