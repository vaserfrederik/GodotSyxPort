using System;
using System.IO;

namespace Game.Save
{
    public abstract class Savable
    {
        public readonly string key;

        public Savable(string key)
        {
            this.key = key;
        }

        protected abstract void Save(FilePutter file);
        protected abstract void Load(FileGetter file);
        protected void LoadFail()
        {
            throw new Exception("Failed to load critical resource: " + key);
        }
    }
}