using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;

namespace game.events.slave
{
    public class UprisingSpot : Coo
    {
        private static readonly long serialVersionUID = 1L;
        public int signedUp;
        public int amountTotal;
        public int race;
        public bool valid = true;

        private UprisingSpot()
        {
        }

        public static UprisingSpot Make(int mx, int my, int amountTotal, Race race)
        {
            UprisingSpot s = new UprisingSpot();
            s.amountTotal = amountTotal;
            s.race = race.index;
            if (SetStart(s, mx, my, 32))
                return s;
            return null;
        }

        public static UprisingSpot Make(FileGetter g)
        {
            UprisingSpot s = new UprisingSpot();
            s.Load(g);
            return s;
        }

        public void MakeDiv(Div d, int id)
        {
            if (!IsPlacable(x(), y()))
            {
                if (!SetStart(this, x(), y(), 16))
                {
                    Clear();
                    return;
                }
            }
            d.settings().musteringSet(true);

            int am = CLAMP.i(signedUp, 0, Config.battle().MEN_PER_DIVISION - d.menNrOf());
            d.info.menSet(am);
            d.info.bannerISet(RND.rInt(GAME.ARMIES().banners.size()));
            d.info.name().clear().add(RACES.all().get(race).info.armyNames.rnd());
            GAME.ARMIES().factors.init(d);
            int w = (int)Math.Ceiling(Math.Sqrt(signedUp) / 2);

            foreach (ENTITY e in SETT.ENTITIES().getAllEnts())
            {
                if (am == 0)
                    break;
                if (e is Humanoid)
                {
                    Humanoid a = (Humanoid)e;
                    if (a.race().index == race && HPoll.Handler.isSlaveReadyForUprising(a) == id)
                    {
                        a.HTypeSet(HTYPES.ENEMY(), null, null);
                        a.setDivision(d);
                        am--;
                        signedUp--;
                        amountTotal--;
                    }
                }
            }
            DIR dir = DIR.NW;

            int x1 = (x() + dir.x() * w) * C.TILE_SIZE + C.TILE_SIZEH;
            int y1 = (y() + dir.y() * w) * C.TILE_SIZE + C.TILE_SIZEH;
            dir = DIR.NE;
            int x2 = x1 + dir.x() * w * C.TILE_SIZE;
            int y2 = y1 + dir.y() * w * C.TILE_SIZE;

            GAME.ARMIES().placer.deploy(d, x1, x2, y1, y2);
        }

        public override void Save(FilePutter file)
        {
            file.i(signedUp);
            file.i(amountTotal);
            file.i(race);
            base.Save(file);
        }

        public override void Load(FileGetter file)
        {
            signedUp = file.i();
            amountTotal = file.i();
            race = file.i();
            race = RACES.all().Get(race).index();
            base.Load(file);
        }

        public override void Clear()
        {
            signedUp = 0;
            amountTotal = 0;
            base.Clear();
        }

        public static bool SetStart(COORDINATEE res, int sx, int sy, int dist)
        {
            for (int i = 0; i < 100000; i++)
            {
                int distX = RND.rInt(dist + 1);
                int distY = dist + 1 - distX;
                int tx = (int)(sx + RND.rSign() * (distX + RND.rInt(1 + i)));
                int ty = (int)(sy + RND.rSign() * (distY + RND.rInt(1 + i)));
                if (SETT.ENV().map.SPACE.Get(tx, ty) == 1 && IsPlacable(tx, ty))
                {
                    res.Set(tx, ty);
                    return true;
                }
            }
            return false;
        }

        public bool Validate()
        {
            if (SETT.ENV().map.SPACE.Get(x(), y()) < 1 || !IsPlacable(x(), y()))
            {
                return SetStart(this, THRONE.coo().x(), THRONE.coo().y(), 128);
            }
            return true;
        }

        private static bool IsPlacable(int cx, int cy)
        {
            int amount = Config.battle().MEN_PER_DIVISION;
            int w = (int)Math.Ceiling(Math.Sqrt(amount));

            for (int y = 0; y < w; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!Placable(cx - w / 2 + x, cy - w / 2 + y))
                        return false;
                }
            }
            return true;
        }

        private static bool Placable(int tx, int ty)
        {
            if (!IN_BOUNDS(tx, ty))
                return false;

            if (PATH().solidity.is(tx, ty) || !PATH().reachability.is(tx, ty))
            {
                return false;
            }

            return true;
        }
    }
}