using System;
using System.Collections.Generic;
using System.Linq;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.util;
using settlement.main;
using settlement.room.main;
using settlement.room.service.arena;
using settlement.room.service.arena.grand;
using settlement.room.service.arena.pit;
using settlement.stats;
using util.text;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    class ExecuteArena : AIPlanGladiator
    {
        private static readonly string ¤¤name = "¤Fighting to death in the arena.";

        static ExecuteArena()
        {
            D.ts(typeof(ExecuteArena));
        }

        public ExecuteArena() : base("PRisArena", true, ¤¤name) { }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            d.planByte3 = 0;

            NEEDS.TYPES().HUNGER.stat().fixMax(a.indu());
            STATS.NEEDS().EXPOSURE.fix(a.indu());

            foreach (ROOM_ARENA arena in SETT.ROOMS().GARENAS)
            {
                if (arena.punishEnabled().is(a.race()))
                {
                    RoomInstance ins = arena.work.reserveDeath(a.tc());
                    if (ins != null)
                    {
                        d.planTile.set(arena.work.gladiatorGetSpot(ins));
                        return base.init(a, d);
                    }
                }

                d.planByte3++;
            }

            foreach (ROOM_FIGHTPIT arena in SETT.ROOMS().FIGHTPITS)
            {
                if (arena.punishEnabled().is(a.race()))
                {
                    RoomInstance ins = arena.work.reserveDeath(a.tc());
                    if (ins != null)
                    {
                        d.planTile.set(arena.work.gladiatorGetSpot(ins));
                        return base.init(a, d);
                    }
                }

                d.planByte3++;
            }

            return null;
        }

        protected override RoomArenaWork w(Humanoid a, AIManager d)
        {
            if (d.planByte3 >= SETT.ROOMS().GARENAS.size())
                return SETT.ROOMS().FIGHTPITS.get(d.planByte3 - SETT.ROOMS().GARENAS.size()).work;

            return SETT.ROOMS().GARENAS.get(d.planByte3).work;
        }

        protected override AISubActivation resume(Humanoid a, AIManager d)
        {
            AISubActivation sub = base.resume(a, d);
            if (sub == null)
            {
                w(a, d).unreserveDeath(d.planTile.x(), d.planTile.y());
                d.planTile.set(-1, -1);
                RoomInstance ins = w(a, d).reserveDeath(a.tc());
                if (ins != null)
                {
                    d.planTile.set(w(a, d).gladiatorGetSpot(ins));
                    return base.init(a, d);
                }
            }

            return sub;
        }

        protected override void cancel(Humanoid a, AIManager d)
        {
            RoomArenaWork w = w(a, d);
            if (w != null)
                w.unreserveDeath(d.planTile.x(), d.planTile.y());
            d.planTile.set(-1, -1);
            base.cancel(a, d);
        }

        protected override void remove(Humanoid a, AIManager d)
        {
            base.remove(a, d);
        }
    }
}