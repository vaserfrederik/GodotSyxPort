using System;
using System.IO;

namespace script
{
    /**
     * 
     * A dynamic script one can add to the game.
     *
     */
    public interface SCRIPT
    {
        /**
         * 
         * @return name of the script
         */
        public CharSequence Name { get; }

        /**
         * 
         * @return
         */
        public CharSequence Desc { get; }

        /**
         * Will be called before the game has had a chance to become set up. One can use hooks or potentially 
         * reflection here to do funky stuff, but common practise is to do nothing.
         * This will be called when starting the game, or loading a game.
         */
        public default void InitBeforeGameCreated()
        {
        }

        /**
         * Called after the game has been created, but before everything has been tightened 
         */
        public default void InitBeforeGameInited()
        {
        }

        /**
         * 
         * @return the script instance that will be actually updated by the game. This method is called once when leaving the
         * main menu, and once for every game load. Must not be null. The instance should be inited as if a new game was started.
         * This method will be called after all game resources have been set up.
         */
        public SCRIPT_INSTANCE CreateInstance();

        /**
         * 
         * @return true if the script can be selected when starting a new game and will appear in selectable lists. If not, it will be inserted into the game 
         */
        public default bool IsSelectable()
        {
            return true;
        }

        /**
         * 
         * @return only return true if this script should force itself on all new games and saves. Note that saving and loading might break.
         */
        public default bool ForceInit()
        {
            return false;
        }

        /**
         * A Script is a factory, responisble of creating this. This is where most logic should go.
         * @author Jake
         *
         */
        public interface SCRIPT_INSTANCE
        {
            /**
             * Called after each tick. A tick is an update of the game. the game is typically
             * updated 60 times per second. Check your conditions here, and act on them. 
             * @param ds - how many seconds of in-game time that has passed since the previous update.
             */
            public abstract void Update(double ds);

            /**
             * Use the fileputter here to save away any counters and variables
             * @param file
             */
            public abstract void Save(FilePutter file);

            /**
             * load your counters and variables back.
             * @param file
             * @throws IOException
             */
            public abstract void Load(FileGetter file);

            /**
             * A chance to manipulate the tooltip shown when hovering something.
             * @param mouseTimer
             * @param text
             */
            public default void HoverTimer(double mouseTimer, GBox text)
            {
            }

            /**
             * Called after the game has rendered. There's a chance here to render something on the screen.
             * @param r
             * @param ds
             */
            public default void Render(Renderer r, float ds)
            {
            }

            /**
             * Listen to a key. You can check if a key is pushed here.
             * @param key
             */
            public default void KeyPush(KEYS key)
            {
            }

            /**
             * 
             * @param button the button that has been clicked.
             */
            public default void MouseClick(MButt button)
            {
            }

            /**
             * 
             * @return if the script was not loaded correctly, here is a chance to fix the state and return true
             * if your script can handle that situation. If return false, then the script will not be run.
             */
            public default bool HandleBrokenSavedState()
            {
                return false;
            }

            /**
             * if you want to do something when hovering the screen.
             * @param mCoo
             * @param mouseHasMoved
             */
            public default void Hover(COORDINATE mCoo, bool mouseHasMoved)
            {
            }
        }
    }
}