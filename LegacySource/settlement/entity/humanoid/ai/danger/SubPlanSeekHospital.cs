using System;
using System.Collections.Generic;
using game.audio;
using init.constant;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.health.hospital;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.danger
{
    class SubPlanSeekHospital
    {
        private readonly Resumer start;

        private readonly ROOM_HOSPITAL b = SETT.ROOMS().HOSPITAL;
        public readonly SoundRace sound = AUDIO.race("SICK_MOAN");

        public AISubActivation Init(Humanoid a, AIManager d)
        {
            if (STATS.SERVICE().hospital.accessRequest(a))
                return start.Set(a, d);
            return null;
        }

        public SubPlanSeekHospital(AIPLAN.PLANRES p)
        {
            Resumer rest = p.NewResumer(b.service().verb)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    d.planByte1 = (byte)(4 + RND.rInt(4));
                    double liveChance = b.recoverRate(d.planTile.x(), d.planTile.y());
                    if (STATS.NEEDS().INJURIES.inDanger(a.indu()))
                    {
                        if (!STATS.NEEDS().INJURIES.willDie(a.indu(), liveChance))
                        {
                            STATS.NEEDS().INJURIES.setNonDanger(a.indu());
                        }
                    }

                    return AI.SUBS().LAY.activateTime(a, d, 15);
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    FSERVICE s = b.service().service(d.planTile.x(), d.planTile.y());
                    if (s == null)
                    {
                        return null;
                    }
                    sound.rnd(a);
                    double liveChance = b.recoverRate(d.planTile.x(), d.planTile.y());

                    if (STATS.DISEASE().status(a.indu()).active && !STATS.DISEASE().diseaseIsDone(a, liveChance))
                        return AI.SUBS().LAY.activateTime(a, d, 60);
                    return Fix(a, d);
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                    FSERVICE s = b.service().service(d.planTile.x(), d.planTile.y());
                    if (s != null && s.findableReservedIs())
                        s.consume();
                }
            };

            start = p.NewResumer(b.service().verb)
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    AISubActivation s = AI.SUBS().walkTo.service(a, d, b.service().finder, b.service().radius());
                    if (s != null)
                    {
                        d.planTile.set(d.path.destX(), d.path.destY());
                    }
                    return s;
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    FSERVICE s = b.service().service(d.planTile.x(), d.planTile.y());
                    if (s == null || !s.findableReservedIs())
                    {
                        return null;
                    }
                    int x = d.planTile.x() * C.TILE_SIZE + C.TILE_SIZEH;
                    int y = d.planTile.y() * C.TILE_SIZE + C.TILE_SIZEH;
                    DIR dir = SETT.ROOMS().HOSPITAL.layCoo(d.planTile.x(), d.planTile.y());
                    x += dir.x() * (C.TILE_SIZEH - 2);
                    y += dir.y() * (C.TILE_SIZEH - 2);
                    a.physics.body().moveC(x, y);
                    a.speed.setDirCurrent(dir);
                    return rest.Set(a, d);
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {

                }
            };
        }

        private AISubActivation Fix(Humanoid a, AIManager d)
        {
            FSERVICE s = b.service().service(d.planTile.x(), d.planTile.y());

            double liveChance = b.recoverRate(d.planTile.x(), d.planTile.y());
            if ((STATS.DISEASE().status(a.indu()).active) && STATS.DISEASE().shouldDie(a) && RND.rFloat() > liveChance)
            {
                AIManager.dead = CAUSE_LEAVES.DISEASE();
                AIManager.deadGore = false;
                return AI.SUBS().LAY.activate(a, d);
            }

            if (STATS.NEEDS().INJURIES.willDie(a.indu(), liveChance))
            {
                HumanoidResource.dead = a.lastLeaveCause() != null ? a.lastLeaveCause() : CAUSE_LEAVES.getAccident();
                AIManager.deadGore = false;
                return AI.SUBS().LAY.activate(a, d);
            }

            if (s != null && s.findableReservedIs())
            {
                s.consume();
                foreach (DIR dir in DIR.ORTHO)
                {
                    if (!SETT.PATH().solidity.is(a.tc(), dir))
                    {
                        int x = (a.tc().x() + dir.x()) * C.TILE_SIZE + C.TILE_SIZEH;
                        int y = (a.tc().y() + dir.y()) * C.TILE_SIZE + C.TILE_SIZEH;
                        a.physics.body().moveC(x, y);
                    }
                }
            }

            STATS.DISEASE().cure(a.indu(), true);
            STATS.NEEDS().INJURIES.setNonDanger(a.indu());

            return null;
        }
    }
}