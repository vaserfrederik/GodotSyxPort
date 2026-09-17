using System;
using System.IO;
using snake2d;
using util.rendering;
using view.sett;

namespace settlement.entity
{
    public abstract class ENTITY : BODY_HOLDER, SETT_HOVERABLE
    {
        int handlerId = -1;
        public readonly ESpeed.Imp speed = new ESpeed.Imp();
        public readonly EPHYSICS.Solid physics = new EPHYSICS.Solid();
        private ENTITY next;
        private ENTITY prev;
        short gx = -1, gy = -1;

        /**
         * renders this entity at its current position with the adjustment of the offsets.
         * @param ds seconds passed
         * @param offsetX
         * @param offsetY
         */
        public abstract void render(Renderer r, ShadowBatch shadows, float ds, int offsetX, int offsetY);

        public abstract void renderSimple(Renderer r, ShadowBatch shadows, float ds, int offsetX, int offsetY);

        public override RECTANGLE body()
        {
            return physics.body();
        }

        /**
         * 
         * @param old - if it's with another entity
         * @param pierceDamage - the damage as returned by collideDamage, or piercing damage
         * @param data.norX - the direction from other entity to you
         * @param data.norY - the direction from other entity to you
         * @param speedDot - the dot product 0-1 of the direction of collision and your speeds direction before the collision
         * @param force - the exchanged force, expressed in your own mass.
         * @return the momentum absorbed
         */
        public abstract void collide(ECollision coll);
        protected abstract void setCollideDamage(ECollision coll, ECollision result);
        protected abstract void meet(ENTITY other);
        protected abstract bool willCollideWith(ENTITY other);
        protected bool collidesWithOthers(ENTITY e)
        {
            return true;
        }

        public abstract double getDefenceSkill(double dirDot, double adx, double ady);
        //public abstract double getDefenceSpeed(double dirDot);

        /**
         * Notifies this entity of a collision with a solid tile.
         * @param norX
         * @param norY
         * @param momentum. The force applied
         */
        public abstract bool collideTile(bool broken, double norX, double norY, double momentum, int tx, int ty);

        public abstract void collideUnconnected();

        /**
         * The main logical update of the entity. Returns false if the entity is no more.
         * @param ds
         * @return
         */
        protected abstract bool update(double ds);

        public bool isRemoved()
        {
            return handlerId == -1;
        }

        protected void add(bool collide)
        {
            ENTITIES().add(this, collide);
        }

        public void helloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie()
        {
            if (!isRemoved())
                SETT.ENTITIES().remove(this);
        }

        protected abstract void removeAction();

        public int id()
        {
            return handlerId;
        }

        protected abstract double height();

        public COORDINATE tc()
        {
            return physics.tileC();
        }

        public int ssx()
        {
            return gx;
        }

        public int ssy()
        {
            return gy;
        }

        protected void save(FilePutter file)
        {
            file.i(handlerId);
            file.s(gx);
            file.s(gy);
            speed.save(file);
            physics.save(file);
        }

        protected void load(FileGetter file)
        {
            handlerId = file.i();
            gx = file.s();
            gy = file.s();
            speed.load(file);
            physics.load(file);
        }
    }
}