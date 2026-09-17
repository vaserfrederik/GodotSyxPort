using System;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace snake2d
{
    public static class Errors
    {
        private static Errors i;
        private static ERROR_HANDLER h;
        private static readonly Logger outLogger = new Logger(Console.Out);
        private static readonly Logger errLogger = new Logger(Console.Error);

        public static void Init(ERROR_HANDLER handler)
        {
            if (i != null)
            {
                throw new InvalidOperationException("handler already setup");
            }
            i = new Errors(handler);
        }

        private Errors(ERROR_HANDLER handler)
        {
            Errors.h = handler;
            try
            {
                Console.SetOut(new StreamWriter(outLogger));
                Console.SetError(new StreamWriter(errLogger));
            }
            catch (Exception e)
            {
                e?.PrintStackTrace();
                throw new InvalidOperationException("can't set stdOut");
            }
        }

        public static void Handle(Exception e)
        {
            if (e == null)
            {
                Check();
                return;
            }

            e?.PrintStackTrace();
            DiagnoseMem();
            if (h == null)
                return;

            string dump = GetDumpFile();

            if (e is DataError dataError)
            {
                h.Handle(dataError, dump);
            }
            else if (e is GameError gameError)
            {
                h.Handle(gameError, dump);
            }
            else
            {
                h.Handle(e, dump);
            }
        }

        public static void Check()
        {
            if (HasDump())
            {
                string dd = i.errLogger.Data.ToString();
                string dump = GetDumpFile();
                if (dump != null && h != null)
                    h.Handle(dd, dump);
            }
        }

        private static bool HasDump()
        {
            return i.errLogger.Data.Length != 0;
        }

        private static string GetDumpFile()
        {
            if (i.errLogger.Data.Length != 0)
            {
                string n = Environment.NewLine;
                string fin = i.outLogger.Data.ToString();
                string err = i.errLogger.Data.ToString();

                fin = fin.Replace("\"", "'");
                err = err.Replace("\"", "'");

                string d = ""
                    + n + "|-------------------|"
                    + n + "|     ERROR LOG     |"
                    + n + "|-------------------|" 
                    + n + err
                    + n + ""
                    + n + "|-------------------|"
                    + n + "|      STD OUT      |"
                    + n + "|-------------------|" 
                    + n + fin;

                i.errLogger.Data.Clear();
                i.outLogger.Data.Clear();
                return d;
            }

            i.errLogger.Data.Clear();
            i.outLogger.Data.Clear();
            return null;
        }

        public static class DataError : Exception
        {
            public readonly string error;
            public readonly string path;

            public DataError(string error, string path) : base(error + " path: " + path)
            {
                this.error = error;
                this.path = path;
            }

            public DataError(string error, System.IO.Path sourcePath) : this(error, Path(sourcePath))
            {
            }

            private static string Path(System.IO.Path sourcePath)
            {
                if (sourcePath.Root != System.IO.Path.GetPathRoot(sourcePath))
                {
                    return sourcePath.Root + "->" + sourcePath.GetFullPath();
                }
                return sourcePath.GetFullPath();
            }

            public DataError(string error) : this(error, "")
            {
            }
        }

        public static class GameError : Exception
        {
            public readonly string error;

            public GameError(string error) : base(error)
            {
                this.error = error;
            }
        }

        private class Logger : Stream
        {
            private StringBuilder data = new StringBuilder();
            private readonly TextWriter outWriter;

            public Logger(TextWriter outWriter)
            {
                this.outWriter = outWriter;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                string str = Encoding.UTF8.GetString(buffer, offset, count);
                outWriter.Write(str);
                if (data.Length > 250000)
                    data.Clear();
                data.Append(str);
            }

            public override void Flush()
            {
                outWriter.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => data.Length;
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        }

        private static void DiagnoseMem()
        {
            int mb = 1024 * 1024;
            try
            {
                Console.Error.WriteLine("Time until crash: " + (CORE.GetUpdateInfo() == null ? 0 : CORE.GetUpdateInfo().SecondsSinceFirstUpdate));
                Console.Error.WriteLine("MEM DIAGNOSE");
                var run = GC.GetTotalMemory(false);
                // available memory
                Console.Error.WriteLine("--JRE Memory");
                Console.Error.WriteLine("--JRE Total: " + run / mb);
                Console.Error.WriteLine("--JRE Free: " + GC.GetTotalMemory(true) / mb);
                Console.Error.WriteLine("--JRE Used: " + (run - GC.GetTotalMemory(true)) / mb);
                Console.Error.WriteLine("--JRE Max: " + GC.GetTotalMemory(true) / mb);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.Error.WriteLine("--JRE Memory After GC");
                Console.Error.WriteLine("--JRE Total: " + GC.GetTotalMemory(false) / mb);
                Console.Error.WriteLine("--JRE Free: " + GC.GetTotalMemory(true) / mb);
                Console.Error.WriteLine("--JRE Used: " + (run - GC.GetTotalMemory(true)) / mb);
                Console.Error.WriteLine("--JRE Max: " + GC.GetTotalMemory(true) / mb);

                Console.Error.WriteLine("NVIDIA: ");
                int i;
                i = GL.GetInteger((GetPName)NVXGPUMemoryInfo.GL_GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX);
                Console.Error.WriteLine("--GPU Dedicated: " + i);
                i = GL.GetInteger((GetPName)NVXGPUMemoryInfo.GL_GPU_MEMORY_INFO_TOTAL_AVAILABLE_MEMORY_NVX);
                Console.Error.WriteLine("--GPU Total Available: " + i);
                i = GL.GetInteger((GetPName)NVXGPUMemoryInfo.GL_GPU_MEMORY_INFO_CURRENT_AVAILABLE_VIDMEM_NVX);
                Console.Error.WriteLine("--GPU Current Available: " + i);
                i = GL.GetInteger((GetPName)NVXGPUMemoryInfo.GL_GPU_MEMORY_INFO_EVICTION_COUNT_NVX);
                Console.Error.WriteLine("--GPU Evictions: " + i);
                i = GL.GetInteger((GetPName)NVXGPUMemoryInfo.GL_GPU_MEMORY_INFO_EVICTED_MEMORY_NVX);
                Console.Error.WriteLine("--GPU Evicted: " + i);

                Console.Error.WriteLine("ATI: ");
                i = GL.GetInteger((GetPName)ATIMeminfo.GL_RENDERBUFFER_FREE_MEMORY_ATI);
                Console.Error.WriteLine("--Renderbuffer Free: " + i);
                i = GL.GetInteger((GetPName)ATIMeminfo.GL_TEXTURE_FREE_MEMORY_ATI);
                Console.Error.WriteLine("--Texture Free: " + i);
                i = GL.GetInteger((GetPName)ATIMeminfo.GL_VBO_FREE_MEMORY_ATI);
                Console.Error.WriteLine("--Vbo Free: " + i);
                GL.GetError();
            }
            catch (Exception e)
            {
                e?.PrintStackTrace();
            }
        }
    }
}