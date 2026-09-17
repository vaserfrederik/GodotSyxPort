using System;

namespace snake2d
{
    public interface SETTINGS
    {
        /**
         * 
         * @return if vsync should be enabled. Locks refresh rate. Reduces screen tearing.
         */
        bool getVSynchEnabled();
        int getNativeWidth();
        int getNativeHeight();

        /**
         * 
         * @return if the rendered scene should be stretched and distorted to fit the selected screen resolution
         */
        bool getFitToScreen();

        /**
         * 
         * @return size of particles
         */
        int getPointSize();

        /**
         * 
         * @return if native width < displaywidth, which filtering to use.
         */
        bool getLinearFiltering();

        /**
         * 
         * @return
         * 0 == normal
         * 1 == deffered lightning + normal map
         */
        int getRenderMode();

        /**
         * 
         * @return the name that will be displayed in the decoration of the window
         */
        string getWindowName();

        /**
         * 
         * @return the absolute path of the location of:
         * icon16.png
         * icon32.png
         * icon48.png
         * ex C:\syx\
         * can be null
         */
        string getIconFolder();

        string getScreenshotFolder();

        bool mutonfocus();

        /**
         * 
         * @return if a windowed window should be decorated
         */
        bool decoratedWindow();

        int monitor();
        DisplayMode display();

        /**
         * if you want messages from LWJGL / OPENGL
         * @return
         */
        bool debugMode();

        string openALDevice();

        bool vsyncAdaptive();

        bool windowFloating();
        bool autoIconify();

        bool windowFullFull();

        /**
         * - 1 to go with the screen
         * @return
         */
        int FPS();
    }
}