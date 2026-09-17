using game.save;
using init.paths;
using world.battle.spec;

namespace game.battle.state
{
    public abstract class BattleStateExiter
    {
        public abstract void AfterExit(BattleStateResult res);

        public void Exit(BATTLE_RESULT res, int plosses, int elosses)
        {
            BattleStateResult r = new BattleStateResult(res, elosses, plosses);
            new GameLoader(PATHS.Local().Save().Get("__beforeBattle"))
            {
                public override void DoAfterSet()
                {
                    AfterExit(r);
                }
            }.Set();
        }
    }
}