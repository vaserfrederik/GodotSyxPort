using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Thing.HalfEntity
{
    public abstract class Factory<T> where T : HalfEntity
    {
        private readonly int index;
        private readonly Stack<T> free = new Stack<T>(128);

        protected Factory(List<Factory<?>> all)
        {
            index = all.Add(this);
        }

        protected abstract void Save(BinaryWriter file);

        protected abstract void Load(BinaryReader file) throws IOException;

        protected abstract void Clear();

        protected final T Create()
        {
            if (free.Count > 0)
                return free.Pop();
            return Make();
        }

        protected abstract T Make();

        protected void ReturnT(HalfEntity t)
        {
            if (free.Count < free.Capacity)
                free.Push((T)t);
        }
    }
}