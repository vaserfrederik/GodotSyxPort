using System;
using System.Collections.Generic;
using Snake2D.Util.File;
using Snake2D.Util.Sets;
using Snake2D.Util.Sprite;
using Util.Info;
using Util.Keymap;

namespace Init.Type
{
    public abstract class TERRAIN : INFO, MAPPED
    {
        public readonly string key;
        private readonly int index;
        public readonly bool world;

        protected TERRAIN(ArrayList<TERRAIN> all, string key, Json json, string name, string desc, bool world)
            : base(name, desc)
        {
            this.key = key;
            json.Json(key);
            this.index = all.Add(this);
            this.world = world;
        }

        public int Index()
        {
            return index;
        }

        public abstract SPRITE Icon();

        public abstract double Value(int wx, int wy);

        public string Key()
        {
            return key;
        }
    }
}