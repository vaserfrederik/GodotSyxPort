using game.faction;
using world.map.regions;

namespace world.battle.spec
{
    public abstract class WBattleSiege : WBattleSpec
    {
        public Region besiged;
        public double fortifications;

        public override void engage()
        {
            throw new System.RuntimeException();
        }

        public abstract class Result
        {
            public Region besiged;

            public abstract void occupy(double devastation, double death, int[] enslave, int[] resources);
            public abstract void abandon(double devastation, double death, int[] enslave, int[] resources);
            public abstract void puppet(double devastation, double death, int[] enslave, int[] resources);

            public static bool canPuppet()
            {
                return FACTIONS.canActivateNext();
            }
        }
    }
}