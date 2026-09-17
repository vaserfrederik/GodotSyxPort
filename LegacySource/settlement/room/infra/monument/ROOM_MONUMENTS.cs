using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Infra.Monument
{
    public sealed class RoomMonuments
    {
        public readonly List<RoomMonument> All;

        public RoomMonuments(RoomInitData init, RoomCategories cats) throws IOException
        {
            this.All = new RoomsCreator<RoomMonument>(init, "MONUMENT", cats.Decor)
            {
                public RoomMonument Create(string key, RoomInitData data, RoomCategorySub cat, int index)
                {
                    init.Init(key);
                    if (data.Data.Has("TYPE"))
                    {
                        if (data.Data.Value("TYPE") == "TORCH")
                            return new Torch(data, index, key, cat);
                    }

                    return new Imp(data, index, key, cat);
                }
            }.All();
        }
    }
}