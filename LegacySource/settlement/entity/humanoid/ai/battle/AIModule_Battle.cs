using System;
using game.audio;
using game.battle.div;
using init.sprite.UI;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.stats;
using util.text;

namespace settlement.entity.humanoid.ai.battle
{
    public sealed class AIModule_Battle : AIModule
    {
        private readonly AIPLAN march = new MarchPlan("BattleMarch");
        private readonly ManPlan planMan = new ManPlan("BattleMan");
        public readonly PlanEscape escape = new PlanEscape();
        public readonly MarchSubCutTo subCutTo = new MarchSubCutTo();
        public readonly SubFight fight = new SubFight("battleFight");
        public readonly MarchSoftCollision subSoft = new MarchSoftCollision();
        public readonly PlanAttackTile tile = new PlanAttackTile("BAttleTile");
        private readonly AIPLAN dessert = new PlanRout("BattleRout");

        public readonly SoundRace soundSword = AUDIO.race("SWORD");

        private static readonly string ¤¤name = "Battle";
        private static readonly string ¤¤desc = "Joining of mustered divisions and fighting.";
        static AIModule_Battle()
        {
            D.ts(typeof(AIModule_Battle));
        }

        public AIModule_Battle() : base(UI.icons().s.sword, ¤¤name, ¤¤desc)
        {
        }

        public override AiPlanActivation GetPlan(Humanoid a, AIManager d)
        {
            if (STATS.BATTLE().ROUTING.indu().Get(a.indu()) == 1)
            {
                return dessert.Activate(a, d);
            }

            if (a.indu().hostile())
            {
                AiPlanActivation p = march.Activate(a, d);
                if (p == null)
                    return planMan.Activate(a, d);
                return p;
            }
            else
            {
                Div div = a.division();
                if (div != null)
                {
                    AiPlanActivation p = march.Activate(a, d);
                    if (p != null)
                        return p;
                }
                return planMan.Activate(a, d);
            }
        }

        public AISubActivation Fight(Humanoid a, AIManager d, ENTITY h)
        {
            d.OtherEntitySet((Humanoid)h);
            return fight.Activate(a, d);
        }

        public bool BreakTile(Humanoid a, AIManager d, int tx, int ty)
        {
            if (tile.ShouldattackTile(d, a, tx, ty))
            {
                tile.Init(d, a, tx, ty);
                d.Overwrite(a, tile);
                return true;
            }
            return false;
        }

        public AIPLAN Interrupt(Humanoid a, AIManager d)
        {
            return march;
        }

        protected override void Update(Humanoid a, AIManager ds, bool newDay, int byteDelta, int updateI)
        {
            if (a.division() != null)
            {
                if (a.division().info.men() < a.division().men())
                {
                    STATS.BATTLE().DIV.Set(a, null);
                }
            }
        }

        public override int GetPriority(Humanoid a, AIManager ds)
        {
            if (STATS.BATTLE().ROUTING.indu().Get(a.indu()) == 1)
                return 11;

            if (a.indu().hostile())
                return 11;

            Div d = a.division();
            if (d != null && d.settings().mustering() && d.deployed() > 0)
            {
                return 9;
            }

            if (planMan.ShouldMan(a, ds))
            {
                return 8;
            }

            return 0;
        }
    }
}