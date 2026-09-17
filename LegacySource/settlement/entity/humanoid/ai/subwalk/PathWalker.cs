using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.subwalk
{
    abstract class PathWalker : Resumable
    {
        protected readonly AISTATES.WALK_DEST state;

        PathWalker(string key, AISTATES.WALK_DEST state, string name) : base(key + "Walker", name)
        {
            this.state = state;
        }

        protected PathWalker(string key, string name) : this(key + "Walker", AI.STATES().WALK2, name)
        {
        }

        public override AISTATE init(Humanoid a, AIManager d)
        {
            if (a.speed.magnitude() > 0)
            {
                return stop.set(a, d);
            }
            if (!d.path.isSuccessful())
            {
                GAME.Notify("here!" + d.path);
                return failure.set(a, d);
            }

            d.subPathByte2 = 0;
            AISTATE st = next.set(a, d);
            if (st == null)
                return stop.set(a, d);
            return st;
        }

        private readonly Resumer next = new Resumer()
        {
            res = (a, d) =>
            {
                if (d.path.isDest())
                {
                    a.speed.turn2(a, (d.path.destX() << C.T_SCROLL) + C.TILE_SIZEH, (d.path.destY() << C.T_SCROLL) + C.TILE_SIZEH);
                    return setLast(a, d);
                }

                d.path.setNext();
                if (!d.path.isSuccessful())
                {
                    GAME.Notify("no " + a.physics.tileC() + " " + d.path.destX() + " " + d.path.destY());
                    return failure.set(a, d);
                }
                d.subPathByte2++;
                if (d.subPathByte2 > 4)
                {
                    d.subPathByte2 = 0;
                    if (hasFailed(a, d))
                    {
                        a.speed.magnitudeInit(0);
                        return failure.set(a, d);
                    }
                }

                return state.path(a, d);
            },
            success = (a, d) => true,
            can = (a, d) => abort(a, d),
            setAction = (a, d) => res(a, d)
        };

        readonly Resumer moveToEdge = new Resumer()
        {
            res = (a, d) =>
            {
                int dy = d.path.destY() * C.TILE_SIZE + C.TILE_SIZEH - a.body().cY();
                int dx = d.path.destX() * C.TILE_SIZE + C.TILE_SIZEH - a.body().cX();
                a.speed.setDirCurrent(DIR.get(dx, dy));
                return wait.set(a, d);
            },
            success = (a, d) => true,
            setAction = (a, d) =>
            {
                int x2 = d.path.getSettCX();
                int y2 = d.path.getSettCY();
                int dd = ((C.TILE_SIZE - a.body().width()) - 2) / 2;
                if (d.path.isFull())
                {
                    if (dd > 3)
                        dd = 3;
                    x2 += RND.rInt0(dd);
                    y2 += RND.rInt0(dd);
                }
                else
                {
                    int dy = d.path.destY() - d.path.y();
                    int dx = d.path.destX() - d.path.x();
                    x2 += dx * dd;
                    y2 += dy * dd;
                }
                return AI.STATES().WALK2.free(a, d, x2, y2);
            },
            can = (a, d) => abort(a, d)
        };

        readonly Resumer wait = new Resumer()
        {
            res = (a, d) =>
            {
                if (hasFailed(a, d) || !d.path.isSuccessful())
                {
                    return failure.set(a, d);
                }
                arrive(a, d);
                return null;
            },
            success = (a, d) => true,
            setAction = (a, d) =>
            {
                a.speed.magnitudeInit(0);
                return AI.STATES().STAND.activate(a, d, 0.5f);
            },
            can = (a, d) => abort(a, d)
        };

        private readonly Resumer stop = new Resumer()
        {
            res = (a, d) =>
            {
                if (hasFailed(a, d))
                {
                    return failure.set(a, d);
                }
                return next.set(a, d);
            },
            success = (a, d) => true,
            setAction = (a, d) => AI.STATES().STOP.activate(a, d),
            can = (a, d) => abort(a, d)
        };

        readonly Resumer failure = new Resumer()
        {
            res = (a, d) => null,
            success = (a, d) => false,
            setAction = (a, d) =>
            {
                a.speed.magnitudeInit(0);
                abort(a, d);
                return AI.STATES().STAND.activate(a, d, 0.1f);
            }
        };

        public override void cancel(Humanoid a, AIManager d)
        {
            base.cancel(a, d);
        }

        public override AISTATE resumeInterrupted(Humanoid a, AIManager d, HEvent event)
        {
            d.subPathByte2 = 0;

            if (event == HEvent.COLLISION_TILE)
                return null;

            if (d.path().isFull() && !SETT.PATH().connectivity.is(d.path().destX(), d.path().destY()))
                return null;

            if (hasFailed(a, d) || !d.path.resume(a.physics.tileC(), a.body()))
                return null;
            return init(a, d);
        }

        public override AISTATE resume(Humanoid a, AIManager d)
        {
            AISTATE s = base.resume(a, d);
            return s;
        }

        protected AISTATE setLast(Humanoid a, AIManager d)
        {
            return moveToEdge.set(a, d);
        }

        protected abstract bool hasFailed(Humanoid a, AIManager d);
        protected abstract void abort(Humanoid a, AIManager d);
        protected abstract void arrive(Humanoid a, AIManager d);
    }
}