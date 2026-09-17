using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenAL;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace snake2d
{
    class CORE_INFO
    {
        public string error = null;
        public string SGL_VENDOR;
        public string SGL_RENDERER;
        public string SGL_VERSION;
        public int monitors = 0;
        public DisplayMode[][] displays = new DisplayMode[0][];
        public DisplayMode[] currentdisplays = new DisplayMode[0];
        public string[] audioDevices;

        public static CORE_INFO cre2ate()
        {
            Process p = Proccesser.executeLwjgl(typeof(CORE_INFO), new string[] { }, new string[] { }, new string[] { });
            while (p.IsAlive)
                ;

            return new CORE_INFO(new Json(new FileInfo("Coreinfo.txt").FullName));
        }

        public static void Main(string[] args)
        {
            CORE_INFO f = new CORE_INFO();
            JsonE j = new JsonE();
            j.AddString("ERROR", f.error);
            j.AddString("SGL_VENDOR", f.SGL_VENDOR);
            j.AddString("SGL_RENDERER", f.SGL_RENDERER);
            j.AddString("SGL_VERSION", f.SGL_VERSION);
            j.Add("MONITORS", f.monitors);

            {
                JsonE[] displays = new JsonE[f.displays.Length];
                int i = 0;
                foreach (DisplayMode[] disps in f.displays)
                {
                    int k = 0;
                    JsonE[] res = new JsonE[disps.Length];

                    foreach (DisplayMode disp in disps)
                    {
                        res[k] = new JsonE();
                        res[k].Add("WI", disp.width);
                        res[k].Add("HI", disp.height);
                        res[k].Add("HZ", disp.refresh);
                        k++;
                    }

                    displays[i] = new JsonE();
                    displays[i].Add("AVAILABLE", res);
                    i++;
                }
                j.Add("DISPLAYS", displays);
            }
            {
                JsonE[] displays = new JsonE[f.currentdisplays.Length];
                for (int i = 0; i < displays.Length; i++)
                {
                    displays[i] = new JsonE();
                    displays[i].Add("WI", f.currentdisplays[i].width);
                    displays[i].Add("HI", f.currentdisplays[i].height);
                    displays[i].Add("HZ", f.currentdisplays[i].refresh);
                }
                j.Add("CURRENT", displays);
            }

            j.AddStrings("AUDIO_DEVICES", f.audioDevices);

            j.Save(Path.Combine(Directory.GetCurrentDirectory(), "Coreinfo.txt"));
        }

        private CORE_INFO(Json json)
        {
            error = json.text("ERROR");
            SGL_VENDOR = json.text("SGL_VENDOR");
            SGL_RENDERER = json.text("SGL_RENDERER");
            SGL_VERSION = json.text("SGL_VERSION");
            monitors = json.i("MONITORS");
            Json[] displays = json.jsons("DISPLAYS");
            this.displays = new DisplayMode[displays.Length][];
            int i = 0;
            foreach (Json j in displays)
            {
                Json[] modes = j.jsons("AVAILABLE");

                this.displays[i] = new DisplayMode[modes.Length];
                int k = 0;
                foreach (Json m in modes)
                {
                    this.displays[i][k++] = new DisplayMode(m.i("WI"), m.i("HI"), m.i("HZ"), false);
                }
                i++;
            }
            {
                Json[] ss = json.jsons("CURRENT");
                currentdisplays = new DisplayMode[ss.Length];
                for (int k = 0; k < displays.Length; k++)
                {
                    currentdisplays[k] = new DisplayMode(ss[k].i("WI"), ss[k].i("HI"), ss[k].i("HZ"), false);
                }
            }
            audioDevices = json.texts("AUDIO_DEVICES");
        }

        public CORE_INFO()
        {
            try
            {
                // Setup an error callback. The default implementation
                // will print the error message in System.err.
                // GLFWErrorCallback.createPrint(System.err).set();

                // Initialize GLFW. Most GLFW functions will not work before doing this.
                if (!GLFW.Init())
                {
                    error = "Unable to initialize GLFW";
                    return;
                }

                // Configure GLFW
                GLFW.DefaultWindowHints(); // optional, the current window hints are already the default
                GLFW.WindowHint(GLFW.Visible, false); // the window will stay hidden after creation

                // Create the window
                long window = GLFW.CreateWindow(300, 300, "test", IntPtr.Zero, IntPtr.Zero);

                if (window == IntPtr.Zero)
                {
                    error = "Unable to create window";
                    return;
                }

                // Make the OpenGL context current
                GLFW.MakeContextCurrent(window);

                GL.LoadAll();

                new Displays();

                monitors = Displays.monitors();

                displays = new DisplayMode[monitors][];

                for (int i = 0; i < displays.Length; i++)
                {
                    displays[i] = new DisplayMode[Displays.available(i).Count];
                    for (int k = 0; k < displays[i].Length; k++)
                        displays[i][k] = Displays.available(i)[k];
                }

                currentdisplays = new DisplayMode[monitors];
                for (int i = 0; i < monitors; i++)
                {
                    currentdisplays[i] = Displays.current(i);
                }

                SGL_VENDOR = GL.GetString(StringName.Vendor);
                SGL_RENDERER = GL.GetString(StringName.Renderer);
                SGL_VERSION = GL.GetString(StringName.Version);

                GLFW.Terminate();
                //GLFW.SetErrorCallback(null).Free();
            }
            catch (Exception e)
            {
                e.printStackTrace(System.err);
                error = e.Message;
            }

            List<string> ss = ALC.GetStringList(IntPtr.Zero, ALCString.AllDevicesSpecifier);
            if (ss.Count == 0)
                error = "No OpenAL device could be found. Try enabling sound and or / plug in/out speakers/earphones or restart your computer.";

            audioDevices = ss.ToArray();
        }
    }
}