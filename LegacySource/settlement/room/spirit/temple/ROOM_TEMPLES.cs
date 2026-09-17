using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Spirit.Temple
{
    public sealed class RoomTemples
    {
        public readonly LIST<RoomTemple> ALL;

        public readonly LIST<RoomShrine> SHRINES;
        public readonly LIST<LIST<RoomTemple>> perRel;
        public readonly LIST<LIST<RoomShrine>> perRelShrine;

        public RoomTemples(ROOMS rooms, RoomInitData init) throws IOException
        {
            ALL = new RoomsCreator<RoomTemple>(init, "TEMPLE", rooms.CATS.SER_REL)
            {
                public RoomTemple Create(string key, RoomInitData data, RoomCategorySub cat, int index) throws IOException
                {
                    return new RoomTemple(index, data, key, cat);
                }
            }.All();

            SHRINES = new RoomsCreator<RoomShrine>(init, "SHRINE", rooms.CATS.SER_REL)
            {
                public RoomShrine Create(string key, RoomInitData data, RoomCategorySub cat, int index) throws IOException
                {
                    return new RoomShrine(key, index, init, cat);
                }
            }.All();

            {
                ArrayList<LIST<RoomTemple>> tt = new ArrayList(RELIGIONS.ALL().Count);

                foreach (Religion rel in RELIGIONS.ALL())
                {
                    ArrayListGrower<RoomTemple> res = new ArrayListGrower<RoomTemple>();
                    foreach (RoomTemple t in ALL)
                    {
                        if (t.Religion == rel)
                            res.Add(t);
                    }
                    tt.Add(res);
                }
                this.perRel = tt;
            }

            {
                ArrayList<LIST<RoomShrine>> tt = new ArrayList(RELIGIONS.ALL().Count);

                foreach (Religion rel in RELIGIONS.ALL())
                {
                    ArrayListGrower<RoomShrine> res = new ArrayListGrower<RoomShrine>();
                    foreach (RoomShrine t in SHRINES)
                    {
                        if (t.Religion == rel)
                            res.Add(t);
                    }
                    tt.Add(res);
                }

                this.perRelShrine = tt;
            }
        }

        public LIST<RoomTemple> Temples(Religion rel)
        {
            return perRel.Get(rel.Index());
        }

        public LIST<RoomShrine> Shrines(Religion rel)
        {
            return perRelShrine.Get(rel.Index());
        }
    }
}