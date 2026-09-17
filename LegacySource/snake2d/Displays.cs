using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace snake2d
{
    public static class Displays
    {
        private static Displays self;
        private readonly DisplayMode[] currentDisplay;
        private readonly LIST<LIST<DisplayMode>> supported;
        private static readonly LIST<DisplayMode> ssupported = new ArrayList<DisplayMode>(0);
        private readonly IGamePlatform platform;
        private readonly IGraphicsContext context;

        public Displays(IGamePlatform platform, IGraphicsContext context)
        {
            self = this;
            this.platform = platform;
            this.context = context;

            Console.WriteLine("DISPLAYS");

            var monitors = platform.GetMonitors();
            if (monitors.Length == 0)
            {
                throw new InvalidOperationException("No monitors are available!");
            }

            this.monitors = monitors;
            ArrayList<LIST<DisplayMode>> tmp = new ArrayList<LIST<DisplayMode>>(monitors.Length);
            currentDisplay = new DisplayMode[monitors.Length];

            for (int mi = 0; mi < monitors.Length; mi++)
            {
                var monitor = monitors[mi];

                var vmode = context.GetVideoMode(monitor);
                currentDisplay[mi] = new DisplayMode(vmode.Width, vmode.Height, vmode.RefreshRate, false);
                var vModes = context.GetVideoModes(monitor);
                Console.WriteLine("DISPLAY " + mi + " ( " + currentDisplay[mi].ToString() + " ) : ");
                ArrayList<DisplayMode> supp = new ArrayList<DisplayMode>(vModes.Length);

                for (int i = 0; i < vModes.Length; i++)
                {
                    supp.Add(new DisplayMode(vModes[i].Width, vModes[i].Height, vModes[i].RefreshRate, true));
                    Console.Write(" | " + supp[i].ToString());
                }
                Console.WriteLine();

                tmp.Add(supp);
            }
            Console.WriteLine("FINISHED");

            this.supported = tmp;
        }

        public static int Monitors()
        {
            if (self == null)
                return 0;
            return self.supported.Size();
        }

        static IGameMonitor pointer(int monitor)
        {
            return self.monitors[monitor];
        }

        public static LIST<DisplayMode> Available(int monitor)
        {
            if (self == null)
                return ssupported;
            return self.supported.Get(monitor);
        }

        public static DisplayMode Current(int monitor)
        {
            if (self == null)
                return null;
            return self.currentDisplay[monitor];
        }

        public static class DisplayMode
        {
            public readonly int width;
            public readonly int height;
            public readonly int refresh;
            public readonly bool fullScreen;

            public DisplayMode(int width, int height, int refresh, bool fullScreen)
            {
                this.width = width;
                this.height = height;
                this.refresh = refresh;
                this.fullScreen = fullScreen;
            }

            public override string ToString()
            {
                return width + "x" + height + "@" + refresh + "Hz";
            }
        }
    }
}