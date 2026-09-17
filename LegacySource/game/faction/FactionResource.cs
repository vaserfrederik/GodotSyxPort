using System.IO;

namespace Game.Faction
{
    public abstract class FactionResource
    {
        protected abstract void Save(FilePutter file);
        protected abstract void Load(FileGetter file);
        protected abstract void Clear();
        protected abstract void Update(double ds, Faction f);
    }
}