using System.IO;
using Init.Paths;
using Settlement.Room.Main;
using Snake2D.Util.File;

namespace Settlement.Room.Main.Util
{
    public class RoomInitData
    {
        private PATH gData = PATHS.INIT().GetFolder("room");
        private PATH gText = PATHS.TEXT().GetFolder("room");
        public readonly PATH gSprite = PATHS.SPRITE().GetFolder("room");
        private Json data;
        private Json text;
        private string key;
        private string type;

        public readonly ROOMS m;

        public RoomInitData(ROOMS m)
        {
            this.m = m;
        }

        public string Key()
        {
            return key;
        }

        public Json Data()
        {
            return data;
        }

        public Json Text()
        {
            return text;
        }

        public Path Sp()
        {
            return gSprite.Get(key);
        }

        public Path Sp(string type, string key)
        {
            return gSprite.Get(key);
        }

        public string Type()
        {
            return type;
        }

        public RoomInitData Init(string key)
        {
            data = new Json(gData.Gets(key));
            text = new Json(gText.Gets(key));
            this.key = key;
            return this;
        }

        public PATH Getter()
        {
            return gData;
        }

        public RoomInitData SetType(string type)
        {
            this.type = type;
            return this;
        }
    }
}