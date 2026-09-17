using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.idle
{
    class SubStand : AISUB.Simple
    {
        private readonly Inter inter = new Inter();
        private readonly AIPLAN plan;

        public SubStand(AIPLAN plan, string key) : base(key)
        {
            this.plan = plan;
        }

        public override AISTATE resume(Humanoid a, AIManager d)
        {
            if (!a.speed.IsZero())
                return AI.STATES().STOP.Instant(a, d);

            switch (d.subByte)
            {
                case 0:
                    d.subByte = 1;
                    if (AI.STATES().WALK2.CTileNeeds(a, d))
                        return AI.STATES().WALK2.CTile(a, d);
                    return AI.STATES().STOP.Instant(a, d);
                case 1:
                    d.subByte = 2;
                    return AI.STATES().STAND.Activate(a, d, 1.0f + RND.rFloat(2.0f));
                case 2:
                    d.subByte = 100;
                    a.speed.SetRaw(a.speed.Dir().Next(1 * (RND.rBoolean() ? 1 : -1)), 0);
                    return AI.STATES().STAND.Activate(a, d, 1.0f + RND.rFloat(2.0f));
                default:
                    return null;
            }
        }

        public override double Poll(Humanoid a, AIManager d, HPollData e)
        {
            return inter.Poll(a, d, e);
        }

        public override bool Event(Humanoid a, AIManager ai, HEventData e)
        {
            if (e.Event == HEvent.MEET_HARMLESS)
            {
                if (e.Other is Humanoid)
                {
                    Humanoid o = (Humanoid)e.Other;
                    AIManager oai = (AIManager)o.ai();
                    if (oai.Plan() == plan)
                    {
                        return false;
                    }
                }
                COORDINATE c = a.physics.TileC();
                DIR d = e.Other.speed.Dir();
                int dd = RND.rBoolean() ? 1 : -1;
                d = d.Next(-2 * dd);
                for (int i = 0; i < 5; i++)
                {
                    if (PATH().coster.player.GetCost(c.X, c.Y, c.X + d.X, c.Y + d.Y) > 0 && PATH().finders.IsGoodTileToStandOn(c.X + d.X, c.Y + d.Y, a) && !SETT.ROOMS().map.Is(c, d))
                    {
                        a.speed.SetDirCurrent(d);
                        ai.subPathByte = (byte)d.Id();
                        ai.Interrupt(a, e);
                        ai.Overwrite(a, sub.Activate(a, ai));

                        return false;
                    }
                    d = d.Next(-dd);
                }
                ai.Interrupt(a, e);
                ai.Overwrite(a, subMoveaway.Activate(a, ai));
            }
            return AIEventListeners.def.Event(a, ai, e);
        }

        private readonly AISUB sub = new AISUB.Simple("standMakeWay", "making way")
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
                        if (d.subPathByte >= DIR.ALL.size || d.subPathByte < 0)
                            return AI.STATES().STOP.Activate(a, d);
                        a.speed.SetDirCurrent(DIR.ALL.Get(d.subPathByte));
                        return AI.STATES().WALK2.DirTile(a, d, a.speed.Dir());
                    case 5: a.speed.MagnitudeInit(0); return AI.STATES().STOP.Activate(a, d);
                    case 6: return AI.STATES().STAND.Activate(a, d, 15f + RND.rFloat(5f));
                    case 7: return AI.STATES().STOP.Instant(a, d);
                    case 8: return null;
                }
                return null;
            }
        };

        private readonly AISUB subMoveaway = new AISUB.Simple("standMoveAway", "making way")
        {
            protected override AISTATE Resume(Humanoid a, AIManager d)
            {
                d.subByte++;
                switch (d.subByte)
                {
                    case 1:
                        PATH().finders.getOutofWay.Request(a, d.path);
                        return AI.STATES().STOP.Instant(a, d);
                    case 2: return AI.STATES().STAND.Activate(a, d, 1f);
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
    }
}