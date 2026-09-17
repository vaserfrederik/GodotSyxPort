using System;
using System.IO;
using game.faction;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using util.rendering;

namespace world.entity
{
    public abstract class WEntity : BODY_HOLDER
    {
        //int managerArrayIndex;
        private readonly Rec hitBox;
        public WEntity renderNext;
        public WEntity regionNext;
        public short gridX, gridY;
        public short regionI = -1;
        public int index = -1;

        public WEntity(int hitBoxWidth, int hitBoxHeight)
        {
            hitBox = new Rec(0, hitBoxWidth, 0, hitBoxHeight);
        }

        protected abstract void save(FilePutter file);

        protected abstract WEntity load(FileGetter file);

        public final RECTANGLEE body()
        {
            return hitBox;
        }

        protected final void add()
        {
            renderNext = null;
            regionNext = null;
            regionI = -1;
            ENTITIES().add(this);
            addAction();
        }

        protected final void remove()
        {
            ENTITIES().remove(this);
            removeAction();
        }

        public final bool added()
        {
            return index != -1;
        }

        public final int index()
        {
            return index;
        }

        protected void renderGround(Renderer r, ShadowBatch s, float ds, int x, int y)
        {

        }

        protected abstract void renderBelowTerrain(Renderer r, ShadowBatch s, float ds, int x, int y);

        protected abstract void renderAboveTerrain(Renderer r, ShadowBatch s, float ds, int x, int y);

        protected void handleFow()
        {

        }

        protected virtual void addAction()
        {

        }

        protected virtual void removeAction()
        {

        }

        protected abstract void update(double ds);

        public int getZ()
        {
            return 0;
        }

        protected abstract WEntityConstructor<? extends WEntity> constructor();

        public short ctx()
        {
            return (short)(body().cX() >> C.T_SCROLL);
        }

        public short cty()
        {
            return (short)(body().cY() >> C.T_SCROLL);
        }

        public Faction faction()
        {
            return null;
        }

        public world.map.pathing.WPath path()
        {
            return null;
        }
    }
}