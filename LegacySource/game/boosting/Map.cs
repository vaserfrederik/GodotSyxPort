using System;
using System.Collections.Generic;

namespace Game.Boosting
{
    public class Map : RMAP<Boostable>
    {
        private readonly ListGrower<Boostable> li;

        public Map() : base("BOOST", new ListGrower<Boostable>())
        {
            li = (ListGrower<Boostable>)this.All();
        }

        public void Clear()
        {
            li.Clear();
            map.Clear();
        }

        public void Add(Boostable b)
        {
            if (map.ContainsKey(key))
                throw new GameError("Another boostable with the same key exists " + key);
            li.Add(b);
            map.Put(b.key, b);
        }

        public KeyMap<Boostable> Map()
        {
            return map;
        }
    }
}