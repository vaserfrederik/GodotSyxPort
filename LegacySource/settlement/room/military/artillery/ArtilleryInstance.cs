using System;
using System.Threading;
using game;
using game.battle;
using init.constant;
using settlement.entity.humanoid;
using settlement.main;
using settlement.path.finders;
using settlement.room.main;
using settlement.stats;
using settlement.thing.projectiles;
using snake2d;
using util.gui.misc;
using util.rendering;

namespace settlement.room.military.artillery
{
    public sealed class ArtilleryInstance : RoomInstance, FINDABLE_MANNING_INSTANCE
    {
        private static readonly long serialVersionUID = 1L;

        public bool hovered, selected;

        private bool enemy;
        bool invisible;
        private readonly byte dir;
        private byte dirCurrent;
        byte men;

        private bool mustered;
        private bool fireAtWill;
        private readonly Coo cTarget = new Coo(-1, -1);
        private short dTarget = -1;
        private bool bombard = false;

        private volatile bool trajLock;
        private readonly Trajectory traj = new Trajectory();
        volatile bool hasTrajectory = false;
        private float progress;
        private float skill;
        private float skillI;
        bool isLoaded;
        private bool targetIsUserSet = false;
        private static readonly Trajectory trajTmp = new Trajectory();

        ArtilleryInstance(ROOM_ARTILLERY b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            dir = (byte)(SETT.ROOMS().fData.item.get(mX(), mY()).rotation * 2);
            dirCurrent = dir;
            Activate();
        }

        public override ROOM_ARTILLERY blueprintI()
        {
            return (ROOM_ARTILLERY)blueprint();
        }

        void Work(double amount, Humanoid hu)
        {
            invisible = false;

            if (!NeedsWork())
                return;

            double skill = (1.0 - 0.75 * GetDegrade()) * blueprintI().bonus().Get(hu.indu()) / blueprintI().bonus().Max(typeof(Induvidual));
            amount /= 6.0 * blueprintI().projectile.reloadSeconds(skill);

            if (hasTrajectory)
            {
                DIR d = DIR.Get(traj.vx(), traj.vy());
                if (d != dirCurrent())
                {
                    progress += amount * 8;
                    if (progress >= 1)
                    {
                        progress -= 1;
                        if (d == dirCurrent().next(-2))
                            dirCurrent = (byte)dirCurrent().next(-1).id();
                        else if (d == dirCurrent().next(2))
                            dirCurrent = (byte)dirCurrent().next(1).id();
                        else
                            dirCurrent = (byte)d.id();
                    }
                    return;
                }

                if (isLoaded && (TargetCooGet() != null || TargetDivGet() != null))
                {
                    isLoaded = false;
                    int h = SETT.TERRAIN().Get(body().cX(), body().cY()).heightEnt(body().cX(), body().cY()) * C.TILE_SIZE;
                    h += Trajectory.RELEASE_HEIGHT;
                    GetTrajectory(trajTmp);
                    int fx = body().x1() * C.TILE_SIZE + body().width() * C.TILE_SIZE / 2;
                    int fy = body().y1() * C.TILE_SIZE + body().height() * C.TILE_SIZE / 2;
                    fx += C.TILE_SIZE * dir().x();
                    fy += C.TILE_SIZE * dir().y();
                    double @ref = Math.Clamp(this.skill / skillI, 0, 1);
                    skillI = 0;
                    this.skill = 0;
                    SETT.PROJS().launch(fx, fy, h, trajTmp, blueprintI().projectile, 1.0 - blueprintI().projectile.accuracy(@ref), @ref, null);
                    return;
                }
            }

            if (!isLoaded)
            {
                progress += amount;
                this.skill += skill;
                skillI++;
                if (progress >= 1)
                {
                    progress -= 1;
                    isLoaded = true;
                }
            }
        }

        public void SetVisible()
        {
            invisible = false;
        }

        double Progress => progress;

        bool HasTrajectory => hasTrajectory;

        bool IsLoaded => isLoaded;

        float Skill => skill;

        float SkillI => skillI;

        bool TargetIsUserSet => targetIsUserSet;

        bool Bombard => bombard;

        public override FINDABLE_MANNING GetManning(int tx, int ty)
        {
            return blueprintI().service.Get(tx, ty);
        }

        public double MenMustering()
        {
            return men / 6.0;
        }

        public override void DestroyTile(int tx, int ty)
        {
            if (enemy)
            {
                SETT.THINGS().gore.debris(Centre().x(), Centre().y(), 0, 0);
                Remove(tx, ty, false, this, true).Clear();
            }
            else
                base.DestroyTile(tx, ty);
        }
    }
}