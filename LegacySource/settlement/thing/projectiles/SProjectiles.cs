using System;
using System.IO;
using game.battle;
using game.debug;
using init.constant;
using settlement.entity;
using settlement.main;
using settlement.stats.equip;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using util.rendering;
using util.text;

namespace settlement.thing.projectiles
{
    public class SProjectiles : SettResource
    {
        private readonly Map map;
        private readonly PData data;
        private readonly Updater updater;
        private readonly PRenderer ren;

        public static readonly CharSequence ¤¤OUT_OF_RANGE = "¤Out of range!";
        public static readonly CharSequence ¤¤FRIENDLIES = "¤Ally subjects are in the trajectory and might get hit!";
        public static readonly CharSequence ¤¤TERRAIN = "¤Trajectory blocked by terrain";

        static SProjectiles()
        {
            D.ts(typeof(SProjectiles));
        }

        public SProjectiles() : base("PROJECTILES", true)
        {
            new Test();
        }

        protected override void load(FileGetter file) throws IOException
        {
            map.Clear();
            data.saver.load(file);
        }

        public override void save(FilePutter file)
        {
            data.saver.save(file);
        }

        protected override void clear()
        {
            map.Clear();
            data.saver.clear();
        }

        public void launchDummy(int x, int y, int height, Trajectory t, Projectile type, double ref, ENTITY e)
        {
            int i = data.create(x, y, height, t.vx(), t.vy(), t.vz(), type, ref, e);
            if (i != -1)
                data.live(i, false);
        }

        public void launch(int x, int y, int height, Trajectory t, Projectile type, double ran, double ref, ENTITY e)
        {
            data.create(x, y, height, t.vx() * RND.rFloat1(ran), t.vy() * RND.rFloat1(ran), t.vz() * RND.rFloat1(ran), type, ref, e);
        }

        public override void update(double ds, Profiler profiler)
        {
            for (int i = 0; i < data.last(); i++)
            {
                updater.update(i, ds);
            }
        }

        public void renderAbove(Renderer r, ShadowBatch s, float ds, int zoomout, RenderData renData)
        {
            ren.renderAbove(r, s, ds, zoomout, renData);
        }

        private static readonly Trajectory traj = new Trajectory();

        public static CharSequence problem(Div dd, Div target)
        {
            int tx = target.reporter.body().cX();
            int ty = target.reporter.body().cY();
            return problem(traj, dd, tx, ty);
        }

        public static CharSequence problem(Trajectory work, Div dd, int destX, int destY)
        {
            int startX = dd.reporter.body().cX();
            int startY = dd.reporter.body().cY();
            int fx = startX >> C.T_SCROLL;
            int fy = startY >> C.T_SCROLL;
            if (!SETT.IN_BOUNDS(fx, fy))
                return Dic.¤¤Problem;

            int tx = destX >> C.T_SCROLL;
            int ty = destY >> C.T_SCROLL;
            if (!SETT.IN_BOUNDS(tx, ty))
                return Dic.¤¤Problem;

            int h = SETT.TERRAIN().get(fx, fy).heightEnt(fx, fy) * C.TILE_SIZE + Trajectory.RELEASE_HEIGHT;
            h -= SETT.TERRAIN().get(tx, ty).heightEnt(tx, ty) * C.TILE_SIZE + Trajectory.HIT_HEIGHT / 2;

            EquipRange am = dd.settings().ammo();
            if (am == null)
                return ¤¤OUT_OF_RANGE;

            double ref = dd.settings().ammoRef();
            double speed = am.projectile.velocity(ref);

            if (speed <= 0)
                return Dic.¤¤Problem;

            CharSequence problem = ¤¤OUT_OF_RANGE;

            double angle = am.projectile.maxAngle(ref);

            if (work.calcLow(h, startX, startY, destX, destY, angle, speed))
            {
                for (int di = 0; di < DIR.NORTHO.size(); di++)
                {
                    DIR d = DIR.ORTHO.get(di);
                    int x = startX + d.x() * dd.reporter.body().width() / 2;
                    int y = startY + d.y() * dd.reporter.body().height() / 2;
                    problem = trajectoryProblem(dd.army(), work, x, y);
                    if (problem == null)
                        return null;
                }
            }
            else if (work.calcHigh(h, startX, startY, destX, destY, angle, speed))
            {
                for (int di = 0; di < DIR.NORTHO.size(); di++)
                {
                    DIR d = DIR.ORTHO.get(di);
                    int x = startX + d.x() * dd.reporter.body().width() / 2;
                    int y = startY + d.y() * dd.reporter.body().height() / 2;
                    problem = trajectoryProblem(dd.army(), work, x, y);
                    if (problem == null)
                        return null;
                }
            }

            return problem;
        }

        public static CharSequence problem(Army a, Trajectory work, int startX, int startY, int destX, int destY, double angle, double speed)
        {
            int fx = startX >> C.T_SCROLL;
            int fy = startY >> C.T_SCROLL;
            if (!SETT.IN_BOUNDS(fx, fy))
                return Dic.¤¤Problem;

            int tx = destX >> C.T_SCROLL;
            int ty = destY >> C.T_SCROLL;
            if (!SETT.IN_BOUNDS(tx, ty))
                return Dic.¤¤Problem;

            int h = SETT.TERRAIN().get(fx, fy).heightEnt(fx, fy) * C.TILE_SIZE + Trajectory.RELEASE_HEIGHT;
            h -= SETT.TERRAIN().get(tx, ty).heightEnt(tx, ty) * C.TILE_SIZE + Trajectory.HIT_HEIGHT / 2;

            CharSequence problem = ¤¤OUT_OF_RANGE;

            if (work.calcLow(h, startX, startY, destX, destY, angle, speed))
            {
                problem = trajectoryProblem(a, work, startX, startY);
            }

            if (problem != null && work.calcHigh(h, startX, startY, destX, destY, angle, speed))
            {
                problem = trajectoryProblem(a, work, startX, startY);
            }

            return problem;
        }

        public static CharSequence trajectoryProblem(Army a, Trajectory traj, int sx, int sy)
        {
            int tx = sx >> C.T_SCROLL;
            int ty = sy >> C.T_SCROLL;
            int h = SETT.TERRAIN().get(tx, ty).heightEnt(tx, ty) * C.TILE_SIZE + Trajectory.RELEASE_HEIGHT;
            return Updater.test(a, traj, h, sx, sy);
        }

        public static int releaseHeight(int tx, int ty)
        {
            return SETT.TERRAIN().get(tx, ty).heightEnt(tx, ty) * C.TILE_SIZE + Trajectory.RELEASE_HEIGHT;
        }

        public static int hitHeight(int tx, int ty)
        {
            return SETT.TERRAIN().get(tx, ty).heightEnt(tx, ty) * C.TILE_SIZE + Trajectory.HIT_HEIGHT / 2;
        }
    }
}