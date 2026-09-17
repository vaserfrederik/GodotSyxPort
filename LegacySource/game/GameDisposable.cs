using System;
using System.Collections.Generic;

namespace game
{
    /**
     * Come and add yourself to the init phase of the game. If you've added yourself, you are
     * guaranteed to be disposed the next time the game is inited,
     * @author mail__000
     *
     */
    public abstract class GameDisposable
    {
        private static List<GameDisposable> initers = new List<GameDisposable>(180);

        public GameDisposable()
        {
            initers.Add(this);
        }

        protected abstract void Dispose();

        static void DisposeAll()
        {
            foreach (GameDisposable i in initers)
                i.Dispose();
        }

//        public static class Counter : GameDisposable
//        {
//            private int i;
//
//            public int GetNext()
//            {
//                int j = i;
//                i++;
//                return j;
//            }
//
//            @Override
//            protected void Dispose()
//            {
//                i = 0;
//            }
//        }
    }
}