using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Snake2D.Util.File
{
    public sealed class SnakeImage
    {
        private IntPtr image;
        public readonly int Height;
        public readonly int Width;
        public readonly ImageGraphics Rgb = new ImageGraphics();
        public readonly string Path;

        public SnakeImage(string path)
        {
            this.Path = path;
            if (!File.Exists(path))
                throw new DataError("File doesn't exist", path);

            Int32 width, height, components;
            image = STBImage.STBI_Load(path, out width, out height, out components, 4);

            if (image == IntPtr.Zero)
                throw new DataError("Failed to load a texture file!\n" + STBImage.STBI_FailureReason(), path);

            Width = width;
            Height = height;
        }

        public SnakeImage(string path, int width, int height) : this(path)
        {
            if (this.Width != width || this.Height != height)
            {
                Dispose();
                throw new IOException("Image has wrong dimensions. Resize to: " + width + "x" + height + " " + path);
            }
        }

        public SnakeImage(int width, int height)
        {
            Path = null;
            image = Marshal.AllocHGlobal(width * height * 4);
            if (image == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            Width = width;
            Height = height;
        }

        public IntPtr Data()
        {
            Marshal.Copy(image, 0, image, Width * Height * 4);
            return image;
        }

        public void Dispose()
        {
            if (Path == null)
            {
                return;
            }
            if (image != IntPtr.Zero)
            {
                STBImage.STBI_Free(image);
                image = IntPtr.Zero;
            }
        }

        public void Save(string path)
        {
            Marshal.Copy(image, 0, image, Width * Height * 4);
            STBImageWrite.STBI_WritePng(path, Width, Height, 4, image, 0);
        }

        public SnakeImage Resized(int nwidth, int nheight)
        {
            SnakeImage nn = new SnakeImage(nwidth, nheight);
            Marshal.Copy(image, 0, image, Width * Height * 4);
            Marshal.Copy(nn.image, 0, nn.image, nwidth * nheight * 4);
            STBImageResize.STBIR_ResizeUInt8Linear(image, Width, Height, 0, nn.image, nwidth, nheight, 0, 4);
            return nn;
        }

        public void SaveJpg(string path)
        {
            Marshal.Copy(image, 0, image, Width * Height * 4);
            STBImageWrite.STBI_WriteJpg(path, Width, Height, 4, image, 90);
        }

        public sealed class ImageGraphics
        {
            private ImageGraphics() { }

            private void BoundCheck(int x, int y, SnakeImage image)
            {
                if (x < 0 || y < 0 || x >= image.Width || y >= image.Height)
                    throw new Exception(x + " " + y + " is out of bounds " + image.Width + " " + image.Height + " " + image.Path);
            }

            public void Set(int x, int y, int r, int g, int b, int a, SnakeImage image)
            {
                BoundCheck(x, y, image);
                int i = 4 * (x + y * image.Width);
                Marshal.WriteByte(image.image, i, (byte)r);
                Marshal.WriteByte(image.image, i + 1, (byte)g);
                Marshal.WriteByte(image.image, i + 2, (byte)b);
                Marshal.WriteByte(image.image, i + 3, (byte)a);
            }

            public void Set(int x, int y, int c, SnakeImage image)
            {
                int r = (c >> 24) & 0x0FF;
                int g = (c >> 16) & 0x0FF;
                int b = (c >> 8) & 0x0FF;
                int a = (c) & 0x0FF;
                Set(x, y, r, g, b, a, image);
            }

            public int Get(int x, int y, SnakeImage image)
            {
                BoundCheck(x, y, image);
                int i = 4 * (x + y * image.Width);
                int res = (Marshal.ReadByte(image.image, i) & 0x0FF) << 24;
                res |= (Marshal.ReadByte(image.image, i + 1) & 0x0FF) << 16;
                res |= (Marshal.ReadByte(image.image, i + 2) & 0x0FF) << 8;
                res |= (Marshal.ReadByte(image.image, i + 3) & 0x0FF);
                return res;
            }
        }
    }
}