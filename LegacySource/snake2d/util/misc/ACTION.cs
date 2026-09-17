using System;

namespace snake2d.util.misc
{
    public interface ACTION
    {
        void Exe();
        
        public static readonly ACTION NOP = new ACTIONImpl();

        private class ACTIONImpl : ACTION
        {
            public void Exe()
            {
                
            }
        }
        
        public interface ACTION_O<T>
        {
            void Exe(T t);
        }
    }
}