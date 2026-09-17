using System;
using OpenTK.Graphics.OpenGL;

namespace snake2d
{
    internal class _FBOBlitter
    {
        private readonly bool mul;
        private int ID;
        private int textureID;
        private int blitFilter;
        private bool scaling;
        private int width;
        private int height;
        private bool debug;

        public _FBOBlitter(SETTINGS sett)
        {
            mul = (sett.getNativeWidth() != sett.display().width || sett.getNativeHeight() != sett.display().height) && GL.GetInteger(GetPName.Samples) > 1;
            this.width = sett.getNativeWidth();
            this.height = sett.getNativeHeight();

            if (mul)
            {
                Console.WriteLine("This machine has forced multiple sampling enabled. Some performance penalties will ensue. Disable overriding sampling (MSAA, antialiasing) for better performance.");
                ID = GL.GenFramebuffers();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

                textureID = GlHelper.getFBTexture(sett.display().width, sett.display().height);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureID, 0);

                if (FramebufferErrorCode.FramebufferComplete != GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer))
                    throw new Exception("Could not create fbo");

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            blitFilter = sett.getLinearFiltering() ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest;
            scaling = sett.getFitToScreen();
            GlHelper.checkErrors();
            // TODO Auto-generated constructor stub
            debug = sett.debugMode();
        }

        private int blitI = 0;

        public void blit(int fbID)
        {
            int blitX;
            int blitY;
            int blitW;
            int blitH;
            int dw = CORE.getGraphics().blitArea.x();
            int dy = CORE.getGraphics().blitArea.y();

            GlHelper.ViewPort.set(dw, dy);

            if (scaling)
            {
                blitX = 0;
                blitY = 0;
                blitW = dw;
                blitH = dy;
            }
            else
            {
                double d = (double)dw / width;
                if (height * d > dy)
                    d = (double)1.0;
                blitW = (int)(width * d);
                blitH = (int)(height * d);
                if (blitW > dw)
                    blitW = dw;
                if (blitH > dy)
                    blitH = dy;
                blitX = (dw - blitW) / 2;
                blitY = (dy - blitH) / 2;
            }

            if (debug && blitI-- < 0)
            {
                blitI = 1000;
                //Printer.ln("BLITTING " + scaling + " " + blitX + " " + blitW + " " + blitY + " " + blitH + " " + mul);
            }

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbID);
            if (mul)
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, ID);
                GL.BlitFramebuffer(0, 0, width, height, blitX, blitY, blitX + blitW, blitY + blitH, BufferBits.ColorBufferBit, (BlitFramebufferFilter)blitFilter);

                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, ID);
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

                GL.BlitFramebuffer(0, 0, dw, dy, 0, 0, dw, dy, BufferBits.ColorBufferBit, BlitFramebufferFilter.Nearest);
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                GL.BlitFramebuffer(0, 0, width, height, blitX, blitY, blitX + blitW, blitY + blitH, BufferBits.ColorBufferBit, (BlitFramebufferFilter)blitFilter);
            }
            GlHelper.ViewPort.setDefault();
        }

        public void dis()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.DeleteTextures(textureID);
            GlHelper.checkErrors();
        }
    }
}