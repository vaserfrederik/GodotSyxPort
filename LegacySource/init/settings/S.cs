using System;
using System.Collections.Generic;
using System.IO;

namespace Init.Settings
{
    public static class S
    {
        private static S s;

        public static S Get()
        {
            if (s == null)
            {
                s = new S();
            }
            return s;
        }

        private readonly ArrayListResize<Setting> all = new ArrayListResize<Setting>(128, 128);
        private readonly LSettings settings = new LSettings();
        public readonly bool developer;
        public readonly bool debug;

        public readonly Setting shadows;
        public readonly Setting particles;
        public readonly Setting graphics;
        public readonly Setting gore;
        public readonly Setting volumeMaster;
        public readonly Setting volumeSound;
        public readonly Setting volumeMusic;
        public readonly Setting volumeAmbience;
        public readonly List<Setting> audio;
        public readonly Setting muteUnfocused;
        public readonly Setting brightness;
        public readonly Setting autoSaveInterval;
        public readonly Setting autoSaveFiles;
        public readonly Setting scroll;
        public readonly Setting lightCycle;
        public readonly Setting uilightCycle;
        public readonly Setting downpour;

        private S()
        {
            S.s = this;

            D.GInit(this);

            var samount = new CharSequence[] { Dic.¤¤off, D.g("some"), D.g("lots") };

            developer = settings.developer.Get() == 1;
            debug = settings.debug.Get() == 1;
            shadows = GetSetting("shadows", samount);
            particles = GetSetting("particles", samount);
            graphics = GetSetting("graphics", new CharSequence[] { "low", "medium", "high" });
            gore = GetSetting("gore", samount);
            volumeMaster = GetSetting("volumeMaster", null);
            volumeSound = GetSetting("volumeSound", null);
            volumeMusic = GetSetting("volumeMusic", null);
            volumeAmbience = GetSetting("volumeAmbience", null);
            audio = new List<Setting> { volumeMaster, volumeSound, volumeMusic, volumeAmbience };
            muteUnfocused = GetSetting("muteUnfocused", new CharSequence[] { "off", "on" });
            brightness = GetSetting("brightness", null);
            autoSaveInterval = GetSetting("autoSaveInterval", null);
            autoSaveFiles = GetSetting("autoSaveFiles", null);
            scroll = GetSetting("scroll", null);
            lightCycle = GetSetting("lightCycle", new CharSequence[] { "off", "day", "night" });
            uilightCycle = GetSetting("uilightCycle", new CharSequence[] { "off", "day", "night" });
            downpour = GetSetting("downpour", new CharSequence[] { "off", "light", "heavy" });

            all.Add(shadows);
            all.Add(particles);
            all.Add(graphics);
            all.Add(gore);
            all.Add(volumeMaster);
            all.Add(volumeSound);
            all.Add(volumeMusic);
            all.Add(volumeAmbience);
            all.Add(muteUnfocused);
            all.Add(brightness);
            all.Add(autoSaveInterval);
            all.Add(autoSaveFiles);
            all.Add(scroll);
            all.Add(lightCycle);
            all.Add(uilightCycle);
            all.Add(downpour);

            all.TrimExcess();
        }

        private Setting GetSetting(string name, CharSequence[] options)
        {
            return new Setting(name, options, settings);
        }

        public LISTINGS Make()
        {
            return new LISTINGS()
            {
                GetWindowName = () => C.NAME,
                GetVSynchEnabled = () => settings.vsync.Get() == 1,
                VsyncAdaptive = () => settings.vsyncadapt.Get() == 1,
                GetScreenshotFolder = () => $"{PATHS.Local().SCREENSHOT.Get()}{Path.DirectorySeparatorChar}",
                GetRenderMode = () => settings.shading.Get(),
                GetPointSize = () => C.SCALE,
                Mutonfocus = () => muteUnfocused.Get() == 1,
                GetNativeWidth = () =>
                {
                    int w = NWidth();
                    int h = NHeight();

                    if (h < C.MIN_HEIGHT)
                        h = C.MIN_HEIGHT;
                    if (w < C.MIN_WIDTH)
                        w = C.MIN_WIDTH;

                    double a = w * h;
                    double d = Math.Pow(C.MAX_SCREEN_AREA / a, 0.5);

                    if (a > C.MAX_SCREEN_AREA)
                    {
                        w = (int)(w * d);
                        h = (int)(h * d);
                        if (h < C.MIN_HEIGHT)
                        {
                            h = C.MIN_HEIGHT;
                            w = C.MAX_SCREEN_AREA / h;
                        }
                    }

                    w &= ~1;
                    C.Init(w, C.HEIGHT());
                    return w;
                },
                GetNativeHeight = () =>
                {
                    int w = NWidth();
                    int h = NHeight();

                    if (h < C.MIN_HEIGHT)
                        h = C.MIN_HEIGHT;
                    if (w < C.MIN_WIDTH)
                        w = C.MIN_WIDTH;

                    double a = w * h;
                    double d = Math.Pow(C.MAX_SCREEN_AREA / a, 0.5);

                    if (a > C.MAX_SCREEN_AREA)
                    {
                        w = (int)(w * d);
                        h = (int)(h * d);
                        if (h < C.MIN_HEIGHT)
                        {
                            h = C.MIN_HEIGHT;
                            w = C.MAX_SCREEN_AREA / h;
                        }
                    }
                    h &= ~1;
                    C.Init(C.WIDTH(), h);
                    return h;
                },
                NWidth = () =>
                {
                    if (settings.screenMode.Get() == LSettings.ScreenModeWindowed)
                    {
                        int w = (int)Math.Ceiling(Displays.Current(Monitor()).Width * settings.windowWidth.GetD());

                        if (settings.developer.Get() == 1 && settings.forcedHD.Get() == 1)
                        {
                            w = 1920;

                            if (w > Displays.Current(Monitor()).Width)
                                w = Displays.Current(Monitor()).Width;
                        }
                        return w;
                    }
                    else if (settings.screenMode.Get() == LSettings.ScreenModeBorderLess)
                        return (int)Math.Ceiling(Displays.Current(Monitor()).Width / (1.0 + settings.windowBorderLessScale.GetD()));

                    return Display().Width;
                },
                NHeight = () =>
                {
                    if (settings.screenMode.Get() == LSettings.ScreenModeWindowed)
                    {
                        int h = (int)Math.Ceiling(Displays.Current(Monitor()).Height * settings.windowHeight.GetD());

                        if (settings.developer.Get() == 1 && settings.forcedHD.Get() == 1)
                        {
                            h = 1080;

                            if (h > Displays.Current(Monitor()).Height)
                                h = Displays.Current(Monitor()).Height;
                        }
                        return h;
                    }
                    else if (settings.screenMode.Get() == LSettings.ScreenModeBorderLess)
                        return (int)Math.Ceiling(Displays.Current(Monitor()).Height / (1.0 + settings.windowBorderLessScale.GetD()));
                    return Display().Height;
                },
                GetLinearFiltering = () => settings.linear.Get() == 1,
                GetIconFolder = () => PATHS_BASE.ICON_FOLDER,
                GetFitToScreen = () =>
                {
                    if (settings.screenMode.Get() == LSettings.ScreenModeBorderLess)
                        return true;
                    if (settings.screenMode.Get() == LSettings.ScreenModeWindowed)
                        return true;
                    if (settings.windowWidth.GetD() == 1 && settings.windowHeight.GetD() == 1)
                        return true;
                    return true;
                },
                Display = () =>
                {
                    if (settings.screenMode.Get() == LSettings.ScreenModeFull)
                    {
                        if (settings.fullScreenDisplay.Get() == -1)
                        {
                            var d = Displays.Current(Monitor());
                            return new DisplayMode(d.Width, d.Height, d.Refresh, true);
                        }
                        return Displays.Available(Monitor())[settings.fullScreenDisplay.Get()];
                    }

                    if (settings.screenMode.Get() == LSettings.ScreenModeBorderLess)
                        return Displays.Current(Monitor());

                    int width = NWidth();
                    int height = NHeight();

                    return new DisplayMode(width, height, Displays.Current(Monitor()).Refresh, false);
                },
                DecoratedWindow = () => settings.decorated.Get() == 1 && settings.screenMode.Get() == LSettings.ScreenModeWindowed,
                DebugMode = () => debug,
                Monitor = () => Math.Clamp(settings.monitor.Get(), 0, Displays.Monitors()),
                OpenALDevice = () => settings.audiodevice.Get(),
                AutoIconify = () => settings.winIconi.Get() == 1,
                WindowFloating = () => settings.winFoat.Get() == 1,
                WindowFullFull = () => settings.winFullFull.Get() == 1,
                FPS = () =>
                {
                    int f = settings.FPS.Get();
                    if (f == 0)
                        return -1;
                    return f;
                }
            };
        }
    }

    public class Setting
    {
        public string Name { get; }
        public CharSequence[] Options { get; }
        private readonly LSettings _settings;

        public Setting(string name, CharSequence[] options, LSettings settings)
        {
            Name = name;
            Options = options;
            _settings = settings;
        }

        public int Get()
        {
            // Implement logic to get the setting value
            return 0;
        }

        public double GetD()
        {
            // Implement logic to get the double setting value
            return 0.0;
        }

        public void Set(int value)
        {
            // Implement logic to set the setting value
        }

        public void Set(double value)
        {
            // Implement logic to set the double setting value
        }
    }

    public class ArrayListResize<T>
    {
        private List<T> _list;

        public ArrayListResize(int capacity, int initialSize)
        {
            _list = new List<T>(capacity);
            for (int i = 0; i < initialSize; i++)
            {
                _list.Add(default);
            }
        }

        public void Add(T item)
        {
            _list.Add(item);
        }

        public void TrimExcess()
        {
            _list.TrimExcess();
        }
    }

    public class CharSequence { }

    public class D { public static void GInit(S s) { } }

    public class LSettings
    {
        public int Get(string setting) => 0; // Placeholder for actual implementation
        public double GetD(string setting) => 0.0; // Placeholder for actual implementation
        public void Set(string setting, int value) { } // Placeholder for actual implementation
        public void Set(string setting, double value) { } // Placeholder for actual implementation
        public static int ScreenModeWindowed => 0; // Placeholder for actual implementation
        public static int ScreenModeBorderLess => 1; // Placeholder for actual implementation
        public static int ScreenModeFull => 2; // Placeholder for actual implementation
        public int monitor { get; set; }
        public int vsync { get; set; }
        public int vsyncadapt { get; set; }
        public int shading { get; set; }
        public int linear { get; set; }
        public int windowWidth { get; set; }
        public int windowHeight { get; set; }
        public int windowBorderLessScale { get; set; }
        public int decorated { get; set; }
        public int winIconi { get; set; }
        public int winFoat { get; set; }
        public int winFullFull { get; set; }
        public int FPS { get; set; }
        public string audiodevice { get; set; }
        public int forcedHD { get; set; }
    }

    public class PATHS
    {
        public static class Local
        {
            public static class SCREENSHOT
            {
                public static string Get() => string.Empty; // Placeholder for actual implementation
            }
        }
    }

    public class PATHS_BASE
    {
        public static string ICON_FOLDER => string.Empty; // Placeholder for actual implementation
    }

    public class DISPLAYS
    {
        public static List<DISPLAY> Current(int monitor) => new List<DISPLAY>(); // Placeholder for actual implementation
        public static List<DISPLAY> Available(int monitor) => new List<DISPLAY>(); // Placeholder for actual implementation
        public static int Monitors => 0; // Placeholder for actual implementation
    }

    public class DISPLAY
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Refresh { get; set; }
    }

    public class DISPLAY_MODE
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Refresh { get; set; }
        public bool Fullscreen { get; set; }
    }

    public class C
    {
        public static int WIDTH => 0; // Placeholder for actual implementation
        public static int HEIGHT => 0; // Placeholder for actual implementation
        public static int MIN_WIDTH => 0; // Placeholder for actual implementation
        public static int MIN_HEIGHT => 0; // Placeholder for actual implementation
        public static int MAX_WIDTH => 0; // Placeholder for actual implementation
        public static int MAX_HEIGHT => 0; // Placeholder for actual implementation
        public static int POINT_SIZE => 0; // Placeholder for actual implementation
        public static int NAME => 0; // Placeholder for actual implementation
        public static void Init(int width, int height) { } // Placeholder for actual implementation
    }
}