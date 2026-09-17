using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;

namespace snake2d
{
    class GraphicsCardGetter
    {
        private string error = null;
        private string version = null;

        public GraphicsCardGetter()
        {
            try
            {
                // Setup an error callback. The default implementation
                // will print the error message in System.err.
                GLFW.ErrorCallback = GLFW.SetErrorCallback((code, description) =>
                {
                    Console.WriteLine($"Error: {description}");
                });

                // Initialize GLFW. Most GLFW functions will not work before doing this.
                if (!GLFW.Init())
                    throw new InvalidOperationException("Unable to initialize GLFW");

                // Configure GLFW
                GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
                GLFW.WindowHint(WindowHintBool.Visible, false); // the window will stay hidden after creation

                // Create the window
                using (var window = GLFW.CreateWindow(300, 300, "test", IntPtr.Zero, IntPtr.Zero))
                {
                    if (window == IntPtr.Zero)
                        throw new InvalidOperationException("No window returned");

                    // Make the OpenGL context current
                    GLFW.MakeContextCurrent(window);
                    GLFW.SwapInterval(1); // Enable vsync

                    GL.LoadAll();

                    version = GL.GetString(StringName.Vendor) + ", " + GL.GetString(StringName.Renderer) + Environment.NewLine
                             + "OpenGL max version: " + GL.GetString(StringName.Version);
                }

                GLFW.Terminate();
                GLFW.SetErrorCallback(null);
            }
            catch (Exception e)
            {
                e.printStackTrace();
                error = e.Message;
            }
        }

        public string Version()
        {
            return version;
        }

        public string Error()
        {
            return error;
        }
    }
}