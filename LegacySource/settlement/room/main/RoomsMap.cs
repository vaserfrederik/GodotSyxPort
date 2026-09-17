using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Main
{
    public class RoomsMap : MAP_OBJECT<Room>
    {
        private const int NOTHING = 0;
        private readonly int[] roomI = Alloc.Ii(SETT.TAREA);

        private int singletonI = 1;
        private const int singletonMax = 256;

        private const int CHUNKSIZE = 1024;
        private Room[] rooms = new Room[CHUNKSIZE];
        private int lastIndex = singletonMax;

        public RoomsMap()
        {
        }

        private readonly SAVABLE saver = new SAVABLE
        {
            Save = (FilePutter file) =>
            {
                file.I(rooms.Length);
                file.Is(roomI);

                int am = 0;
                foreach (Room r in rooms)
                {
                    if (r != null && !r.singleton)
                        am++;
                }
                file.I(am);

                foreach (Room r in rooms)
                {
                    if (r == null || r.singleton)
                        continue;
                    SETT.ROOMS().collection.saver().Save(r.blueprint(), file);

                    int pos = file.GetPosition();
                    file.I(0);
                    file.Object(r);
                    r.SaveExtra(file);
                    int le = file.GetPosition() - pos - 4;
                    file.SetAtPosition(pos, le);
                }
            },

            Load = (FileGetter file) =>
            {
                Room[] nn = new Room[file.I()];
                file.Is(roomI);

                for (int i = 0; i < singletonMax; i++)
                {
                    nn[i] = rooms[i];
                }
                rooms = nn;

                int am = file.I();

                for (int i = 0; i < am; i++)
                {
                    RoomBlueprint print = SETT.ROOMS().collection.loader().Load(file);
                    int le = file.I();
                    if (print != null)
                    {
                        Room r = (Room)file.Object(true);
                        if (r != null)
                        {
                            r.bI = (short)print.Index();
                            if (r.LoadExtra(file))
                            {
                                r.LoadFix();
                                rooms[r.Index()] = r;
                                continue;
                            }
                        }
                    }
                    file.SetPosition(file.GetPosition() + le);
                }

                for (int i = 0; i < SETT.TAREA; i++)
                {
                    if (roomI[i] != NOTHING && rooms[roomI[i]] == null)
                    {
                        SETT.ROOMS().fData.Clean(i);
                        SETT.ROOMS().pData.Set(i, 0);
                    }
                }

                for (lastIndex = singletonI; lastIndex < roomI.Length; lastIndex++)
                {
                    if (roomI[lastIndex] == NOTHING)
                    {
                        break;
                    }
                }
            },

            Clear = () =>
            {
                Room[] nn = new Room[CHUNKSIZE];
                for (int i = 0; i < singletonI; i++)
                {
                    nn[i] = rooms[i];
                }
                rooms = nn;
                for (int i = 0; i < SETT.TAREA; i++)
                {
                    roomI[i] = NOTHING;
                }
            }
        };

        public Room GetByIndex(int index)
        {
            return rooms[index];
        }

        public int Max()
        {
            return rooms.Length;
        }

        int Create(Room r, bool singleton)
        {
            if (singleton)
            {
                if (singletonI > singletonMax)
                    throw new RuntimeException("too many singleton rooms!");
                int i = singletonI;
                singletonI++;
                if (rooms[i] != null)
                    throw new RuntimeException();
                rooms[i] = r;
                return i;
            }

            for (; lastIndex < rooms.Length; lastIndex++)
            {
                if (rooms[lastIndex] == null)
                {
                    rooms[lastIndex] = r;
                    return lastIndex;
                }
            }

            lastIndex = singletonI;
            for (; lastIndex < rooms.Length; lastIndex++)
            {
                if (rooms[lastIndex] == null)
                {
                    rooms[lastIndex] = r;
                    return lastIndex;
                }
            }
            Room[] nn = new Room[rooms.Length + CHUNKSIZE];
            for (int i = 0; i < rooms.Length; i++)
                nn[i] = rooms[i];
            rooms = nn;
            rooms[lastIndex] = r;
            return lastIndex;
        }

        Room GetRaw(int tx, int ty)
        {
            return rooms[roomI[tx + ty * SETT.TWIDTH]];
        }

        void Set(int tile, Room n)
        {
            if (Get(tile) != null)
                throw new RuntimeException(tile + " " + n + " " + Get(tile));
            SETT.ROOMS().fData.Clean(tile);
            roomI[tile] = n.roomI;
            SETT.ROOMS().pData.Set(tile, 0);
        }

        void Clear(int tile, Room old)
        {
            if (old != Get(tile))
                throw new RuntimeException(tile + " " + indexGetter.Get(tile) + " " + old + " " + Get(tile) + " " + Get(tile % SETT.TWIDTH, tile / SETT.TWIDTH));
            SETT.ROOMS().fData.Clean(tile);
            SETT.ROOMS().pData.Set(tile, 0);
            roomI[tile] = NOTHING;
        }

        void Replace(int tile, Room old, Room current)
        {
            if (old != Get(tile))
                throw new RuntimeException((tile % SETT.TWIDTH) + " " + (tile / SETT.TWIDTH) + " " + Get(tile) + " " + old + " " + current);
            roomI[tile] = current.roomI;
        }

        TmpArea Delete(Room room, int mx, int my, object user)
        {
            TmpArea a = SETT.ROOMS().tmpArea(user);
            a.Set(room, mx, my);
            if (!(room is RoomSingleton) && room.blueprint() != SETT.ROOMS().THRONE)
            {
                if (rooms[room.roomI] == null)
                    throw new RuntimeException();
                rooms[room.roomI] = null;
                if (room.roomI < lastIndex)
                {
                    lastIndex = room.roomI;
                }
            }

            Init(a);

            return a;
        }

        public void Init(AREA room)
        {
            tmp.Set(room.Body());
            foreach (COORDINATE c in tmp)
            {
                if (room.Is(c))
                {
                    SETT.TILE_MAP().miniCUpdate(c.x(), c.y());
                    PATH().availability.UpdateAvailability(c.x(), c.y());
                    SETT.ENV().map.SetChanged(c.x(), c.y());
                    PATH().availability.UpdateService(c.x(), c.y());
                }
            }
        }

        private readonly Rec tmp = new Rec();

        public Room Get(int tile)
        {
            return rooms[roomI[tile]];
        }

        public Room Get(int tx, int ty)
        {
            if (IN_BOUNDS(tx, ty))
            {
                return rooms[roomI[tx + ty * SETT.TWIDTH]];
            }
            return null;
        }

        public readonly MAP_OBJECT<RoomBlueprint> blueprint = new MAP_OBJECT<RoomBlueprint>
        {
            Get = tile =>
            {
                int i = roomI[tile];
                if (i != NOTHING)
                {
                    Room r = GetByIndex(i);
                    if (r != null)
                        return r.blueprint();
                }
                return null;
            },

            Get = (tx, ty) =>
            {
                if (IN_BOUNDS(tx, ty))
                    return Get(tx + ty * SETT.TWIDTH);
                return null;
            }
        };

        public readonly MAP_OBJECT<RoomBlueprintImp> blueprintImp = new MAP_OBJECT<RoomBlueprintImp>
        {
            Get = tile =>
            {
                int i = roomI[tile];
                if (i != NOTHING)
                {
                    Room r = GetByIndex(i);
                    if (r != null && r.constructor() != null)
                        return r.constructor().blue();
                }
                return null;
            },

            Get = (tx, ty) =>
            {
                if (IN_BOUNDS(tx, ty))
                    return Get(tx + ty * SETT.TWIDTH);
                return null;
            }
        };

        public readonly MAP_OBJECT<RoomInstance> instance = new MAP_OBJECT<RoomInstance>
        {
            Get = tile =>
            {
                Room r = this.Get(tile);
                if (r is RoomInstance)
                    return (RoomInstance)r;
                return null;
            },

            Get = (tx, ty) =>
            {
                if (IN_BOUNDS(tx, ty))
                    return Get(tx + ty * SETT.TWIDTH);
                return null;
            }
        };

        public readonly MAP_OBJECT<ROOMA> rooma = new MAP_OBJECT<ROOMA>
        {
            private readonly RoomAreaWrapper wrap = new RoomAreaWrapper();

            Get = tile =>
            {
                wrap.Done();
                Room r = this.Get(tile);
                if (r != null)
                    return wrap.Init(r, tile % SETT.TWIDTH, tile / SETT.TWIDTH);
                return null;
            },

            Get = (tx, ty) =>
            {
                wrap.Done();
                if (IN_BOUNDS(tx, ty))
                {
                    Room r = this.Get(tx, ty);
                    if (r != null)
                        return wrap.Init(r, tx, ty);
                }
                return null;
            }
        };
    }
}