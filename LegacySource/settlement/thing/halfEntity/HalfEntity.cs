using System;
using System.IO;

namespace Settlement.Thing.HalfEntity
{
    public abstract class HalfEntity : BODY_HOLDER
    {
        //int managerArrayIndex;
        private readonly Rec hitBox;
        private HalfEntity renderNext;
        private short gridX, gridY;
        private int index = -1;

        public HalfEntity(int hitBoxWidth, int hitBoxHeight)
        {
            hitBox = new Rec(0, hitBoxWidth, 0, hitBoxHeight);
        }

        protected abstract void Save(FilePutter file);

        protected abstract HalfEntity Load(FileGetter file);

        public final RECTANGLEE Body()
        {
            return hitBox;
        }

        protected final void Add()
        {
            renderNext = null;
            SETT.HALFENTS().Add(this);
            AddAction();
        }

        protected final void Remove()
        {
            SETT.HALFENTS().Remove(this);

            RemoveAction();
        }

        public final bool Added()
        {
            return index != -1;
        }

        public abstract void HoverInfo(GBox box);

        protected void RenderBelow(Renderer r, ShadowBatch s, float ds, int x, int y)
        {

        }

        protected abstract void Render(Renderer r, ShadowBatch s, float ds, int x, int y);

        protected void RenderAbove(Renderer r, ShadowBatch s, float ds, int x, int y)
        {

        }


        protected void AddAction()
        {

        }
        protected void RemoveAction()
        {

        }

        protected abstract void Update(double ds);


        public int GetZ()
        {
            return 0;
        }


        public int Index()
        {
            return index;
        }

        protected abstract Factory<? extends HalfEntity> Constructor();

        public int Ctx()
        {
            return Body().CX() >> C.T_SCROLL;
        }

        public int Cty()
        {
            return Body().CY() >> C.T_SCROLL;
        }
    }
}