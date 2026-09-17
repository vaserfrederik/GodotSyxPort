using System;

namespace Game.Battle.Formation
{
    public class FormationBody : Rec
    {
        public bool Init(DivPosition d)
        {
            Clear();

            int cx = 0;
            int cy = 0;
            int am = 0;
            for (int i = 0; i < d.Deployed(); i++)
            {
                int x = d.Px(i);
                int y = d.Py(i);
                cx += x;
                cy += y;
                am++;
            }

            if (am == 0)
                return false;

            cx /= am;
            cy /= am;

            int width = 0;
            int height = 0;
            am = 0;
            for (int i = 0; i < d.Deployed(); i++)
            {
                int x = d.Px(i);
                int y = d.Py(i);

                width += Math.Abs(cx - x);
                height += Math.Abs(cy - y);
                am++;
            }

            if (am == 0)
                return false;

            width /= am;
            height /= am;
            width *= 4;
            height *= 4;
            SetDim(width, height);
            MoveC(cx, cy);

            return true;
        }
    }
}