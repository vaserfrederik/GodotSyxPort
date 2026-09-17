using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Main.Util
{
    public abstract class RoomsCreator<T> where T : RoomBlueprint
    {
        private readonly string type;
        private readonly RoomCategorySub cat;
        private readonly RoomInitData data;

        public RoomsCreator(RoomInitData data, string type, RoomCategorySub cat)
        {
            this.type = type;
            this.cat = cat;
            this.data = data;
        }

        public abstract T Create(string key, RoomInitData data, RoomCategorySub cat, int index);

        public List<T> All() 
        {
            data.SetType(type);

            LinkedList<T> tmp = new LinkedList<T>();
            string[] files = Directory.GetFiles(PATHS.INIT().GetFolder("room").FullName);

            foreach (string s in files)
            {
                string fileName = Path.GetFileName(s);
                if (fileName.StartsWith(type) && fileName.Length > type.Length && fileName[type.Length] == '_')
                {
                    tmp.Add(create(fileName, data, cat, tmp.Count));
                }
            }

            return new List<T>(tmp);
        }
    }
}