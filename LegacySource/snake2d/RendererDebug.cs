using System;
using OpenTK.Graphics.OpenGL;
using snake2d.util.color;
using snake2d.util.light;
using snake2d.util.sprite;

namespace snake2d
{
    class RendererDebug : Renderer
    {
        private readonly VboSpriteOpti vbo;
        private readonly VboParticles vboParticles;
        private readonly _FBODebug fbo;
        private readonly bool debug;

        public RendererDebug(SETTINGS sett, int pointSize) : base(pointSize)
        {
            vbo = VboSpriteOpti.GetDebug(sett);
            fbo = new _FBODebug(sett);
            vboParticles = VboParticles.GetDebug(sett);
            GL.Disable(EnableCap.DepthTest);
            CheckErrors();
            this.debug = sett.debugMode();
        }

        public override void Dis()
        {
            CheckErrors();
            vbo.Dis();
            CheckErrors();
            vboParticles.Dis();
            CheckErrors();
            ElementArrays.Dispose();
            CheckErrors();
        }

        public override int PnewLayer(bool keepLights, int pointSize)
        {
            vboParticles.SetNew(pointSize);
            return vbo.SetNew();
        }

        public override int PnewFinalLightLayer(int pointSize)
        {
            vboParticles.SetNew(pointSize);
            return vbo.SetNew();
        }

        public override void RenderParticle(short x, short y, byte nx, byte ny, byte nz, byte nA, COLOR color, OPACITY opacity)
        {
            vboParticles.Render(x, y, nx, ny, nz, nA, color, opacity);
        }

        public override void Pclear(int pSize)
        {
            vbo.Clear();
            vboParticles.Clear(pSize);
        }

        public override void RenderSprite(TextureCoords t, TextureCoords to, int x1, int x2, int y1, int y2, COLOR color, OPACITY opacity)
        {
            vbo.Render(t, to, x1, x2, y1, y2, color, opacity);
        }

        public override void RenderShadow(TextureCoords t, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, byte d, byte depth)
        {
            // Implementation needed
        }

        public override void Pflush(int pSize)
        {
            Debug();
            fbo.BindAndClear();
            Debug();
            GL.Disable(EnableCap.DepthTest);
            Debug();
            vboParticles.Flush(pSize);
            Debug();
            vbo.Flush();
            Debug();
            // GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            fbo.BlitTexture();
            Debug();
        }

        private void Debug()
        {
            if (debug)
                CheckErrors();
        }

        public override void RegisterLight(LIGHT_POINT l, float x, float y, float z, int radius, int x1, int x2, int y1, int y2, byte ne, byte se, byte sw, byte nw, byte depth)
        {
            // Implementation needed
        }

        public override void RegisterAmbient(LIGHT_AMBIENT l, int x1, int x2, int y1, int y2, byte depth)
        {
            // Implementation needed
        }

        public override void RenderTilelight(int x1, int y1, int dim, byte nw, byte ne, byte se, byte sw)
        {
            // Implementation needed
        }

        public override void SetTileLight(LIGHT_AMBIENT l, byte depth)
        {
            // Implementation needed
        }

        public override void RenderPointlight(int x, int y, int z, int radius)
        {
            // Implementation needed
        }

        public override void SetPointLight(LIGHT_POINT l, byte depth)
        {
            // Implementation needed
        }

        public override void RenderDisplace(float tx1, float ty1, float dx1, float dy1, int w, int h, double scale, int x1, int x2, int y1, int y2, COLOR color, OPACITY opacity)
        {
            // Implementation needed
        }

        public override void PsetMaxDepth(int x1, int x2, int y1, int y2, TextureCoords stencil, int depth)
        {
            // Implementation needed
        }

        private void CheckErrors()
        {
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine("OpenGL error: " + error);
            }
        }
    }
}