using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Snake2D.Util.Color;
using Snake2D.Util.File;
using Snake2D.Util.Gl;

namespace Snake2D
{
    /**
     * A spritesheet
     * @author mail__000
     *
     */
    public class TextureHolder : CoreResource
    {
        public readonly int PixelWidth;
        public readonly int PixelHeight;

        private readonly Texture texture;
        private readonly Texture normalTexture;
        private readonly VboParticles pixels;
        private readonly int FBO;
        private TextureHolderChunk chunk;

        /**
         * 
         * @param diffusePath the path to the png image
         */
        public TextureHolder(SnakeImage diffuse, SnakeImage normal, int x1, int y1, int w, int h)
        {
            if (!Core.IsGLThread())
                throw new Exception();
            GlHelper.CheckErrors();
            texture = new Texture(diffuse, true);

            if (normal != null)
                normalTexture = Texture.Normal(normal, true);
            else
                normalTexture = null;

            PixelWidth = texture.Width;
            PixelHeight = texture.Height;

            texture.Bind();
            if (normalTexture != null)
                normalTexture.Bind();

            Core.AddDisposable(this);
            ColorImp.SetSprite(x1, y1, w, h);

            pixels = VboParticles.GetForTexture(PixelWidth, PixelHeight);

            FBO = GL.GenFramebuffer();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureRectangle, texture.Id, 0);

            GL.DrawBuffers(BufferTarget.Framebuffer, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Could not create fbo");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GlHelper.CheckErrors();
        }

        void Flush()
        {
            if (pixels.Count > 0)
            {
                GlHelper.ViewPort.Set(PixelWidth, PixelHeight);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
                GL.DrawBuffers(BufferTarget.Framebuffer, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });
                GlHelper.Stencil.Enable(true);
                pixels.Flush(1);
                GlHelper.ViewPort.SetDefault();
            }

            if (chunk != null)
            {
                GlHelper.CheckErrors();
                ByteBuffer drawBuff = chunk.Buff;
                drawBuff.Flip();
                texture.UploadPixels(chunk.X1, chunk.W, chunk.Y1, chunk.Am / (chunk.W), drawBuff);
                chunk = null;
                GlHelper.CheckErrors();
            }
        }

        override void Dis()
        {
            GlHelper.CheckErrors();
            texture.Dis();
            GlHelper.CheckErrors();
            if (normalTexture != null)
                normalTexture.Dis();
            GlHelper.CheckErrors();
            pixels.Dis();
            GL.DeleteFramebuffers(1, ref FBO);
            GlHelper.CheckErrors();
        }

        public void PutPixel(int x, int y, byte r, byte g, byte b)
        {
            pixels.Render((short)x, (short)(PixelHeight - y), r, g, b);
        }

        public void PutPixelBatch(int x1, int y1, int width, byte[] pixels)
        {
            ByteBuffer drawBuff = ByteBuffer.AllocateDirect(pixels.Length);

            foreach (byte b in pixels)
            {
                drawBuff.Put(b);
            }
            drawBuff.Flip();
            new Core.GlJob
            {
                DoJob = () =>
                {
                    texture.UploadPixels(x1, width, y1, pixels.Length / (width * 4), drawBuff);
                }
            }.Perform();
        }

        public void AddChunk(int x1, int y1, int width, int am, TextureHolderChunk chunk)
        {
            chunk.X1 = x1;
            chunk.Y1 = y1;
            chunk.W = width;
            chunk.Am = am;
            this.chunk = chunk;
        }

        public class TextureHolderChunk
        {
            public readonly int Width, Height;
            private readonly ByteBuffer buff;
            private int x1, y1, w, am;

            public TextureHolderChunk(int width, int height)
            {
                this.Width = width;
                this.Height = height;
                buff = ByteBuffer.AllocateDirect(width * height * 4);
            }

            public void Put(int i, byte r, byte g, byte b, byte a)
            {
                buff.Position(i * 4);
                buff.Put(r).Put(g).Put(b).Put(a);
            }

            public void Put(int i, Color c)
            {
                buff.Position(i * 4);
                buff.Put((byte)(c.Red * 2)).Put((byte)(c.Green * 2)).Put((byte)(c.Blue * 2)).Put((byte)0xFF);
            }
        }
    }
}