using settlement.entity.humanoid;
using settlement.stats;
using util.data.GETTER_TRANS;
using util.info;

namespace settlement.stats.stat
{
    public abstract class StatObject<T> : GETTER_TRANSE<Humanoid, T>
    {
        public readonly INFO info;

        public StatObject(CharSequence name, CharSequence desc)
        {
            this.info = new INFO(name, desc);
        }

        public abstract T Get(Induvidual i);

        public override T Get(Humanoid f)
        {
            return Get(f.indu());
        }

        public abstract STAT Stat();
    }
}