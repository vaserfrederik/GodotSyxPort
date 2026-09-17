using System;
using game.battle.div;
using init.constant;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using snake2d.util.datatypes;
using snake2d.util.misc;

namespace settlement.entity.humanoid.ai.battle
{
    public sealed class MarchSubCutTo : AISUB.Simple
    {
        public MarchSubCutTo() : base("MarchCutTo")
        {
        }

        private readonly AISUB inter = new AISUB.Simple("MarchCutToInter")
        {
            public override AISubActivation activate(Humanoid a, AIManager d)
            {
                return base.activate(a, d, AI.STATES.STOP.activate(a, d, 0));
            }

            protected override AISTATE resume(Humanoid a, AIManager d)
            {
                return null;
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.Event == HEvent.COLLISION_TILE)
                {
                    if (AI.modules().battle.tile.shouldattackTile(d, a, e.Tx, e.Ty))
                    {
                        d.interrupt(a, e);
                        d.overwrite(a, AI.modules().battle.tile.init(d, a, e.Tx, e.Ty));
                    }
                    else
                    {
                        d.interrupt(a, e);
                        d.overwrite(a, inter.activate(a, d));
                    }

                    return false;
                }

                return InterBattle.listener.event(a, d, e);
            }
        };

        private static readonly VectorImp vec = new VectorImp();
        private readonly int distFar = (int)((C.TILE_SIZE + C.TILE_SIZEH) * (C.TILE_SIZE + C.TILE_SIZEH));
        private readonly double distFarI = 1.0 / distFar;
        private readonly int distClose = (int)(C.TILE_SIZEH * C.TILE_SIZEH / 2);
        private readonly double distCloseI = 1.0 / distClose;

        protected override AISTATE resume(Humanoid a, AIManager d)
        {
            d.subByte++;
            Div div = a.division();
            if (div == null)
            {
                if (d.subByte <= 1)
                    return AI.STATES.STAND.activate(a, d, 0.05);
                return null;
            }

            if (!div.reporter.posHas(a))
            {
                if (d.subByte == 1)
                    return AI.STATES.STAND.activate(a, d, 0.05);
                return null;
            }
            COORDINATE dest = div.reporter.getPixel(a);

            if (BattleUtil.isInPosition(dest, a, d))
            {
                a.speed.magnitudeInit(0);
                if (d.subByte == 1)
                    return AI.STATES.STAND.activate(a, d, 0.05);
                return null;
            }

            double speed = div.settings().speed();

            int distX = dest.x() - a.physics.body().cX();
            int distY = dest.y() - a.physics.body().cY();
            double dist = distX * distX + distY * distY;

            if (dist > distFar)
                speed += a.speed.magintudeMax() * (dist - distFar) * distFarI;
            else if (dist < distClose)
            {
                speed *= dist * distCloseI;
            }

            speed = CLAMP.d(speed, C.TILE_SIZEH, a.speed.magintudeMax());

            AISTATE s = AI.STATES().MOVE_TO.move(a, d, dest.x(), dest.y(), 0.05, speed);

            if (dist < distFar)
            {
                DIR dir = div.position().dir(a.divSpot());
                if (dir != null)
                {
                    if (div.status().threatAt(dir, div))
                        a.speed.setDirCurrent(dir);
                    else if (div.status().isFighting())
                    {
                        a.speed.setDirCurrent(div.position().dir());
                    }
                }
                else
                {
                    a.speed.setDirCurrent(div.position().dir());
                }
            }
            a.speed.setDirCurrent(div.position().dir());

            return s;
        }

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.Event == HEvent.COLLISION_TILE)
            {
                if (AI.modules().battle.tile.shouldattackTile(d, a, e.Tx, e.Ty))
                {
                    d.overwrite(a, AI.modules().battle.tile.init(d, a, e.Tx, e.Ty));
                }
                else
                {
                    d.interrupt(a, e);
                    d.overwrite(a, inter.activate(a, d));
                }
                return false;
            }
            if (e.Event == HEvent.MEET_ENEMY)
            {
                if (a.speed.magnitudeRelative() > 0.4)
                {
                    Div div = a.division();
                    if (div != null)
                    {
                        if (div.reporter.posHas(a))
                        {
                            COORDINATE dest = div.reporter.getPixel(a);
                            double m = vec.set(a.physics.body(), dest.x(), dest.y());
                            if (m > 0 && vec.nX() * a.speed.nX() + vec.nY() * a.speed.nY() > 0.6)
                            {
                                d.overwrite(a, AI.STATES().MOVE_TO.move(a, d, dest.x(), dest.y(), 0.05, 0.7 * a.speed.magintudeMax()));
                                return false;
                            }
                        }
                    }
                }
            }
            return InterBattle.listener.event(a, d, e);
        }
    }
}