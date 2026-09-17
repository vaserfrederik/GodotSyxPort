using util.keymap;

namespace init.resources
{
    public class ResG : MAPPED
    {
        public readonly RESOURCE resource;
        private readonly int index;
        private readonly string key;

        public ResG(int index, string key, RESOURCE r)
        {
            this.index = index;
            resource = r;
            this.key = key;
        }

        public override int index()
        {
            return index;
        }

        public override string key()
        {
            return key;
        }
    }
}