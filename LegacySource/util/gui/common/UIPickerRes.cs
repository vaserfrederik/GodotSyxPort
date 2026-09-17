using System.Collections.Generic;
using Init.Resources;
using Init.Sprite;
using Snake2D.Util.Gui;
using Snake2D.Util.Sets;
using Util.Gui.Misc;
using Util.Text;

namespace Util.Gui.Common
{
    public abstract class UIPickerRes : GuiSection
    {
        public UIPickerRes()
            : this(RESOURCES.ALL(), false)
        {
        }

        public UIPickerRes(bool includenull)
            : this(RESOURCES.ALL(), includenull)
        {
        }

        public UIPickerRes(LIST<RESOURCE> list, bool includenull)
        {
            int i = 0;
            if (includenull)
            {
                Add(new Resbutt(null, i++));
            }
            foreach (RESOURCE r in list)
            {
                Resbutt rb = new Resbutt(r, i);
                rb.Body.MoveX1Y1(rb.Body.Width() * (i % 8), rb.Body.Height() * (i / 8));
                Add(rb);
                i++;
            }
        }

        protected abstract RESOURCE GetResource();
        protected abstract void Select(RESOURCE r, int li);
        protected void HoverResource(RESOURCE r, GBox b)
        {
            b.Title(r.Name);
            b.Text(r.Desc);
            b.NL();
        }

        private class Resbutt : GButt.ButtPanel
        {
            private readonly RESOURCE Res;
            private readonly int I;

            public Resbutt(RESOURCE res, int i)
                : base(res == null ? SPRITES.icons().m.cancel : res.Icon())
            {
                Res = res;
                I = i;
                Pad(4, 4);
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                if (Res == null)
                    text.Text(Dic.¤¤cancel);
                else
                    ((UIPickerRes)Parent).HoverResource(Res, (GBox)text);
            }

            protected override void ClickA()
            {
                Select(Res, I);
            }

            protected override void RenAction()
            {
                SelectedSet(((UIPickerRes)Parent).GetResource() == Res);
            }
        }
    }
}