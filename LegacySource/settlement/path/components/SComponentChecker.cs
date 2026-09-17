using util.data;

namespace settlement.path.components
{
    public sealed class SComponentChecker : BOOLEANO<SComponent>
    {
        private readonly SComponentLevel level;
        private short[] neighbourcheck;
        private short neigbourcheckI = 0;

        public SComponentChecker(SComponentLevel level)
        {
            this.level = level;
            neighbourcheck = new short[level.ComponentsMax() + 100];
        }

        public SComponentChecker Init()
        {
            if (neighbourcheck.Length < level.ComponentsMax())
            {
                neighbourcheck = new short[level.ComponentsMax() + 100];
                neigbourcheckI = 0;
            }
            neigbourcheckI++;
            if (neigbourcheckI == 0)
            {
                for (int i = 0; i < neighbourcheck.Length; i++)
                    neighbourcheck[i] = 0;
                neigbourcheckI = 1;
            }
            return this;
        }

        /**
         * 
         * @param c
         * @return true if component previously has been set
         */
        public override bool Is(SComponent c)
        {
            return IsSet(c.Index());
        }

        public bool Inbounds(SComponent c)
        {
            if (c.Index() < 0 || c.Index() >= neighbourcheck.Length)
                return false;
            return true;
        }

        /**
         * 
         * @param c
         * @return true if component previously has been set
         */
        public bool IsSet(int c)
        {
            if (c < 0 || c >= neighbourcheck.Length)
                return false;
            return neighbourcheck[c] == neigbourcheckI;
        }

        public void Unset(SComponent c)
        {
            neighbourcheck[c.Index()] = (short)(neigbourcheckI - 1);
        }

        /**
         * 
         * @param x
         * @param y
         * @param c
         * @return true if component previously has been set
         */
        public bool IsSetAndSet(SComponent c)
        {
            if (!Is(c))
            {
                neighbourcheck[c.Index()] = neigbourcheckI;
                return false;
            }
            return true;
        }
    }
}