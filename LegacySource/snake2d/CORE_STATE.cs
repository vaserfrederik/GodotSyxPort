using System;
using System.Collections.Generic;

namespace snake2d
{
    public abstract class CORE_STATE
    {
        public interface Constructor
        {
            CORE_STATE getState();

            default void doAfterSet()
            {
            }
        }

        /**
         * updates the game!
         * @param ds
         */
        protected abstract void update(float ds, double slowTheFuckDown);

        /**
         * 
         * @param key
         * @param scancode
         * @param action Keyboard.RELEASE|PRESS|REPEAT
         */
        protected abstract void keyPush(List<KeyEvent> keys, bool hasCleared);

        /**
         * this character has been pressed
         * @param c
         */
        //protected abstract void charPush(char c);
        /**
         * A mouse Button has been pressed!
         * @param button
         */
        protected abstract void mouseClick(MButt button);

        protected abstract void render(Renderer r, float ds);

        protected virtual void exit()
        {
        }
    }
}