using snake2d.util.map;

namespace settlement.path.components
{
    public abstract class SComponentLevel : MAP_OBJECT<SComponent>
    {
        protected SComponentLevel()
        {
        }

        public abstract int ComponentsMax();

        public abstract SComponent GetByIndex(int index);

        protected abstract void Update();

        public abstract int Level();

        public abstract int Size();

        protected abstract void Init();
    }
}