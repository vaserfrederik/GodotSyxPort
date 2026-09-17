using System;
using settlement.entity;
using static settlement.main.SETT;
using game;
using init.constant;
using settlement.entity.humanoid;
using settlement.main;
using settlement.path;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.rnd;
using util;

public class ResolverTile
{
    private ResolverTile()
    {
    }

    private static double attack(int tx, int ty, double m, ENTITY e)
    {
        if (!IN_BOUNDS(tx, ty))
            return m;

        if (e is Humanoid && GAME.ARMIES().map.attackableI.is(tx, ty, ((Humanoid)e).indu()))
        {
            double str = GAME.ARMIES().map.strength.get(tx, ty) * 2;
            str += RND.rFloat() * str;

            if (m > str + RND.rFloat() * str)
            {
                GAME.ARMIES().map.breakIt(tx, ty);
                return str;
            }
            if (!collides(tx, ty, e))
            {
                return CLAMP.d(m - str, 0, m);
            }
        }

        return m;
    }

    private static void resolve(int tx, int ty, ENTITY a, EPHYSICS.Solid p)
    {
        double x1 = p.body().x1();
        double ox1 = p.x1;
        double y1 = p.body().y1();
        double oy1 = p.y1;
        double size = p.body().width();

        double vx = (x1 - ox1);
        final double targetX;
        if (vx > 0)
        {
            targetX = (tx << C.T_SCROLL) - size - 1;
        }
        else if (vx < 0)
        {
            targetX = (tx << C.T_SCROLL) + C.TILE_SIZE + 1;
        }
        else
        {
            targetX = x1;
        }

        double vy = (y1 - oy1);
        final double targetY;
        if (vy > 0)
        {
            targetY = (ty << C.T_SCROLL) - size - 1;
        }
        else if (vy < 0)
        {
            targetY = (ty << C.T_SCROLL) + C.TILE_SIZE + 1;
        }
        else
        {
            targetY = y1;
        }

        if (vy == 0 && vx == 0)
        {
            GAME.Notify("entity stuck in tile, but havent moved " + tx + " " + ty);
        }
        else if (vx == 0)
        {
            p.hitbox.moveY1(targetY);
            if (vy * a.speed.nY() > 0)
            {
                double m = Math.Abs(a.speed.y() * p.getMass());
                double m2 = attack(tx, ty, m, a);
                bool broken = m2 < m;
                if (a.collideTile(broken, 0, vy < 0 ? -1 : 1, m2 * a.physics.massI, tx, ty))
                {
                    if (broken)
                    {
                        double ny = m2 * p.getMassI();
                        if (a.speed.nY() < 0)
                            ny = -ny;
                        a.speed.setRaw(a.speed.x(), ny);
                    }
                    else
                    {
                        a.speed.setRaw(a.speed.x(), -a.speed.y() * p.getRestitution());
                    }
                }
                else if (!broken)
                {
                    a.speed.magnitudeInit(0);
                }
            }
        }
        else if (vy == 0)
        {
            p.hitbox.moveX1(targetX);
            if (vx * a.speed.nX() > 0)
            {
                double m = Math.Abs(a.speed.x() * p.getMass());
                double m2 = attack(tx, ty, m, a);
                bool broken = m2 < m;
                if (a.collideTile(broken, vx < 0 ? -1 : 1, 0, m2 * a.physics.massI, tx, ty))
                {
                    if (broken)
                    {
                        double nx = m2 * p.getMassI();
                        if (a.speed.nX() < 0)
                            nx = -nx;
                        a.speed.setRaw(nx, a.speed.y());
                    }
                    else
                    {
                        a.speed.setRaw(-a.speed.x() * p.getRestitution(), a.speed.y());
                    }
                }
                else if (!broken)
                {
                    a.speed.magnitudeInit(0);
                }
            }
        }
        else
        {
            if (Math.Abs(vx) > Math.Abs(vy))
            {
                p.hitbox.moveX1(targetX);
                if (vx * a.speed.nX() > 0)
                {
                    double m = Math.Abs(a.speed.x() * p.getMass());
                    double m2 = attack(tx, ty, m, a);
                    bool broken = m2 < m;
                    if (a.collideTile(broken, vx < 0 ? -1 : 1, 0, m2 * a.physics.massI, tx, ty))
                    {
                        if (broken)
                        {
                            double nx = m2 * p.getMassI();
                            if (a.speed.nX() < 0)
                                nx = -nx;
                            a.speed.setRaw(nx, a.speed.y());
                        }
                        else
                        {
                            a.speed.setRaw(-a.speed.x() * p.getRestitution(), a.speed.y());
                        }
                    }
                    else if (!broken)
                    {
                        a.speed.magnitudeInit(0);
                    }
                }
            }
            else
            {
                p.hitbox.moveY1(targetY);
                if (vy * a.speed.nY() > 0)
                {
                    double m = Math.Abs(a.speed.y() * p.getMass());
                    double m2 = attack(tx, ty, m, a);
                    bool broken = m2 < m;
                    if (a.collideTile(broken, 0, vy < 0 ? -1 : 1, m2 * a.physics.massI, tx, ty))
                    {
                        if (broken)
                        {
                            double ny = m2 * p.getMassI();
                            if (a.speed.nY() < 0)
                                ny = -ny;
                            a.speed.setRaw(a.speed.x(), ny);
                        }
                        else
                        {
                            a.speed.setRaw(a.speed.x(), -a.speed.y() * p.getRestitution());
                        }
                    }
                    else if (!broken)
                    {
                        a.speed.magnitudeInit(0);
                    }
                }
            }
        }
    }

    private static bool collides(int tx, int ty, ENTITY e)
    {
        AVAILABILITY a = map().getAvailability(tx, ty);
        if (!a.tileCollide)
            return false;
        if (e is Humanoid)
        {
            return a.isSolid(((Humanoid)e).indu().army());
        }
        return a.player < 0;
    }

    private static bool resolve(ENTITY a, EPHYSICS.Solid p)
    {
        int tx1 = (p.hitbox.x1() >> C.T_SCROLL);
        int ty1 = (p.hitbox.y1() >> C.T_SCROLL);
        int tx2 = (p.hitbox.x2() >> C.T_SCROLL);
        int ty2 = (p.hitbox.y2() >> C.T_SCROLL);
        if (tx1 == p.tx1 && ty1 == p.ty1 && tx2 == p.tx2 && ty2 == p.ty2)
        {
            return false;
        }

        if (collides(tx1, ty1, a))
        {
            resolve(tx1, ty1, a, p);
            return true;
        }
        if (tx1 != tx2 && collides(tx2, ty1, a))
        {
            resolve(tx2, ty1, a, p);
            return true;
        }
        if (ty1 != ty2 && collides(tx1, ty2, a))
        {
            resolve(tx1, ty2, a, p);
            return true;
        }
        if (tx1 != tx2 && ty1 != ty2 && collides(tx2, ty2, a))
        {
            resolve(tx2, ty2, a, p);
            return true;
        }

        p.tx1 = (short)tx1;
        p.ty1 = (short)ty1;
        p.tx2 = (short)tx2;
        p.ty2 = (short)ty2;

        if (map().getAvailability(p.tileC().x(), p.tileC().y()).player < 0)
            a.collideUnconnected();

        return false;
    }

    public static bool collide(ENTITY a)
    {
        EPHYSICS.Solid p = a.physics;

        if (resolve(a, p))
        {
            int i = 0;
            while (resolve(a, p))
            {
                i++;
                if (i > 4)
                {
                    GAME.Notify("killing trapped entity... solong" + " " + a.physics.tileC());
                    a.helloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie();
                    return false;
                }
            }

            int tx1 = (p.hitbox.x1() >> C.T_SCROLL);
            int ty1 = (p.hitbox.y1() >> C.T_SCROLL);
            int tx2 = (p.hitbox.x2() >> C.T_SCROLL);
            int ty2 = (p.hitbox.y2() >> C.T_SCROLL);

            p.tx1 = (short)tx1;
            p.ty1 = (short)ty1;
            p.tx2 = (short)tx2;
            p.ty2 = (short)ty2;
            return true;
        }

        return false;
    }

    private static PATHING map()
    {
        return PATH();
    }

    static bool trapped(ENTITY a)
    {
        EPHYSICS.Solid p = a.physics;

        int cx = p.hitbox.cX();
        int cy = p.hitbox.cY();
        int tx = cx >> C.T_SCROLL;
        int ty = cy >> C.T_SCROLL;
        int index = 0;

        while (index < GUTIL.circle().length())
        {
            index++;
            if (GUTIL.circle().radius(index) > 10)
                break;
            COORDINATE c = GUTIL.circle().get(index);
            int dx = c.x() + tx;
            int dy = c.y() + ty;
            if (!IN_BOUNDS(dx, dy))
                continue;
            if (!collides(dx, dy, a))
            {
                p.hitbox.moveC((dx << C.T_SCROLL) + C.TILE_SIZEH, (dy << C.T_SCROLL) + C.TILE_SIZEH);

                double norX = dx - tx;
                double norY = dy - ty;

                if (norX < 0)
                {
                    norX = -1;
                }
                else if (norX > 0)
                {
                    norX = 1;
                }
                if (norY < 0)
                {
                    norY = -1;
                }
                else if (norY > 0)
                {
                    norY = 1;
                }

                if (norX != 0 && norY != 0)
                {
                    norX *= C.SQR2I;
                    norY *= C.SQR2I;
                }

                a.collideTile(false, norX, norY, 0, tx, ty);

                int tx1 = (p.hitbox.x1() >> C.T_SCROLL);
                int ty1 = (p.hitbox.y1() >> C.T_SCROLL);
                int tx2 = (p.hitbox.x2() >> C.T_SCROLL);
                int ty2 = (p.hitbox.y2() >> C.T_SCROLL);

                p.tx1 = (short)tx1;
                p.ty1 = (short)ty1;
                p.tx2 = (short)tx2;
                p.ty2 = (short)ty2;
                return true;
            }
        }

        Console.Error.WriteLine("killing trapped entity... solong");
        a.helloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie();
        return false;
    }
}