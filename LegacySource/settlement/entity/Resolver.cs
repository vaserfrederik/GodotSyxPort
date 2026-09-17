using System;

namespace Settlement.Entity
{
    internal sealed class Resolver
    {
        public Resolver()
        {
        }

        private readonly ECollision ca = new ECollision();
        private readonly ECollision cb = new ECollision();

        public void ResolveCollision(ENTITY a, ENTITY b)
        {
            if (!a.CollidesWithOthers(b) || !b.CollidesWithOthers(a))
            {
                a.Meet(b);
                b.Meet(a);
                return;
            }

            if (!a.WillCollideWith(b) && !b.WillCollideWith(a))
            {
                a.Meet(b);
                b.Meet(a);
                return;
            }

            double ya1 = a.Physics.GetZ();
            double ya2 = ya1 + a.Physics.GetHeight();
            double yb1 = b.Physics.GetZ();
            double yb2 = yb1 + b.Physics.GetHeight();

            if (ya1 > yb2 || ya2 < yb1)
            {
                return;
            }

            double distX = b.Body().CX() - a.Body().CX();
            double distY = b.Body().CY() - a.Body().CY();

            double norLength = Math.Sqrt((distX * distX + distY * distY));

            if (norLength == 0)
            {
                // do something making sense
                // a.Interactor().Collide(b, 0, 0, 0);
                // b.Interactor().Collide(a, 0, 0, 0);
                return;
            }

            double norX = distX / norLength;
            double norY = distY / norLength;

            double mA = a.Physics.GetMass();
            double mB = b.Physics.GetMass();

            if (mA <= 0 || mB <= 0)
                return;

            ESpeed.Imp va = a.Speed;
            ESpeed.Imp vb = b.Speed;
            double vAX = va.X();
            double vAY = va.Y();
            double vBX = vb.X();
            double vBY = vb.Y();

            double dVX = vBX - vAX;
            double dVY = vBY - vAY;
            if (dVX * norX > 0 && dVY * norY > 0)
            {
                // a.Interactor().Collide(b, norX, norY, 0);
                // b.Interactor().Collide(a, -norX, -norY, 0);
                return;
            }

            double momX = dVX * norX < 0 ? dVX : 0;
            double momY = dVY * norY < 0 ? dVY : 0;

            double mom = Math.Sqrt(momX * momX + momY * momY);

            double speedDotA = (1 + a.Speed.Dir().XN() * norX + a.Speed.Dir().YN() * norY) * 0.5;
            double speedDotB = (1 - b.Speed.Dir().XN() * norX - b.Speed.Dir().YN() * norY) * 0.5;
            ca.Other = b;
            ca.DirDot = speedDotA;
            ca.DirDotOther = speedDotB;
            ca.NorX = norX;
            ca.NorY = norY;
            ca.TileMomentum = 0;
            ca.DamageTileStrength = 0;
            ca.SpeedHasChanged = false;
            ca.Leave = null;

            cb.Other = a;
            cb.DirDot = speedDotB;
            cb.DirDotOther = speedDotA;
            cb.NorX = -norX;
            cb.NorY = -norY;
            cb.TileMomentum = 0;
            cb.DamageTileStrength = 0;
            cb.SpeedHasChanged = false;
            cb.Leave = null;

            a.SetCollideDamage(ca, cb);
            b.SetCollideDamage(cb, ca);

            if (mom > 0)
            {
                mom *= 1 + (a.Physics.GetRestitution() + b.Physics.GetRestitution()) * 0.5;

                if (mA <= 0)
                {
                    SolidCollision(a, b, -norX, -norY, mA);
                    ca.SpeedHasChanged = true;
                    cb.SpeedHasChanged = true;
                    ca.TileMomentum += mom;
                    a.Collide(ca);
                    cb.TileMomentum += mom;
                    b.Collide(cb);

                    return;
                }
                else if (mB <= 0)
                {
                    SolidCollision(b, a, norX, norY, mB);
                    mom *= mA;
                    ca.SpeedHasChanged = true;
                    cb.SpeedHasChanged = true;
                    ca.TileMomentum += mom;
                    a.Collide(ca);
                    cb.TileMomentum = +mom;
                    b.Collide(cb);
                    return;
                }
                double magScale = 1.0;
                if (mA > mB)
                {
                    dVX /= mom;
                    dVY /= mom;
                    mom *= mB;
                    magScale -= 0.3 * mB / mA;
                }
                else
                {
                    dVX /= mom;
                    dVY /= mom;
                    mom *= mA;
                    magScale -= 0.3 * mA / mB;
                }

                CollidePair(a, b, -norX, -norY, mom, dVX, dVY, magScale);
                ca.TileMomentum += mom;
                cb.TileMomentum += mom;
                ca.SpeedHasChanged = true;
                cb.SpeedHasChanged = true;
                a.Collide(ca);
                b.Collide(cb);

            }
            else
            {
                a.Collide(ca);
                b.Collide(cb);

            }
        }

        private static void CollidePair(ENTITY a, ENTITY b, double norX, double norY, double mom, double dx, double dy, double magScale)
        {
            if (mom == 0)
                mom = 1;

            ESpeed.Imp va = a.Speed;
            ESpeed.Imp vb = b.Speed;

            double x = mom, y = mom;

            x *= norX;
            y *= norY;

            va.SetRaw(va.X() + x * a.Physics.MassI, va.Y() + y * a.Physics.MassI);
            vb.SetRaw(vb.X() - x * b.Physics.MassI, vb.Y() - y * b.Physics.MassI);

            va.MagnitudeInit(va.Magnitude() * magScale);
            vb.MagnitudeInit(vb.Magnitude() * magScale);
        }

        private static double SolidCollision(ENTITY p, ENTITY solid, double norX, double norY, double m)
        {
            double vAX = p.Speed.NX();
            double vAY = p.Speed.NY();

            if (vAX * norX + vAY * norY >= 0)
                return 0;

            double r = (p.Physics.GetRestitution() + solid.Physics.GetRestitution()) / 2;
            double resX = 5 * norX;
            double resY = 5 * norY;

            if (vAX * norX > 0)
            {
                resX -= vAX * r;
            }
            else
            {
                resX += vAX;
            }
            if (vAY * norY > 0)
            {
                resY -= vAY * r;
            }
            else
            {
                resY += vAY;
            }

            p.Speed.SetRaw(resX, resY);
            double momX = (vAX - resX);
            double momY = (vAY - resY);
            return p.Physics.GetMass() * Math.Sqrt(momX * momX + momY * momY);
        }
    }
}