using System;
using System.Collections.Generic;

namespace Settlement.Entity.Animal
{
    using static Settlement.Main.SETT;
    using static Settlement.Main.SETT.ANIMALS;
    using static Settlement.Main.SETT.ROOMS;

    using Init.Constant;
    using Settlement.Entity;
    using Settlement.Entity.Humanoid;
    using Settlement.Main;
    using Snake2D;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Rnd;
    using View.Keyboard;
    using View.Main;

    enum State
    {
        STAND,
        GRACE,
        WALK_RANDOM,
        PANIC,
        UNCONSIOUS,
        CONTROLLED
    }

    static class StateExtensions
    {
        private static readonly Dictionary<State, StateImplementation> implementations = new Dictionary<State, StateImplementation>
        {
            { State.STAND, new StandImplementation() },
            { State.GRACE, new GraceImplementation() },
            { State.WALK_RANDOM, new WalkRandomImplementation() },
            { State.PANIC, new PanicImplementation() },
            { State.UNCONSIOUS, new UnconsciousImplementation() },
            { State.CONTROLLED, new ControlledImplementation() }
        };

        public static bool Update(this State state, Animal a, double ds)
        {
            return implementations[state].Update(a, ds);
        }

        public static void Activate(this State state, Animal a, double duration)
        {
            implementations[state].Activate(a, duration);
        }

        public static Sprite Sprite(this State state, Animal a)
        {
            return implementations[state].Sprite(a);
        }

        public static void Scare(this State state, Animal a, ENTITY other, bool flee)
        {
            implementations[state].Scare(a, other, flee);
        }

        public static void Collide(this State state, Animal a, ENTITY other, double norX, double norY, double momentum)
        {
            implementations[state].Collide(a, other, norX, norY, momentum);
        }

        public static bool CollideTile(this State state, Animal a, bool broken, double norX, double norY, double momentum)
        {
            return implementations[state].CollideTile(a, broken, norX, norY, momentum);
        }

        public static bool WantsToCollide(this State state, Animal a, double mom)
        {
            return implementations[state].WantsToCollide(a, mom);
        }

        public static void CollideUnwalkable(this State state, Animal a)
        {
            implementations[state].CollideUnwalkable(a);
        }

        public static void Meet(this State state, Animal a, ENTITY other)
        {
            implementations[state].Meet(a, other);
        }

        public static bool WillCollideWith(this State state, Animal a, ENTITY other)
        {
            return implementations[state].WillCollideWith(a, other);
        }
    }

    abstract class StateImplementation
    {
        public virtual bool Update(Animal a, double ds)
        {
            a.Speed.MagnitudeAdjust(ds, 1.0, 1);
            a.StateTimer -= ds;
            return a.StateTimer > 0;
        }

        public virtual void Activate(Animal a, double duration)
        {
            a.StateTimer = (float)duration;
        }

        public virtual Sprite Sprite(Animal a)
        {
            return Sprite.STAND_STILL;
        }

        public virtual void Scare(Animal a, ENTITY other, bool flee)
        {
            if (a.Domesticated())
                return;
            if (other == null)
                return;
            OtherSet(a, other);

            bool shouldFlee = flee || other is Animal || a.Cub;

            if (!shouldFlee)
                shouldFlee = !ANIMALS().Spawn.IsTimeForAKill(a.Species());

            if (shouldFlee)
            {
                a.Speed.Turn2(other.Body.CX() + RND.rInt0(C.TILE_SIZEH), other.Body.CY() + RND.rInt0(C.TILE_SIZEH), a.Body.CX(), a.Body.CY());
            }
            else
                a.Speed.Turn2(a.Body, other.Body);
            a.SetState(State.PANIC, 5);
        }

        public virtual void Collide(Animal a, ENTITY other, double norX, double norY, double momentum)
        {
            if (momentum < a.Species().MomTreshold)
            {
                if (!a.Domesticated())
                {
                    Scare(a, other, false);
                }
                a.SetState(State.STAND, 1);
            }
            else if (momentum < a.Species().MomTreshold * 1.6)
            {
                a.SetState(State.UNCONSIOUS, 4);
            }
            else
            {
                a.Physics.SetHeightOverGround(a.Physics.GetZ() + (momentum - a.Species().MomTreshold) * 4);
                a.SetState(State.UNCONSIOUS, 8);
            }
        }

        public virtual bool CollideTile(Animal a, bool broken, double norX, double norY, double momentum)
        {
            if (momentum < a.Species().MomTreshold)
            {
                a.SetState(State.STAND, 1);
                return false;
            }
            else if (momentum < a.Species().MomTreshold * 1.5)
            {
                a.SetState(State.UNCONSIOUS, 4);
            }
            else
            {
                a.Physics.SetHeightOverGround(a.Physics.GetZ() + (momentum - a.Species().MomTreshold) * 4);
                a.SetState(State.UNCONSIOUS, 8);
            }

            return true;
        }

        public virtual bool WantsToCollide(Animal a, double mom)
        {
            return mom > a.Species().MomTreshold;
        }

        public virtual void CollideUnwalkable(Animal a)
        {
            int tx = a.TC.X();
            int ty = a.TC.Y();
            DIR d = a.Speed.Dir();
            for (int i = 0; i < DIR.ALL.Size(); i++)
            {
                if (!SETT.PATH().SOLIDITY.Is(tx, ty, d))
                {
                    a.Speed.SetRaw(d, 1.0);
                    break;
                }
                d = d.Next(1);
            }
        }

        public virtual void Meet(Animal a, ENTITY other)
        {
            OtherSet(a, other);
            a.SetState(State.WALK_RANDOM, 1f);
        }

        private static ENTITY Other(Animal a)
        {
            ENTITY e = SETT.ENTITIES().GetByID(a.StateI);
            if (e == null)
                a.StateI = -1;
            return e;
        }

        private static void OtherSet(Animal a, ENTITY other)
        {
            if (other != null)
                a.StateI = other.ID();
            else
                a.StateI = -1;
        }

        public virtual bool WillCollideWith(Animal a, ENTITY other)
        {
            return other is Humanoid && !a.Domesticated();
        }
    }

    class StandImplementation : StateImplementation
    {
        public override bool Update(Animal a, double ds)
        {
            a.Speed.MagnitudeAdjust(ds, 1.0, 1);
            a.StateTimer -= ds;
            return a.StateTimer > 0;
        }

        public override void Activate(Animal a, double duration)
        {
            a.StateTimer = (float)duration;
        }

        public override Sprite Sprite(Animal a)
        {
            return Sprite.STAND_STILL;
        }
    }

    class GraceImplementation : StateImplementation
    {
        public override bool Update(Animal a, double ds)
        {
            a.Speed.MagnitudeAdjust(ds, 1.0, 1);
            a.StateTimer -= ds;
            return a.StateTimer > 0;
        }

        public override void Activate(Animal a, double duration)
        {
            a.StateTimer = (float)duration;
        }

        public override Sprite Sprite(Animal a)
        {
            return Sprite.STAND_STILL;
        }
    }

    class WalkRandomImplementation : StateImplementation
    {
        public override bool Update(Animal a, double ds)
        {
            a.Speed.MagnitudeAdjust(ds, 1.0, 1);
            a.StateTimer -= ds;
            return a.StateTimer > 0;
        }

        public override void Activate(Animal a, double duration)
        {
            a.StateTimer = (float)duration;
        }

        public override Sprite Sprite(Animal a)
        {
            return Sprite.STAND_STILL;
        }
    }

    class PanicImplementation : StateImplementation
    {
        public override bool Update(Animal a, double ds)
        {
            a.Speed.MagnitudeAdjust(ds, 1.0, 1);
            a.StateTimer -= ds;
            return a.StateTimer > 0;
        }

        public override void Activate(Animal a, double duration)
        {
            a.StateTimer = (float)duration;
        }

        public override Sprite Sprite(Animal a)
        {
            return Sprite.STAND_STILL;
        }
    }

    class UnconsciousImplementation : StateImplementation
    {
        public override bool Update(Animal a, double ds)
        {
            a.Speed.MagnitudeAdjust(ds, 1.0, 1);
            a.StateTimer -= ds;
            return a.StateTimer > 0;
        }

        public override void Activate(Animal a, double duration)
        {
            a.StateTimer = (float)duration;
        }

        public override Sprite Sprite(Animal a)
        {
            return Sprite.STAND_STILL;
        }
    }

    class ControlledImplementation : StateImplementation
    {
        public override bool Update(Animal a, double ds)
        {
            a.Speed.MagnitudeAdjust(ds, 1.0, 1);
            a.StateTimer -= ds;
            return a.StateTimer > 0;
        }

        public override void Activate(Animal a, double duration)
        {
            a.StateTimer = (float)duration;
        }

        public override Sprite Sprite(Animal a)
        {
            return Sprite.STAND_STILL;
        }
    }
}