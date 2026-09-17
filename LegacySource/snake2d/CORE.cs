using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace snake2d
{
    public class CORE
    {
        private CORE()
        {
        }

        public static readonly float UPDATE_SECONDS_MAX = 1f / 32f;
        public static readonly float UPDATE_SECONDS_MIN = 1f / 1024f;

        private static bool created;
        private static GraphicContext graphics;
        private static Input input;
        private static SOUND_CORE soundCore;
        private static volatile bool running = true;
        private static Updater updater;
        private static volatile CORE_STATE.Constructor newState;
        private static volatile Exception updateException;
        private static volatile GlJob glJob;
        private static Thread glThread;
        private static int FPS;
        private static volatile bool swapping = false;
        private static volatile bool debug;

        public static void Init(ERROR_HANDLER error)
        {
            glThread = Thread.CurrentThread;
            Errors.Init(error);
            Thread.CurrentThread.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                {
                    if (updateException != null)
                    {
                        Errors.Handle(updateException);
                        ex?.PrintStackTrace();
                    }
                    else
                        Errors.Handle(ex);
                    running = false;
                    try
                    {
                        Dispose();
                    }
                    catch (Exception e2)
                    {
                        e2?.PrintStackTrace();
                    }
                }
            };
        }

        public static void Create(SETTINGS settings)
        {
            if (Thread.CurrentThread != glThread)
                throw new RuntimeException();

            int mb = 1014 * 1024;
            var platform = $"{System.Runtime.InteropServices.RuntimeInformation.OSDescription} {System.Runtime.InteropServices.RuntimeInformation.OSArchitecture} Platform.";
            var nrOfPross = Environment.ProcessorCount;
            var jre = Environment.Version.ToString();

            var JREargs = new List<string>(Environment.GetCommandLineArgs());
            Printer.Ln("SYSTEM INFO");
            Printer.Ln($"---Running on a: {platform} {OS.Get()}");
            var bits = IntPtr.Size * 8;
            Printer.Ln($"---jre: {jre} bits: {bits}");
            Printer.Ln($"---charset: {Encoding.Default.EncodingName}");
            Printer.Ln($"---Processors available: {nrOfPross}");
            Printer.Ln($"---JRE Memory");

            var run = GC.GetTotalMemory(false);
            // available memory
            Printer.Ln($"      Total: {run / mb}");
            Printer.Ln($"      Free: {GC.GetTotalMemory(true) / mb}");
            Printer.Ln($"      Used: {(run - GC.GetTotalMemory(true)) / mb}");
            Printer.Ln($"      Max: {run / mb}");
            Printer.Ln("---JRE Input Arguments : ", string.Join(", ", JREargs));
            Printer.Ln("---JRE cp : ", System.Reflection.Assembly.GetEntryAssembly().Location);

            Printer.Fin();

            if (created)
            {
                throw new RuntimeException("Core already created!");
            }

            created = true;
            swapping = false;

            graphics = new GraphicContext(settings);
            input = new Input(graphics, settings);

            soundCore = SOUND_CORE.Create(settings);

            FPS = graphics.RefreshRate;
            debug = settings.DebugMode();
        }

        private static void SetUpdater(CORE_STATE.Constructor state)
        {
            updater = new Updater(state);
            updater.UnhandledException += (sender, e) =>
            {
                running = false;
                Printer.Ln("ERROR IN UPDATER DETECTED");
                updateException = e.ExceptionObject as Exception;
            };
            updater.IsBackground = true;
            updater.Start();
        }

        public static void PerformWork(Action action, string name)
        {
            var t = new Thread(() => action());
            t.Name = name;
            t.UnhandledException += (sender, e) =>
            {
                running = false;
                Printer.Ln($"ERROR IN THREAD: {name} DETECTED");
                updateException = e.ExceptionObject as Exception;
            };
            t.Start();
        }

        public static void Start()
        {
            while (running)
            {
                if (Thread.CurrentThread == glThread && !swapping)
                {
                    graphics.FlushRenderer();
                    graphics.PollEvents();
                    running = graphics.SwapAndCheckClose();

                    return;
                }

                swapping = true;
                while (swapping)
                    Sleep(1);
            }
        }

        private static void Sleep(int ms)
        {
            Thread.Sleep(ms);
        }

        private static ArrayList<CORE_RESOURCE> clientDisposables = new ArrayList<CORE_RESOURCE>(20);

        public static void AddDisposable(CORE_RESOURCE dis)
        {
            if (Thread.CurrentThread != glThread)
                throw new RuntimeException("gl resource must be created using a gl job :(. Threading sucks!");
            if (dis is TextureHolder)
                GetGraphics().SetTexture((TextureHolder)dis);
            clientDisposables.Add(dis);
        }

        public static LIST<CORE_RESOURCE> Disposables()
        {
            return clientDisposables;
        }

        public abstract class GlJob
        {
            protected abstract void DoJob();

            private volatile bool ru = true;

            public final void Gc()
            {
                GC.Collect();
            }

            public final void Perform()
            {
                var t = Thread.CurrentThread;

                if (debug)
                {
                    var ss = new Thread(() =>
                    {
                        long now = DateTime.Now.Ticks;
                        while (ru)
                        {
                            if (DateTime.Now.Ticks - now < 20000 * TimeSpan.TicksPerMillisecond)
                                Sleep(1);
                            else
                            {
                                Console.Error.WriteLine("gl Thread is stuck!!!");
                                foreach (var e in glThread.GetStackTrace())
                                    Console.Error.WriteLine(e);

                                Console.Error.WriteLine($"{t.Name} is stuck!!!");
                                foreach (var e in t.GetStackTrace())
                                    Console.Error.WriteLine(e);

                                now = DateTime.Now.Ticks;
                            }
                        }
                    });
                    ss.Name = "gljob";
                    ss.Start();
                }

                if (Thread.CurrentThread == glThread && !swapping)
                    DoJob();
                else
                {
                    glJob = this;
                    while (glJob != null && running)
                        Sleep(1);
                }

                ru = false;
            }
        }

        static bool IsRunning()
        {
            return running;
        }

        public static void Annihilate(Exception e)
        {
            updateException = e;
            running = false;
            updater.DieHard();
        }

        public static void Annihilate()
        {
            running = false;
            updater.DieHard();
        }

        public static Input GetInput()
        {
            return input;
        }

        public static GraphicContext GetGraphics()
        {
            return graphics;
        }

        public static Renderer Renderer()
        {
            return graphics.Renderer;
        }

        public static SOUND_CORE GetSoundCore()
        {
            return soundCore;
        }

        public static CoreTime GetUpdateInfo()
        {
            return updater.GetCoreInfo();
        }

        public static void SwapAndPoll()
        {
            if (Thread.CurrentThread == glThread && !swapping)
            {
                graphics.FlushRenderer();
                graphics.PollEvents();
                running = graphics.SwapAndCheckClose();

                return;
            }

            swapping = true;
            while (swapping)
                Sleep(1);
        }

        public static void CheckIn()
        {
            new GlJob
            {
                DoJob = () =>
                {
                    input.Poll(DateTime.Now.Ticks, graphics.IsFocused());
                    input.ClearAllInput();
                }
            }.Perform();
        }

        public static void SetCurrentState(CORE_STATE.Constructor stateMaker)
        {
            newState = stateMaker;
            updater.DieHard();
        }

        private static void Dispose()
        {
            if (!created)
                return;

            Printer.Ln();
            Printer.Ln("DISPOSING");

            DisposeClient();

            clientDisposables.Clear();
            if (soundCore != null)
            {
                soundCore.Dis();
                soundCore = null;
            }
            if (input != null)
            {
                input.Dis();
                input = null;
            }

            if (graphics != null)
            {
                var c = graphics;
                graphics = null;
                c.Dis();
            }

            created = false;
            swapping = false;
            GraphicContext.Terminate();
            Printer.Ln("---Core was successfully disposed");
            if (GlHelper.Debug)
                Errors.Check();
        }

        public static bool IsGLThread()
        {
            return glThread == Thread.CurrentThread;
        }

        public static Thread GLThread()
        {
            return glThread;
        }

        public static void DisposeClient()
        {
            new GlJob
            {
                DoJob = () =>
                {
                    foreach (var d in clientDisposables)
                    {
                        var s = GlHelper.GetErrors();
                        if (s != null)
                        {
                            new Exception($"{s} {d}").PrintStackTrace();
                        }
                        Printer.Ln($"---{d}");
                        d.Dis();
                        s = GlHelper.GetErrors();
                        if (s != null)
                        {
                            Console.Error.WriteLine(s);
                            new Exception(s).PrintStackTrace();
                        }
                    }
                    clientDisposables.Clear();
                }
            }.Perform();
        }
    }
}