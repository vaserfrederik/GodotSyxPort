using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.HEvent;
using settlement.entity.humanoid.HPoll;
using settlement.entity.humanoid.ai.battle;

namespace settlement.entity.humanoid.ai.battle
{
    public class InterBattle
    {
        public static HEventListener listener = new HEventListener()
        {
            public bool Event(Humanoid a, AIManager d, HEventData e)
            {
                switch (e.eventType)
                {
                    case HEvent.HEventType.MEET_HARMLESS:
                        return false;
                    case HEvent.HEventType.COLLISION_SOFT:
                        d.Interrupt(a, e);
                        d.Overwrite(a, AI.Modules().Battle.SubSoft.InitReady(d, a, e.Other, e.NorX, e.NorY, e.FacingDot, e.Momentum));
                        break;
                    case HEvent.HEventType.MEET_ENEMY:
                        d.Interrupt(a, e);
                        AI.Modules().Battle.SoundSword.Rnd(a);
                        d.Overwrite(a, AI.Modules().Battle.SubSoft.InitReady(d, a, e.Other, e.NorX, e.NorY, e.FacingDot, e.Momentum));
                        break;

                    case HEvent.HEventType.CHECK_MORALE:
                        Div div = a.Division();
                        if (div == null)
                        {
                            return AIEventListeners.Def.Event(a, d, e);
                        }
                        else
                        {
                            if (div.Settings().Mustering() && div.Morale() <= 0)
                            {
                                d.Overwrite(a, AI.Modules().Battle.Dessert);
                            }
                        }
                        break;
                    case HEvent.HEventType.ALERT_DANGER:
                        break;
                    case HEvent.HEventType.COLLISION_TILE:
                        if (AI.Modules().Battle.Tile.ShouldAttackTile(d, a, e.Tx, e.Ty))
                        {
                            d.Overwrite(a, AI.Modules().Battle.Tile.Init(d, a, e.Tx, e.Ty));
                            break;
                        }
                        else
                        {
                            d.Interrupt(a, e);
                            d.Overwrite(a, AI.Subs().Stand.ActivateTime(a, d, 1));
                        }
                        return true;
                    case HEvent.HEventType.NOTIFY_CRIME:
                        break;
                    default:
                        return AIEventListeners.Def.Event(a, d, e);
                }
                return false;
            }

            public double Poll(Humanoid a, AIManager d, HPollData e)
            {
                if (e.Type == HPoll.HPollType.DEFENCE_SKILL)
                {
                    return GAME.Battle().Fight.ValueDefenceSkill(a, e.FacingDot, e.Adx, e.Ady);
                }
                if (e.Type == HPoll.HPollType.PARRY_SKILL)
                {
                    return GAME.Battle().Fight.ValueParrySkill(a, e.FacingDot, e.Adx, e.Ady);
                }
                return AIEventListeners.Def.Poll(a, d, e);
            }
        };

        public static double PollReady(Humanoid a, AIManager d, HPollData e)
        {
            if (e.Type == HPoll.HPollType.IMPACT_DAMAGE)
            {
                if (e.IsEnemy)
                {
                    GAME.Battle().Fight.SetImpactDamage(a, e.Colli, e.Damage);
                }
                else
                {
                    e.Damage.DamageTileStrength = 0;
                }
                return 0;
            }
            if (e.Type == HPoll.HPollType.DEFENCE_SKILL)
            {
                return GAME.Battle().Fight.ValueDefenceSkill(a, e.FacingDot, e.Adx, e.Ady);
            }
            if (e.Type == HPoll.HPollType.PARRY_SKILL)
            {
                return GAME.Battle().Fight.ValueParrySkill(a, e.FacingDot, e.Adx, e.Ady);
            }

            return AIEventListeners.Def.Poll(a, d, e);
        }
    }
}