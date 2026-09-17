using System;
using OpenTK.Graphics.OpenGL;
using Snake2D.Util.File;

namespace Snake2D
{
    class Texture
    {
        readonly int id;
        readonly int width;
        readonly int height;

        private bool disposed = false;

        private static int MAX_SIZE = 0x04000;
        private readonly int ACTIVE_TEXTURE;

        public static Texture Normal(SnakeImage i, bool pixelated)
        {
            return new Texture(i, pixelated, TextureUnit.Texture1);
        }

        Texture(SnakeImage i, bool pixelated)
            : this(i, pixelated, TextureUnit.Texture0)
        {
        }

        Texture(SnakeImage i, bool pixelated, TextureUnit ACTIVE_TEXTURE)
        {
            GlHelper.CheckErrors();
            this.ACTIVE_TEXTURE = (int)ACTIVE_TEXTURE;
            width = i.width;
            height = i.height;

            if (width > MAX_SIZE || height > MAX_SIZE)
                throw new RuntimeException();

            id = GL.GenTexture();

            GL.ActiveTexture(ACTIVE_TEXTURE);
            GL.BindTexture(TextureTarget.TextureRectangle, id);

            // some strange filters. Experiment!
            GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, pixelated ? (int)TextureMagFilter.Nearest : (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.TextureRectangle, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, i.Data());

            int e = GL.GetError();
            if (e != ErrorCode.NoError)
            {
                GlHelper.DiagnoseMem();
                throw new RuntimeException("Texture Error " + width + " " + height + " " + e);
            }

            i.Dispose();
            GlHelper.CheckErrors();
        }

        void Bind()
        {
            if (disposed)
                throw new InvalidOperationException("trying to bind a texture that was disposed");
            else
            {
                GL.ActiveTexture((TextureUnit)ACTIVE_TEXTURE);
                GL.BindTexture(TextureTarget.TextureRectangle, id);
            }
        }

        void Dis()
        {
            GlHelper.CheckErrors();
            GL.ActiveTexture((TextureUnit)ACTIVE_TEXTURE);
            GL.DeleteTextures(id);
            GlHelper.CheckErrors();
            disposed = true;
        }

        public void UploadPixels(int px, int width, int py, int height, System.ByteBuffer pixels)
        {
            GL.ActiveTexture((TextureUnit)ACTIVE_TEXTURE);
            GL.TexSubImage2D(TextureTarget.TextureRectangle, 0, px, py, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        }

        public void UploadPixel(int px, int py, System.ByteBuffer pixel)
        {
            GL.ActiveTexture((TextureUnit)ACTIVE_TEXTURE);
            GL.TexSubImage2D(TextureTarget.TextureRectangle, 0, px, py, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixel);
        }

        public int GetWidth() => width;
        public int GetHeight() => height;
    }
}