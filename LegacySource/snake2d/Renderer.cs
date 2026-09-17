using System;

namespace Snake2D
{
    public abstract class Renderer : CoreResource, ISpriteRenderer
    {
        protected int zoomout = 0;
        protected readonly int pointSize;

        private readonly byte noShadows = 255;
        private readonly byte shadows = 0;
        private byte shadowDepth = noShadows;
        private byte lightDepth = shadows;

        private int layer = 0;

        protected Renderer(int pointSize)
        {
            this.pointSize = pointSize;
            zoomout = 0;
        }

        private void Flush()
        {
            PFlush(pointSize >> zoomout);
            Clear();
        }

        protected abstract void PFlush(int pz);

        private int spritesRendered = 0;
        private int shadowsRendered = 0;
        private int particlesRendererd = 0;
        private int lightsRendererd = 0;
        private int shadowsRenderedO = 0;
        private int spritesRenderedO = 0;
        private int particlesRendererdO = 0;
        private int lightsRendererdO = 0;

        private readonly Color white = ColorImp.WHITE100;
        private Color current = white;
        private readonly Opacity OpacityDefault = OpacityImp.O100;
        private Opacity currentOpacity = OpacityDefault;

        public int Pointsize()
        {
            return pointSize >> zoomout;
        }

        public int GetSpritesSprocessed()
        {
            return spritesRenderedO;
        }

        public int GetParticlesProcessed()
        {
            return particlesRendererdO;
        }

        public int GetLightsProcessed()
        {
            return lightsRendererdO;
        }

        public int GetShadowsRendered()
        {
            return shadowsRenderedO;
        }

        public void SetColor(Color color)
        {
            current = color;
        }

        public Color ColorGet()
        {
            return current;
        }

        public Color GetBoundColor()
        {
            return current;
        }

        public void SetNormalColor()
        {
            current = white;
        }

        public void SetOpacity(Opacity o)
        {
            currentOpacity = o;
        }

        public void SetNormalOpacity()
        {
            currentOpacity = OpacityDefault;
        }

        public bool IsNormalOpacity()
        {
            return currentOpacity == OpacityDefault;
        }

        public void ShadeLight(byte depth)
        {
            if (depth == shadows)
            {
                lightDepth = shadows;
                shadowDepth = noShadows;
            }
            else
            {
                lightDepth = noShadows;
                shadowDepth = depth;
            }
        }

        public void RenderTileLight(int x1, int y1, int dim, byte nw, byte ne, byte se, byte sw)
        {
            if (zoomout != 0)
            {
                x1 = x1 >> zoomout;
                y1 = y1 >> zoomout;
                dim = dim >> zoomout;
            }
            RenderTilelight(x1, y1, dim, nw, ne, se, sw);
        }

        public void SetTileLight(AmbientLight l)
        {
            SetTileLight(l, shadowDepth);
        }

        protected abstract void SetTileLight(LIGHT_AMBIENT l, byte depth);

        protected abstract void RenderPointlight(int x, int y, int z, int radius);

        protected abstract void SetPointLight(LIGHT_POINT l, byte depth);

        protected abstract void RenderTilelight(int x1, int y1, int dim, byte nw, byte ne, byte se, byte sw);

        public void RenderUniLight(int x, int y, int z, int radius)
        {
            if (zoomout != 0)
            {
                x = x >> zoomout;
                y = y >> zoomout;
                z = z >> zoomout;
                radius = radius >> zoomout;
            }
            RenderPointlight(x, y, z, radius);
        }

        public void SetUniLight(LIGHT_POINT l)
        {
            SetPointLight(l, shadowDepth);
        }

        public void SetZoom(int i)
        {
            zoomout = i;
        }
    }
}