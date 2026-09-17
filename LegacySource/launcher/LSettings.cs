using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace launcher
{
    public class LSettings
    {
        private readonly List<LSetting> all = new List<LSetting>();
        public readonly LSettingInt otherJVM = new LSettingInt("JVM", 0, 1);
        public readonly LSettingInt debug = new LSettingInt("DEBUG", 0, 1);
        public readonly LSettingInt developer = new LSettingInt("DEVELOPER", 0, 1);
        public readonly LSettingInt rpc = new LSettingInt("RPC", PATHS.IsSteam() && OS.Get() != OS.MAC ? 1 : 0, 1);
        public readonly LSettingInt linear = new LSettingInt("LINEAR", 1, 1);
        public readonly LSettingInt shading = new LSettingInt("SHADING", 1, 1);
        public readonly LSettingInt vsync = new LSettingInt("VSYNC", 0, 1);
        public readonly LSettingInt vsyncadapt = new LSettingInt("VSYNC_ADAPTIVE", 0, 1);
        public readonly LSettingInt easy = new LSettingInt("EASY_FONT", 0, 1);
        public readonly LSettingInt version = new LSettingInt("VERSION", -1, int.MaxValue - 1);
        public readonly LSettingInt monitor = new LSettingInt("MONITOR", 0, int.MaxValue - 1)
        {
            public override int Max() => Displays.Monitors() - 1;
        };

        public readonly LSettingInt screenMode = new LSettingInt("SCREEN_MODE", 0, 2);
        public readonly LSettingInt FPS = new LSettingInt("FPS_CAP", 0, 100);

        public const int screenModeBorderLess = 0;
        public const int screenModeFull = 1;
        public const int screenModeWindowed = 2;
        public readonly LSettingInt fullScreenDisplay = new LSettingInt("FULL_DISPLAY", 0, int.MaxValue - 1)
        {
            public override int Max()
            {
                var dis = Displays.Available(monitor.Get());
                if (dis == null || dis.Count == 0)
                    return 0;
                return dis.Count - 1;
            }
        };

        public readonly LSettingInt windowWidth = new LSettingInt("WINDOW_WIDTH", 15, 20)
        {
            public override int Min()
            {
                return (int)(Max() * (double)C.MIN_WIDTH / Displays.Current(monitor.Get()).Width);
            }
        };

        public readonly LSettingInt windowHeight = new LSettingInt("WIDOW_HEIGHT", 15, 20)
        {
            public override int Min()
            {
                return (int)(Max() * (double)C.MIN_HEIGHT / Displays.Current(monitor.Get()).Height);
            }
        };

        public readonly LSettingInt windowBorderLessScale = new LSettingInt("WIDOW_SCALE", 0, 100)
        {
            public override int Max()
            {
                double dh = Displays.Current(monitor.Get()).Height / (double)C.MIN_HEIGHT;
                double dv = Displays.Current(monitor.Get()).Width / (double)C.MIN_WIDTH;
                double d = Math.Min(dh, dv);
                d -= 1.0;
                return (int)CLAMP.d(d / 0.05, 0, 100);
            }

            public override double GetD()
            {
                return Get() * 0.05;
            }
        };

        public readonly LSettingInt decorated = new LSettingInt("WINDOW_DECORATE", 1, 1);
        public readonly LSettingInt forcedHD = new LSettingInt("WINDOW_FORCE_HD", 0, 1);
        public readonly LSettingInt shadows = new LSettingInt("SHADOWS", 2, 2);
        public readonly LSettingInt particles = new LSettingInt("PARICLES", 2, 2);
        public readonly LSettingInt gore = new LSettingInt("GORE", 2, 2);
        public readonly LSettingInt volumeMaster = new LSettingInt("VOLUME_MASTER", 70, 100);
        public readonly LSettingInt volumeSound = new LSettingInt("VOLUME_SOUND", 100, 100);
        public readonly LSettingInt volumeMusic = new LSettingInt("VOLUME_MUSIC", 70, 100);
        public readonly LSettingInt volumeAmbience = new LSettingInt("VOLUME_AMBIENCE", 70, 100);
        public readonly LSettingInt focusMute = new LSettingInt("FOCUS_MUTE", 1, 1);
        public readonly LSettingInt brightness = new LSettingInt("BRIGHTNESS22", 50, 100);
        public readonly LSettingInt autoSaveInterval = new LSettingInt("AUTO_SAVE_TIME", 9, 10);
        public readonly LSettingInt autoSaveFiles = new LSettingInt("AUTO_SAVE_FILES", 5, 10);
        public readonly LSettingInt edgeScroll = new LSettingInt("EDGE_SCROLL", 0, 1);
        public readonly LSettingInt detail = new LSettingInt("GRAPHIC_DETAIL", 1, 1);
        public readonly LSettingInt lightCycle = new LSettingInt("LIGHT_CYCLE", 1, 1);
        public readonly LSettingInt uiLightCycle = new LSettingInt("UI_LIGHT_CYCLE", 1, 1);
        public readonly LSettingInt downpour = new LSettingInt("DOWNPOUR", 1, 1);
        public readonly LSettingInt winIconi = new LSettingInt("WIN_AUTO_ICONIFY", 1, 1);
        public readonly LSettingInt winFoat = new LSettingInt("WINDOW_FLOAT", 0, 1);
        public readonly LSettingInt winFullFull = new LSettingInt("WINDOW_FULL_FULL", 0, 1);
        public readonly SString alternateJVM = new SString("PATH_JAVA", "");
        public readonly SString lang = new SString("LANGUAGE", "");
        public readonly SString audiodevice = new SString("OPENAL", "");
        public readonly SStrings mods = new SStrings("MODS", new string[] { });
        public readonly SStrings jvmArguments = new SStrings("JVM_ARGS2", new string[] {
            "-Xms512m",
            "-Xmx4096m",
            "-XX:+UseCompressedOops",
            "-Dfile.encoding=UTF-8",
            "-server",
            "-Dfml.earlyprogresswindow=false",
            "-XX:+UseSerialGC"
        });

        public void SetDefault()
        {
            foreach (var s in all)
                s.SetDefault();
        }

        public LSettings()
        {
            try
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(PATHS.Local().SETTINGS.Get("LauncherSettings")));
                foreach (var s in all)
                {
                    if (json.ContainsKey(s.key))
                        s.Read(json);
                    else
                        s.SetDefault();
                }
            }
            catch (Exception e)
            {
                e.printStackTrace(Console.Out);
                SetDefault();
                Save();
            }
        }

        public void Save()
        {
            var json = new Dictionary<string, object>();
            foreach (var s in all)
                s.Write(json);
            File.WriteAllText(PATHS.Local().SETTINGS.Get("LauncherSettings"), JsonConvert.SerializeObject(json));
        }

        public abstract class LSetting
        {
            protected readonly string key;

            protected LSetting(string key)
            {
                this.key = key;
                all.Add(this);
            }

            public abstract void SetDefault();

            public abstract void Read(Dictionary<string, object> json);

            public abstract void Write(Dictionary<string, object> json);
        }

        public class LSettingInt : LSetting, INumber<int>
        {
            private int v;

            public LSettingInt(string key, int defaultValue, int maxValue) : base(key)
            {
                this.defaultValue = defaultValue;
                this.maxValue = maxValue;
            }

            public override void SetDefault()
            {
                v = defaultValue;
            }

            public override void Read(Dictionary<string, object> json)
            {
                if (json.ContainsKey(key))
                {
                    if (int.TryParse(json[key].ToString(), out int value))
                        v = Math.Min(value, maxValue);
                }
                else
                {
                    SetDefault();
                }
            }

            public override void Write(Dictionary<string, object> json)
            {
                json[key] = v;
            }

            public int Get()
            {
                return v;
            }

            public void Set(int t)
            {
                v = Math.Min(t, maxValue);
                Save();
            }

            public int Min => 0;

            public int Max => maxValue;

            private readonly int defaultValue;
            private readonly int maxValue;
        }

        public class SString : LSetting, IGetter<string>
        {
            private string current;

            public SString(string key, string def) : base(key)
            {
                this.def = def;
            }

            public override void SetDefault()
            {
                current = def;
            }

            public override void Read(Dictionary<string, object> json)
            {
                current = json.ContainsKey(key) ? json[key].ToString() : null;
            }

            public override void Write(Dictionary<string, object> json)
            {
                json[key] = current;
            }

            public string Get()
            {
                return current;
            }

            public void Set(string t)
            {
                current = t;
            }

            public string def;
        }

        public class SStrings : LSetting, IGetter<string[]>
        {
            private string[] current;

            public SStrings(string key, string[] def) : base(key)
            {
                this.key = key;
                this.def = def;
            }

            public override void SetDefault()
            {
                current = def;
            }

            public override void Read(Dictionary<string, object> json)
            {
                if (json.ContainsKey(key))
                {
                    var array = json[key] as JArray;
                    current = array?.ToObject<string[]>() ?? def;
                }
                else
                {
                    current = def;
                }
            }

            public override void Write(Dictionary<string, object> json)
            {
                json[key] = current;
            }

            public string[] Get()
            {
                return current;
            }

            public void Set(string[] t)
            {
                current = t;
            }

            public string key;
            public string[] def;
        }
    }
}