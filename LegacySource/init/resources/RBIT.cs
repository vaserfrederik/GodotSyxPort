using System;
using System.IO;
using System.Collections.Generic;

namespace Init.Resources
{
    [Serializable]
    public class RBIT
    {
        /**
         * 
         */
        private static readonly long serialVersionUID = 1L;

        public static readonly RBIT NONE = new RBIT();
        public static readonly RBIT ALL = new RBIT(-1L, -1L);

        protected long l1;
        protected long l2;

        public RBIT()
        {
        }

        protected RBIT(long l1, long l2)
        {
            this.l1 = l1;
            this.l2 = l2;
        }

        public bool Has(RESOURCE res)
        {
            return (l1 & res.bitL1) != 0 || (l2 & res.bitL2) != 0;
        }

        public bool Has(RBIT other)
        {
            if (other == null)
                return false;
            return (l1 & other.l1) != 0 || (l2 & other.l2) != 0;
        }

        public bool HasAll(RBIT other)
        {
            if (other == null)
                return false;
            return (l1 & other.l1) == other.l1 && (l2 & other.l2) == other.l2;
        }

        public override string ToString()
        {
            return Convert.ToString(l2, 2) + " " + Convert.ToString(l1, 2);
        }

        public bool IsClear()
        {
            return l1 == 0 && l2 == 0;
        }

        public void Debug()
        {
            Console.WriteLine("rbits");
            foreach (RESOURCE res in RESOURCES.ALL())
            {
                if (Has(res))
                    Console.WriteLine(res);
            }
        }

        [Serializable]
        public class RBITImp : RBIT, SAVABLE
        {
            public static RBITImp tmp = new RBITImp();
            /**
             * 
             */
            private static readonly long serialVersionUID = 1L;

            public RBITImp()
            {
            }

            public RBITImp And(RESOURCE res)
            {
                l1 &= res.bitL1;
                l2 &= res.bitL2;
                return this;
            }

            public RBITImp And(RBIT other)
            {
                l1 &= other.l1;
                l2 &= other.l2;
                return this;
            }

            public RBITImp Or(RESOURCE res)
            {
                l1 |= res.bitL1;
                l2 |= res.bitL2;
                return this;
            }

            public RBITImp Or(RBIT other)
            {
                l1 |= other.l1;
                l2 |= other.l2;
                return this;
            }

            public RBITImp Clear(RESOURCE res)
            {
                l1 &= ~res.bitL1;
                l2 &= ~res.bitL2;
                return this;
            }

            public RBITImp Clear(RBIT other)
            {
                l1 &= ~other.l1;
                l2 &= ~other.l2;
                return this;
            }

            public RBITImp SetAll()
            {
                l1 = -1;
                l2 = -1;
                return this;
            }

            public override void Clear()
            {
                l1 = 0;
                l2 = 0;
            }

            public RBITImp ClearSet(RBIT other)
            {
                l1 = other.l1;
                l2 = other.l2;
                return this;
            }

            public void Save(FilePutter file)
            {
                file.WriteLong(l1);
                file.WriteLong(l2);
            }

            public void Load(FileGetter file)
            {
                l1 = file.ReadLong();
                l2 = file.ReadLong();
            }

            public void Toggle(RESOURCE resource)
            {
                l1 ^= resource.bitL1;
                l2 ^= resource.bitL2;
            }

            public void Xor(RBIT bits)
            {
                l1 &= ~bits.l1;
                l2 &= ~bits.l2;
            }

            public void Flip()
            {
                l1 = ~l1;
                l2 = ~l2;
            }

            public void Set(RESOURCE res, bool yes)
            {
                if (yes)
                    Or(res);
                else
                    Clear(res);
            }
        }
    }
}