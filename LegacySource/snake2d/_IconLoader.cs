using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace Snake2D
{
    public class _IconLoader
    {
        static void SetIcon(IntPtr window, string path)
        {
            string[] extens = new string[] {
                "Icon16", "Icon32", "Icon48"
            };

            List<SnakeImage> all = new List<SnakeImage>();
            IntPtr[] icons = new IntPtr[extens.Length];

            for (int i = 0; i < extens.Length; i++)
            {
                SnakeImage im = new SnakeImage(path + extens[i] + ".png");
                all.Add(im);
                icons[i] = im.DataPointer;
            }

            Glfw.SetWindowIcon(window, icons.Length, icons);

            for (int i = 0; i < icons.Length; i++)
            {
                GL.DeleteTexture(icons[i]);
            }

            foreach (SnakeImage im in all)
                im.Dispose();
        }
    }
}