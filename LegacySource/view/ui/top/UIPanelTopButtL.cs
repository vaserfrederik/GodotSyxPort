using Snake2D;
using Util.Gui.Misc;
using Util.Color;
using Util.Sprite;

namespace View.Ui.Top
{
    public abstract class UIPanelTopButtL : UIPanelTopButtAbs
    {
        public UIPanelTopButtL(Sprite icon) : base(icon, 60, 48)
        {
        }

        protected override void Render(SpriteRenderer r, Sprite label, GStat stat, bool active)
        {
            Color.Black.Bind();
            label.RenderC(r, Body().CX + 1, Body.Y1 + 17);
            Color.Unbind();
            label.RenderC(r, Body().CX, Body.Y1 + 16);

            if (active)
            {
                Opacity.O50.Bind();

                int w = stat.Width;

                int y2 = Body.Y2 - 6;
                int y1 = y2 - stat.Height;

                int x1 = Body.CX - w / 2;
                int x2 = x1 + w;

                Color.Black.Render(r, x1 - 2,
                        x2 + 2, y1 - 1, y2 + 1);
                Opacity.Unbind();
                stat.Render(r, x1, y1);
            }
        }
    }
}