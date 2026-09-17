using System;
using OpenTK.Graphics.OpenGL;
using snake2d.util.color;
using snake2d.util.light;
using snake2d.util.sprite;
using snake2d.util.opengl;

namespace snake2d
{
    class RendererDeffered : Renderer
    {
        private readonly VboSpriteOpti vboTextured;
        private readonly VboShadowPoints vboShadows;
        private readonly VboLightAmbient vboLightAmbient;
        private readonly VboTileLight vboTileLight;
        private readonly VboLightPoint vboLightPoint;
        private readonly VboParticles vboParticles;
        private readonly _FBODeffered fbo;
        private readonly VboSpriteDisplace vboDisplace2;
        private readonly VboLightPointUni vboLightPointUni;
        private readonly VboStencilMaxSetter depth;
        private readonly bool debug;

        public RendererDeffered(SETTINGS sett, int pointSize) : base(pointSize)
        {
            GlHelper.CheckErrors();
            Printer.Ln("SHADERS: ");
            vboTextured = VboSpriteOpti.GetDeffered(sett);
            vboShadows = new VboShadowPoints(sett);
            vboLightAmbient = new VboLightAmbient(sett);
            vboLightPoint = new VboLightPoint(sett);
            vboParticles = VboParticles.GetDeffered(sett);
            vboTileLight = new VboTileLight(sett);
            vboDisplace2 = VboSpriteDisplace.GetDeffered(sett);
            vboLightPointUni = new VboLightPointUni(sett);
            depth = new VboStencilMaxSetter(sett);
            fbo = new _FBODeffered(sett);
            debug = sett.debugMode();
            GlHelper.Stencil.Enable(true);
            GlHelper.CheckErrors();
            Printer.Fin();
        }

        protected override void Dis()
        {
            GlHelper.CheckErrors();
            vboTextured.Dis();
            GlHelper.CheckErrors();
            vboShadows.Dis();
            GlHelper.CheckErrors();
            fbo.Dis();
            GlHelper.CheckErrors();
            vboLightAmbient.Dis();
            GlHelper.CheckErrors();
            vboLightPoint.Dis();
            GlHelper.CheckErrors();
            vboParticles.Dis();
            GlHelper.CheckErrors();
            vboTileLight.Dis();
            GlHelper.CheckErrors();
            vboDisplace2.Dis();
            GlHelper.CheckErrors();
            vboLightPointUni.Dis();
            GlHelper.CheckErrors();
            depth.Dis();
            GlHelper.CheckErrors();
            ElementArrays.Dispose();
            GlHelper.CheckErrors();
        }

        protected override int PnewLayer(bool keepLights, int pointSize)
        {
            if (keepLights)
            {
                vboLightAmbient.SetNewButKeepLight();
                vboLightPoint.SetNewButKeepLight();
                vboTileLight.SetNewButKeepLight();
                vboLightPointUni.SetNewButKeepLight();
            }
            else
            {
                vboLightAmbient.SetNew();
                vboLightPoint.SetNew();
                vboTileLight.SetNew();
                vboLightPointUni.SetNew();
            }
            vboParticles.SetNew(pointSize);
            int i = vboTextured.SetNew();
            vboDisplace2.SetNew();
            vboShadows.SetNewFinalOverride(i);
            depth.SetNewFinalOverride(i);
            return i;
        }

        protected override int PnewFinalLightLayer(int pointSize)
        {
            vboLightAmbient.SetNewFinal();
            vboLightPoint.SetNewFinal();
            vboTileLight.SetNewFinal();
            vboLightPointUni.SetNewFinal();
            vboParticles.SetNew(pointSize);

            int i = vboTextured.SetNew();
            vboDisplace2.SetNew();
            vboShadows.SetNewFinalOverride(i);
            depth.SetNewFinalOverride(i);
            return i;
        }

        protected override void Pclear(int pointSize)
        {
            vboTextured.Clear();
            vboShadows.Clear();
            vboLightAmbient.Clear();
            vboLightPoint.Clear();
            vboTileLight.Clear();
            vboParticles.Clear(pointSize);
            vboDisplace2.Clear();
            vboLightPointUni.Clear();
            depth.Clear();
        }

        public override void RenderShadow(TextureCoords t, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, byte d, byte depth)
        {
            vboShadows.Render(t, x1, y1, x2, y2, x3, y3, x4, y4, d, depth);
        }

        public override void RenderSprite(TextureCoords t, TextureCoords to, int x1, int x2, int y1, int y2, COLOR color, OPACITY opacity)
        {
            vboTextured.Render(t, to, x1, x2, y1, y2, color, opacity);
        }

        public override void RegisterLight(LIGHT_POINT l, float x, float y, float z, int radius, int x1, int x2, int y1, int y2, byte ne, byte se, byte sw, byte nw, byte depth)
        {
            vboLightPoint.Render(l, x, y, z, radius, x1, x2, y1, y2, ne, se, sw, nw, depth);
        }

        public override void RegisterAmbient(LIGHT_AMBIENT l, int x1, int x2, int y1, int y2, byte depth)
        {
            vboLightAmbient.Render(l, x1, x2, y1, y2, depth);
        }

        public override void RenderParticle(short x, short y, byte nx, byte ny, byte nz, byte nA, COLOR color, OPACITY opacity)
        {
            vboParticles.Render(x, y, nx, ny, nz, nA, color, opacity);
        }

        private int di = 0;

        protected override void Pflush(int pointSize)
        {
            Debug();
            fbo.BindDiffAndNorForTarget();
            Debug();

            Debug();
            vboParticles.Flush(pointSize);
            Debug();
            Debug();
            vboTextured.Flush();
            Debug();
            vboDisplace2.Flush();
            Debug();
            depth.Flush();
            vboShadows.Flush();

            Debug();

            Debug();
            fbo.BindLightTextureForTarget();
            Debug();
            vboLightAmbient.Flush();
            Debug();
            vboLightPoint.Flush();
            Debug();
            vboLightPointUni.Flush();
            Debug();
            vboTileLight.Flush();
            Debug();
            fbo.BlitTexture();
            Debug();
            GL.UseProgram(0); // puts an end to the goddamn nvidia errors
        }

        private void Debug()
        {
            if (debug && di++ >= 1000)
            {
                di = 0;
                GlHelper.CheckErrors();
            }
        }

        public override void RenderTilelight(int x1, int y1, int dim, byte nw, byte ne, byte se, byte sw)
        {
            vboTileLight.Render(x1, y1, dim, nw, ne, se, sw);
        }

        public override void SetTileLight(LIGHT_AMBIENT l, byte depth)
        {
            vboTileLight.SetLight((float)l.r(), (float)l.g(), (float)l.b(), l.x(), l.y(), l.z(), depth);
        }

        public override void RenderPointlight(int x, int y, int z, int radius)
        {
            vboLightPointUni.Render((short)x, (short)y, (short)z, (short)radius);
        }

        public override void SetPointLight(LIGHT_POINT l, byte depth)
        {
            vboLightPointUni.SetLight(l.getRadius(), l.getRed(), l.getGreen(), l.getBlue(), l.getFalloff(), depth);
        }

        public override void RenderDisplace(float tx1, float ty1, float dx1, float dy1, int w, int h, double scale, int x1, int x2, int y1, int y2, COLOR color, OPACITY opacity)
        {
            vboDisplace2.Render(tx1, ty1, dx1, dy1, w, h, scale, x1, x2, y1, y2, color, opacity);
        }

        public override void PsetMaxDepth(int x1, int x2, int y1, int y2, TextureCoords stencil, int depth)
        {
            this.depth.Render(stencil, x1, y1, x2, y2, depth);
        }
    }
}