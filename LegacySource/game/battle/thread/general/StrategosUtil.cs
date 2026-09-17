using game;
using game.battle;
using game.battle.formation;
using settlement.main;
using settlement.room.main.throne;
using snake2d;
using snake2d.util.datatypes;

namespace game.battle.thread.general
{
    public sealed class StrategosUtil
    {
        public readonly PathUtilOnline flooder;
        public readonly DivDeployer deployer;
        public readonly UtilDeployer divDeployer;

        public StrategosUtil()
        {
            flooder = new PathUtilOnline(SETT.TWIDTH);
            deployer = new DivDeployer(flooder);
            divDeployer = new UtilDeployer(this);
        }

        public COORDINATE GetDestCoo()
        {
            return THRONE.Coo();
        }

        public Army GetArmy()
        {
            return GAME.ARMIES().Enemy();
        }
    }
}