using System;
using System.Collections.Generic;
using static Settlement.Main.SETT;
using static Game.GAME;
using static Game.Factions.FACTIONS;
using Init.Resources;
using Settlement.Path;
using Settlement.Room.Main;
using Snake2D.Util.Bit;
using Snake2D.Util.DataTypes;
using Snake2D.Util.Misc;
using Snake2D.Util.Rnd;

namespace Settlement.Maintenance
{
    internal sealed class MRoom : MType
    {
        private const double MIN_JOBS = 4;
        private static readonly Bits extraBit = new Bits(0b1000_0000_0000_0000_0000_0000_0000_0000);
        private static readonly Bits jobsPlaced = new Bits(0b0111_1111_0000_0000_0000_0000_0000_0000);
        private static readonly Bits secret = new Bits(0b0000_0000_0000_0000_0000_0000_0000_1111);
        private static readonly Bits tot = new Bits(0b0000_0000_1111_1111_1111_1111_1111_0000);
        private static readonly int jobSize = (int)(tot.mask / (3));

        public MRoom()
        {
            if (false)
            {
                //something is wrong with single tile rooms. They do not degrade... WTF is secret?
            }
        }

        public override bool Validate(int tx, int ty)
        {
            Room room = ROOMS().Map.Get(tx, ty);
            if (room != null)
            {
                Room_Degradable deg = room.Degradable(tx, ty);
                if (deg == null || IsBlocked(tx, ty, room))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private bool IsBlocked(int tx, int ty, Room room)
        {
            if (room.IsBadMaintenanceTile(tx, ty))
                return true;
            foreach (DIR d in DIR.ORTHO)
            {
                int dx = tx + d.X();
                int dy = ty + d.Y();
                if (!IN_BOUNDS(dx, dy))
                {
                    continue;
                }
                if (room.IsSame(tx, ty, dx, dy))
                {
                    if (PATH().Availability.Get(dx, dy).Player <= AVAILABILITY.ROOM.Player && PATH().Availability.Get(dx, dy).Player > 0)
                        return false;
                }
                else if (PATH().Availability.Get(dx, dy).Player > 0)
                    return false;
            }
            return true;
        }

        public override bool Degrade(int tx, int ty, int tile, double rate)
        {
            Room room = ROOMS().Map.Get(tx, ty);
            if (room == null)
                return false;

            Room_Degradable deg = room.Degradable(tx, ty);
            if (deg == null)
                return true;

            double r = deg.Rate(rate);
            if (Locked(room, tx, ty))
                r *= 3;
            Inc(tx, ty, r / room.Area(tx, ty));
            return true;
        }

        public override void Vandalize(int tx, int ty)
        {
            Inc(tx, ty, MIN_JOBS);
        }

        private static readonly double min = tot.mask >> 2;
        private static readonly double II = 1.0 / (tot.mask - min);

        public static double Degrade(int data)
        {
            double w = tot.Get(data);
            if (w > min)
            {
                w -= min;
                w *= II;

                return w;
            }
            return 0;
        }

        private void Inc(int tx, int ty, double am)
        {
            Room room = ROOMS().Map.Get(tx, ty);
            Room_Degradable deg = room.Degradable(tx, ty);

            int jz = jobSize(room.Area(tx, ty));

            double d = jz * am;
            int dd = (int)d;
            if (d > 0)
            {
                if (RND.rFloat() < dd - d)
                    dd++;
            }
            else
            {
                if (RND.rFloat() < -(dd - d))
                    dd--;
            }

            if (dd != 0)
            {
                int data = deg.GetData();

                int a = tot.Get(data) + dd + secret.Get(data) * jz;
                int s = CLAMP.i(a / jz, 0, 4);
                a -= s * jz;

                a = CLAMP.i(a, 0, tot.mask);

                data = tot.Set(data, a);
                data = secret.Set(data, s);
                Set(deg, data);
            }
        }

        private static int jobSize(int area)
        {
            return (int)Math.Ceiling((double)jobSize / area);
        }

        public override int ShouldPlaceResource(int tx, int ty)
        {
            Room room = ROOMS().Map.Get(tx, ty);
            if (room == null)
                return 0;
            Room_Degradable deg = room.Degradable(tx, ty);
            if (deg == null)
                return 0;

            if (deg.ResSize() == 0)
                return 0;

            double am = 0;
            for (int ri = 0; ri < deg.ResSize(); ri++)
            {
                am += deg.ResAmount(ri);
            }

            double rr = Room_Degradable.Rate(1, 1, 1, am, room.Area(tx, ty));
            double rrw = rr - Room_Degradable.Rate(1, 1, 1, 0, room.Area(tx, ty));

            if (rrw > rr * RND.rFloat())
            {
                double lim = am * RND.rFloat();
                am = 0;
                for (int ri = 0; ri < deg.ResSize(); ri++)
                {
                    int a = deg.ResAmount(ri);
                    if (a > 0)
                    {
                        am += deg.ResAmount(ri);
                        if (am >= lim && a > 0)
                        {
                            return ri + 1;
                        }
                    }
                }
            }
            return 0;
        }

        public override double ResRate(int tx, int ty, int ri)
        {
            if (ri == 0)
                return 0;
            ri--;

            Room room = ROOMS().Map.Get(tx, ty);
            if (room != null)
            {
                Room_Degradable deg = room.Degradable(tx, ty);
                if (deg != null)
                {
                    if (ri >= deg.ResSize())
                        return 0;
                    return Room_Degradable.RateResource(1, deg.Base(), room.Isolation(tx, ty), deg.ResAmount(ri)) / room.Area(tx, ty);
                }
            }
            return 0;
        }

        public override void Maintain(int tx, int ty)
        {
            Room room = ROOMS().Map.Get(tx, ty);

            Room_Degradable deg = room.Degradable(tx, ty);
            if (deg == null)
            {
                Notify("MAINTENANCE " + tx + " " + ty);
                return;
            }

            Inc(tx, ty, -1);
            Set(deg, jobsPlaced.Inc(deg.GetData(), -1));
        }

        public override RESOURCE Res(int tx, int ty, int ri)
        {
            throw new NotImplementedException();
        }

        public static int Jobs(int data, int area)
        {
            return secret.Get(data) + (tot.Get(data)) / jobSize(area);
        }

        public static void InitRoom(Room room, int rx, int ry)
        {
            Room_Degradable deg = room.Degradable(rx, ry);
            if (deg == null)
                return;
            int data = deg.GetData();
            data = jobsPlaced.Set(data, 0);
            Set(deg, data);
        }

        public override double Degrade(int tx, int ty)
        {
            Room room = ROOMS().Map.Get(tx, ty);
            Room_Degradable deg = room.Degradable(tx, ty);
            return deg.Get();
        }

        private static void Set(Room_Degradable deg, int data)
        {
            double d = Degrade(data);
            bool changed = false;
            if (d > 0.5)
            {
                if (extraBit.Get(data) == 0)
                {
                    data = extraBit.Set(data, 1);
                    changed = true;
                }
            }
            else if (d == 0)
            {
                if (extraBit.Get(data) == 1)
                {
                    data = extraBit.Set(data, 0);
                    changed = true;
                }
            }
            deg.SetData(data, changed);
        }

        public static bool DegradeReal(int data)
        {
            return extraBit.Get(data) == 1;
        }

        public static double SecretDegrade(int data)
        {
            return (double)tot.Get(data) / tot.mask;
        }
    }
}