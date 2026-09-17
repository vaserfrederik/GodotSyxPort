using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.misc.util;
using settlement.room.service.module;
using snake2d.util.sets;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.service
{
    public abstract class MPlan<T> where T : ROOM_SERVICE_ACCESS_HASER
    {
        private static int dist;
        public readonly LIST<T> services;
        private readonly bool include;

        public MPlan(string key, LIST<T> services, bool include)
        {
            base("SPlan_" + key);
            this.services = services;
            this.include = include;
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            return walk.Set(a, d);
        }

        private readonly Resumer walk = new Resumer("Walk")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                AISubActivation s = null;

                if (include)
                    s = AI.SUBS().walkTo.serviceInclude(a, d, Blue(d).service(), dist);
                else
                    s = AI.SUBS().walkTo.service(a, d, Blue(d).service().finder, dist);
                if (s == null)
                    return null;
                d.planTile.Set(d.path.destX(), d.path.destY());
                Blue(d).service().reportAccess(a, d.planTile);
                Blue(d).service().reportDistance(a);
                Blue(d).service().reportAccess(a, d.planTile);
                return s;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                return Arrive(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }
        };

        protected abstract AISubActivation Arrive(Humanoid a, AIManager d);

        protected T Blue(AIManager d)
        {
            return services.Get(d.planByte3);
        }

        protected override void Name(Humanoid a, AIManager d, Str string)
        {
            string.Add(Blue(d).service().verb);
            if (S.Get().debug)
            {
                string.s().Add('(');
                base.Name(a, d, string);
                string.Add(')');
            }
        }

        protected override void Cancel(Humanoid a, AIManager d)
        {
            //Blue(d).service().clearAccess(a);
            base.Cancel(a, d);
        }

        protected FSERVICE Get(Humanoid a, AIManager d)
        {
            T blue = Blue(d);
            if (blue != null)
                return blue.service().service(d.planTile.x(), d.planTile.y());
            return null;
        }

        public NEED Need(AIManager d)
        {
            return Blue(d).service().need;
        }
    }
}