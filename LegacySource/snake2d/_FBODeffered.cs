using System;
using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace snake2d
{
    class _FBODeffered : CORE_RESOURCE
    {
        private int width;
        private int height;
        private _FBOBlitter blitter;

        private readonly int fbID;
        private int iddiffuse;
        private int idNormal;
        private int idLight;
        private readonly int stencilID;

        private readonly int[] diffuseNormalBuffer;

        _FBODeffered(SETTINGS sett)
        {
            this.width = sett.getNativeWidth();
            this.height = sett.getNativeHeight();
            blitter = new _FBOBlitter(sett);
            diffuseNormalBuffer = new int[2] { (int)FramebufferAttachment.ColorAttachment1, (int)FramebufferAttachment.ColorAttachment2 };
            fbID = GL.GenFramebuffer();
            stencilID = GL.GenRenderbuffer();
            generateTextures();
        }

        void applySettings(SETTINGS sett)
        {
            if (this.width != sett.getNativeWidth() || this.height != sett.getNativeHeight())
            {
                this.width = sett.getNativeWidth();
                this.height = sett.getNativeHeight();
                deleteTextures();
                generateTextures();
            }
        }

        private void generateTextures()
        {
            GlHelper.checkErrors();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbID);

            iddiffuse = GlHelper.getFBTexture(width, height);
            idNormal = GlHelper.getFBTexture(width, height);
            idLight = GlHelper.getFBTexture(width, height);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, iddiffuse, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, idNormal, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, idLight, 0);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, stencilID);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.Depth24Stencil8, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, stencilID);

            GL.DrawBuffers(2, diffuseNormalBuffer);
            GlHelper.checkErrors();

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new RuntimeException("Could not create fbo " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, iddiffuse);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, idNormal);

            GlHelper.checkErrors();
        }

        private void deleteTextures()
        {
            GL.DeleteTexture(iddiffuse);
            GL.DeleteTexture(idNormal);
            GL.DeleteTexture(idLight);
        }

        public override void dis()
        {
            GlHelper.checkErrors();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            deleteTextures();
            GL.DeleteRenderbuffer(stencilID);
            GL.DeleteFramebuffer(fbID);
            blitter.dis();
            GlHelper.checkErrors();
        }

        public void bindDiffAndNorForTarget()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbID);
            GL.DrawBuffers(2, diffuseNormalBuffer);
            GlHelper.ViewPort.set(width, height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void bindLightTextureForTarget()
        {
            GL.DrawBuffers(1, new int[] { (int)FramebufferAttachment.ColorAttachment0 });
            GlHelper.ViewPort.set(width, height);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void blitTexture()
        {
            blitter.blit(fbID);
        }
    }
}