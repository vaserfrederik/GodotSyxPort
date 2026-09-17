using Snake2D;
using Snake2D.Util.Sprite;
using Util.Gui.Misc;

namespace View.Ui.Top
{
    abstract class UIPanelTopButtAbs : GButt
    {
        private readonly GStat stat = new GStat
        {
            Update = (text) =>
            {
                GFORMAT.I(text, GetNumber());
                text.Lablify();
            }
        }.Decrease();

        public UIPanelTopButtAbs(Sprite icon, int width, int height) : base(icon)
        {
            Body.SetWidth(width);
            Body.SetHeight(height);
        }

        protected override void Render(SpriteRenderer r, float ds, bool isActive, bool isSelected, bool isHovered)
        {
            RenAction();

            GButt.ButtPanel.RenderBG(r, isActive, isSelected, isHovered, Body);
            GButt.ButtPanel.RenderFrame(r, isActive, isSelected, isHovered, Body);

            bool active = IsActive();
            if (active)
            {
                double cu = Value();
                double ta = ValueNext();
                GMeter.RenderSuperDelta(r, cu, ta, Body.X1() + 2, Body.X2() - 2, Body.Y1() + 2, Body.Y2() - 2, false);
            }

            stat.Adjust();
            Render(r, Label, stat, active);
        }

        public abstract void Render(SpriteRenderer r, Sprite label, GStat stat, bool active);

        protected abstract int GetNumber();

        protected abstract double Value();

        protected abstract double ValueNext();

        protected abstract bool IsActive();
    }
}