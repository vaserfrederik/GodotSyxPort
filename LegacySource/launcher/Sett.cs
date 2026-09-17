using System;
using init.paths;
using snake2d;

namespace launcher
{
    class Sett : SETTINGS
    {
        public Sett() { }

        public const int WIDTH = 896;
        public const int HEIGHT = 448;
        public const int SCALE = 4;
        private readonly DisplayMode display = new DisplayMode(WIDTH, HEIGHT, 60, false);

        public int getNativeWidth()
        {
            return WIDTH;
        }

        public int getNativeHeight()
        {
            return HEIGHT;
        }

        public int getRenderMode()
        {
            return 0;
        }

        public string getWindowName()
        {
            return "SOS Launcher";
        }

        public bool getVSynchEnabled()
        {
            return false;
        }

        public int getPointSize()
        {
            return SCALE;
        }

        public bool getLinearFiltering()
        {
            return false;
        }

        public string getIconFolder()
        {
            return PATHS_BASE.ICON_FOLDER;
        }

        public bool getFitToScreen()
        {
            return false;
        }

        public bool decoratedWindow()
        {
            return true;
        }

        public DisplayMode display()
        {
            return display;
        }

        public bool debugMode()
        {
            return false;
        }

        public string getScreenshotFolder()
        {
            return "" + PATHS.local().SCREENSHOT.get();
        }

        public int monitor()
        {
            return 0;
        }

        public string openALDevice()
        {
            return null;
        }

        public bool autoIconify()
        {
            // TODO Auto-generated method stub
            return false;
        }

        public bool windowFloating()
        {
            // TODO Auto-generated method stub
            return false;
        }

        public bool vsyncAdaptive()
        {
            return false;
        }

        public bool windowFullFull()
        {
            // TODO Auto-generated method stub
            return false;
        }

        public int FPS()
        {
            return -1;
        }
    }
}