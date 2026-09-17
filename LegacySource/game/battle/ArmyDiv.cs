using System.IO;

namespace game.battle
{
    public abstract class ArmyDiv
    {
        protected ArmyDiv()
        {
        }

        protected abstract void Save(FilePutter file);

        protected abstract void Load(FileGetter file);

        protected abstract void Clear();
    }
}