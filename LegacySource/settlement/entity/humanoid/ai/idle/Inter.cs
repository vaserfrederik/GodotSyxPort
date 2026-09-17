using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.idle;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.idle
{
    class Inter : AIEventListeners.Default
    {
        private static readonly CharSequence ¤¤MoveAway = "¤making way";

        static Inter()
        {
            D.ts(typeof(Inter));
        }

        private readonly AISUB sub = new AISUB.Simple("IdleMoveAway", ¤¤MoveAway)
        {
            protected override AISTATE Resume(Humanoid a, AIManager d)
            {
                d.subByte++;
                switch (d.subByte)
                {
                    case 1: return AI.STATES().STOP.Instant(a, d);
                    case 2: return AI.STATES().WALK2.CTile(a, d);
                    case 3: return AI.STATES().STAND.Activate(a, d, 0.5f);
                    case 4:
                        if (d.subPathByte >= DIR.ALL.Length || d.subPathByte < 0)
                            return AI.STATES().STOP.Activate(a, d);
                        a.speed.SetDirCurrent(DIR.ALL[d.subPathByte]);
                        return AI.STATES().WALK2.DirTile(a, d, a.speed.Dir());
                    case 5: a.speed.MagnitudeInit(0); return AI.STATES().STOP.Activate(a, d);
                    case 6: return AI.STATES().STAND.Activate(a, d, 3f + RND.rFloat(3f));
                    case 7: return AI.STATES().STOP.Instant(a, d);
                    case 8: return null;
                }
                return null;
            }
        };

        private readonly AISUB subMoveaway = new AISUB.Simple("IdleMoveAway2", ¤¤MoveAway)
        {
            protected override AISTATE Resume(Humanoid a, AIManager d)
            {
                d.subByte++;
                switch (d.subByte)
                {
                    case 1:
                        PATH().finders.getOutofWay.Request(a, d.path);
                        return AI.STATES().STOP.Instant(a, d);
                    case 2: return AI.STATES().STAND.Activate(a, d, 1.5f);
                    case 3:
                        if (PATH().finders.getOutofWay.CheckAndSetRequest(a.tc().x(), a.tc().y(), d.path))
                        {
                            if (d.path.IsSuccess())
                            {
                                d.Overwrite(a, AI.SUBS().walkTo.Path(a, d));
                            }
                        }
                    case 4: return AI.STATES().STOP.Activate(a, d);
                    case 5: return AI.STATES().STAND.Activate(a, d, 3f + RND.rFloat(3f));
                }
                return null;
            }
        };

        public override bool Event(Humanoid a, AIManager ai, HEventData e)
        {
            if (e.Event == HEvent.MEET_HARMLESS)
            {
                COORDINATE c = a.physics.tileC();
                DIR d = e.Other.speed.Dir();
                int dd = RND.rBoolean() ? 1 : -1;
                d = d.Next(-2 * dd);
                for (int i = 0; i < 5; i++)
                {
                    if (PATH().coster.player.GetCost(c.x(), c.y(), c.x() + d.x(), c.y() + d.y()) > 0 && PATH().finders.IsGoodTileToStandOn(c.x() + d.x(), c.y() + d.y(), a))
                    {
                        a.speed.SetDirCurrent(d);

                        ai.Interrupt(a, e);
                        ai.subPathByte = (byte)d.Id();
                        ai.Overwrite(a, sub.Activate(a, ai));

                        return false;
                    }
                    d = d.Next(-dd);
                }
                ai.Interrupt(a, e);
                ai.Overwrite(a, subMoveaway.Activate(a, ai));
            }
            return base.Event(a, ai, e);
        }
    }
}