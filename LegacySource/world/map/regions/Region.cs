using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.sets;
using world;
using world.region;

namespace world.map.regions
{
    public class Region : MAP_BOOLEAN, BOOSTABLE_O, INDEXED
    {
        private readonly short index;
        public readonly RegionInfo info = new RegionInfo();

        public Region(int index)
        {
            this.index = (short)index;
            Clear();
        }

        public int Index()
        {
            return index;
        }

        public bool Besieged()
        {
            return WORLD.BATTLES().Besiged(this);
        }

        public void Save(FilePutter f)
        {
            info.Save(f);
        }

        public void Load(FileGetter f)
        {
            info.Load(f);
        }

        public void Clear()
        {
            info.Clear();
        }

        public Faction Faction()
        {
            Realm realm = Realm();
            if (realm != null)
                return realm.Faction();
            return null;
        }

        public Realm Realm()
        {
            return RD.REALM(this);
        }

        public COLOR Color()
        {
            if (Faction() != null)
                return Faction().Banner().ColorBG();
            return COLOR.WHITE65;
        }

        public void FactionSet(Faction f, bool log)
        {
            RD.SetFaction(this, f, log);
        }

        public void SetCapitol()
        {
            RD.SetCapitol(this);
        }

        public bool Capitol()
        {
            if (Realm() == null)
                return false;
            return Realm().Capitol() == this;
        }

        public bool CanSetCapitol()
        {
            if (Faction() == null)
                return false;

            return true;
        }

        public int Cx()
        {
            return info.Cx();
        }

        public int Cy()
        {
            return info.Cy();
        }

        public bool Active()
        {
            return info.Area() > 0 && WORLD.REGIONS().Map.Is(Cx(), Cy());
        }

        public override string ToString()
        {
            return info.Name() + " " + Cx() + " " + Cy() + " " + info.Area() + " " + Index();
        }

        public bool Is(int tile)
        {
            return WORLD.REGIONS().Map.Get(tile) == this;
        }

        public bool Is(int tx, int ty)
        {
            return WORLD.REGIONS().Map.Get(tx, ty) == this;
        }

        public double BoostableValue(BValue v)
        {
            return v.VGet(this);
        }

        public bool IsBesigeTile(int tx, int ty)
        {
            if (WORLD.REGIONS().CentreEdgeTile().Is(tx, ty))
            {
                if (WORLD.PATH().Map.Is.Is(tx, ty) && WORLD.REGIONS().Centre.Get(tx, ty) != this)
                {
                    for (int di = 0; di < DIR.ALL.Size; di++)
                    {
                        DIR d = DIR.ALL.Get(di);
                        if (WORLD.PATH().Map.Can(tx, ty, d) & WORLD.REGIONS().Centre.Get(tx + d.X(), ty + d.Y()) == this)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}