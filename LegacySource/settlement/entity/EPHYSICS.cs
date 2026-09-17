using System;
using System.IO;
using init.constant;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;

namespace settlement.entity
{
    public interface EPHYSICS : BODY_HOLDER
    {
        public static int MOM_TRESHOLD = 7 * C.TILE_SIZE;
        public static double MOM_TRESHOLDI = 1.0 / MOM_TRESHOLD;

        /**
         * 
         * @return the tile which the centre occupies
         */
        public COORDINATE tileC();

        /**
         * 
         * @return Height of the entity.
         */
        public double getHeight();
        /**
         * 
         * @return the hight over ground
         */
        public double getZ();
        /**
         * 
         * @return bounciness of entity.
         */
        public double getRestitution();
        /**
         * 
         * @return mass of the entity. -1 for solid. 0 for no mass
         */
        public double getMass();

        /**
         * 
         * @return inverted mass. Note that mass <= 0 will have inverted mass = 0
         */
        public double getMassI();

        /**
         * 
         * @param other
         * @return - 1 if other is "taller" than this and should be rendered above
         *              1 if opposite
         */
        public default int compareRenderHeight(EPHYSICS other)
        {
            double h1 = getHeight() + getZ();
            double h2 = other.getHeight() + other.getZ();
            if (h1 == h2)
                return 0;

            return h1 - h2 < 0 ? -1 : 1;
        }

        public class Solid : EPHYSICS
        {
            private static readonly int MASK = ~(C.SCALE - 1);
            /**
             * Can be used by by all static entities 
             */
            protected double heightOverGround = 0;
            protected double height = RND.rFloat(0.01);
            protected double restitution = 0.2f;
            private float mass = 1;
            protected float massI = 0;
            protected readonly Rec hitbox = new Rec
            {
                moveX1 = (double X1) =>
                {
                    base.moveX1(X1);
                    currentTile.set((int)(cX() >> C.T_SCROLL), currentTile.y());
                    return this;
                },

                moveY1 = (double Y1) =>
                {
                    base.moveY1(Y1);
                    currentTile.set(currentTile.x(), (int)(cY() >> C.T_SCROLL));
                    return this;
                }
            };
            protected readonly Coo currentTile = new Coo();
            int x1, x2, y1, y2 = -1;
            /**
             * loss/s
             */
            protected static readonly double AIR_REDUCER = 0.005;
            /**
             * m/s/mass
             */
            protected static readonly double GROUND_FRICTION = C.TILE_SIZE;
            private readonly static double FLIGHTM = C.TILE_SIZE * 10;
            short tx1, tx2, ty1, ty2;

            void initMoveCheck()
            {
                x1 = body().x1();
                x2 = body().x2();
                y1 = body().y1();
                y2 = body().y2();
            }

            bool MoveCheck()
            {
                return ((x1 & MASK) == (body().x1() & MASK) && (y1 & MASK) == (body().y1() & MASK));
            }

            public override double getHeight() { return height; }
            public void setHeight(double height) { this.height = height; }

            public override double getRestitution() { return restitution; }
            public void setRestitution(float r) { this.restitution = r; }

            public override double getMass() { return mass; }
            public void setMass(double mass)
            {
                this.mass = (float)mass;
                if (mass > 0)
                    this.massI = 1f / this.mass;
                else
                    this.massI = 0;
            }

            public override double getMassI() { return massI; }

            public override double getZ() { return heightOverGround; }
            public void setHeightOverGround(double height) { this.heightOverGround = height; }

            public override Rec body() { return hitbox; }

            public override COORDINATE tileC() { return currentTile; }

            public void initPosition(Solid other)
            {
                hitbox.set(other);
                currentTile.set(other.currentTile);
            }

            public void initPosition(int x, int y, int hitBoxWidth, int hitBoxHeight)
            {
                hitbox.setWidth(hitBoxWidth);
                hitbox.setHeight(hitBoxHeight);
                hitbox.moveC(x, y);
                currentTile.set(x >> C.T_SCROLL, y >> C.T_SCROLL);
            }

            public void initPosition(BODY_HOLDER e)
            {
                this.initPosition(e.body().cX(), e.body().cY(), e.body().width(), e.body().height());
            }

            public bool move(ENTITY e, ESpeed.Imp speed, double ds)
            {
                if (speed.magnitude() > 0)
                {
                    hitbox.incr(speed.x() * ds, speed.y() * ds);
                    return true;
                }

                return false;
            }

            public double getFlightMomentum()
            {
                return getMass() * FLIGHTM;
            }

            public bool isWithinTile()
            {
                return tx1 == tx2 && ty1 == ty2;
            }

            public int tx1() { return tx1; }
            public int tx2() { return tx2; }
            public int ty1() { return ty1; }
            public int ty2() { return ty2; }

            public void save(FilePutter file)
            {
                file.d(heightOverGround);
                file.d(height);
                file.d(restitution);
                file.f(mass);
                file.f(massI);
                hitbox.save(file);
                currentTile.save(file);
                file.s(tx1).s(tx2).s(ty1).s(ty2);
            }

            public void load(FileGetter file) throws IOException
            {
                heightOverGround = file.d();
                height = file.d();
                restitution = file.d();
                mass = file.f();
                massI = file.f();
                hitbox.load(file);
                currentTile.load(file);
                tx1 = file.s();
                tx2 = file.s();
                ty1 = file.s();
                ty2 = file.s();
            }
        }
    }
}