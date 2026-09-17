using System;
using System.Text;

namespace Snake2D.Util.Sprite.Text
{
    public abstract class StrInserter<T>
    {
        public readonly string key;
        private static readonly Str tmp = new Str(128);

        public StrInserter(string key)
        {
            this.key = key;
        }

        protected abstract void Set(T t, Str str);

        public bool Insert(T t, Str str)
        {
            bool has = false;
            while (str.HasInsert(key))
            {
                tmp.Clear();
                has = true;
                Set(t, tmp);
                str.Insert(key, tmp);
            }
            return has;
        }

        public class Simple : StrInserter<CharSequence>
        {
            public Simple(string key) : base(key) { }

            protected override void Set(CharSequence t, Str str)
            {
                str.Add(t);
            }
        }
    }
}