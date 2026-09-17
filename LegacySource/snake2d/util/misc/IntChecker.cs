using System;

namespace Snake2D.Util.Misc
{
    public class IntChecker
    {
        private readonly short[] check;
        private int checkI = 1;

        public IntChecker(int size)
        {
            check = new short[size];
        }

        public IntChecker Init()
        {
            checkI++;
            if (checkI == 0x0FFFF)
            {
                for (int i = 0; i < check.Length; i++)
                    check[i] = 0;
                checkI = 1;
            }
            return this;
        }

        /**
         * 
         * @param c
         * @return true if component previously has been set
         */
        public bool IsSet(int i)
        {
            return (check[i] & 0x0FFFF) == checkI;
        }

        public void Unset(int i)
        {
            check[i] = 0;
        }

        /**
         * 
         * @param x
         * @param y
         * @param c
         * @return true if component previously has been set
         */
        public bool IsSetAndSet(int i)
        {
            if (!IsSet(i))
            {
                check[i] = (short)(checkI);
                return false;
            }
            return true;
        }

        public int Size()
        {
            return check.Length;
        }
    }
}