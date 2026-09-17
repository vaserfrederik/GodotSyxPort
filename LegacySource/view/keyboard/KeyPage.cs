using System.Collections.Generic;

namespace View.Keyboard
{
    public abstract class KeyPage
    {
        private readonly ArrayListResize<Key> all = new ArrayListResize<Key>(64, 1024);
        private readonly MapIndexed<Key> map = new MapIndexed<Key>();
        public readonly string key;

        protected KeyPage(string key)
        {
            this.key = key;
        }

        public Key Get(int modCode, int keyCode)
        {
            if (map.Contains(Key.Hash(modCode, keyCode)))
                return map.Get(Key.Hash(modCode, keyCode));
            return null;
        }

        public IList<Key> All()
        {
            return all;
        }

        public abstract System.CharSequence Name();
    }
}