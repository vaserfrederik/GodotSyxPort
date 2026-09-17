using System.Collections.Generic;
using game;
using init.constant;
using snake2d;
using snake2d.util.light;
using snake2d.util.sets;
using util.rendering;

namespace settlement.main
{
    public abstract class ON_TOP_RENDERABLE
    {
        private static readonly ArrayList<ON_TOP_RENDERABLE> renderables = new ArrayList<ON_TOP_RENDERABLE>(64);

        static ON_TOP_RENDERABLE()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    renderables.Clear();
                }
            };
        }

        private bool isAdded = false;

        public void Render(ShadowBatch shadowBatch, RenderData data, int zoomout, double ds)
        {
            CORE.Renderer().NewLayer(false, zoomout);
            AmbientLight.Full.Register(0, C.WIDTH() << zoomout, 0, C.HEIGHT() << zoomout);
            Render(CORE.Renderer(), shadowBatch, data, ds);
        }

        protected abstract void Render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds);

        public void Add()
        {
            if (isAdded)
                return;
            renderables.Add(this);
            isAdded = true;
        }

        public void Remove()
        {
            if (!isAdded)
                return;
            renderables.Remove(this);
            isAdded = false;
        }

        public bool IsAdded()
        {
            return isAdded;
        }
    }
}