using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.misc.util;
using settlement.room.service.module;
using settlement.stats;
using snake2d.util.sets;
using snake2d.util.sprite.text;

abstract class SPlanAbs<T> : AIPLAN.PLANRES where T : ROOM_SERVICE_ACCESS_HASER
{
    public readonly LIST<T> services;
    private readonly bool include;

    public SPlanAbs(string key, LIST<T> services, bool include)
        : base("SPlan_" + key)
    {
        this.services = services;
        this.include = include;
    }

    protected override AISubActivation init(Humanoid a, AIManager d)
    {
        return walk.set(a, d);
    }

    private readonly Resumer walk = new Resumer("Walk")
    {
        protected override AISubActivation setAction(Humanoid a, AIManager d)
        {
            AISubActivation s = null;

            if (include)
                s = AI.SUBS().walkTo.serviceInclude(a, d, blue(d).service(), STATS.FOOD().STARVATION.indu().get(a.indu()) > 0 ? int.MaxValue : blue(d).service().radius);
            else
                s = AI.SUBS().walkTo.service(a, d, blue(d).service().finder, STATS.FOOD().STARVATION.indu().get(a.indu()) > 0 ? int.MaxValue : blue(d).service().radius);
            if (s == null)
                return null;
            d.planTile.set(d.path.destX(), d.path.destY());
            blue(d).service().reportDistance(a);
            blue(d).service().reportAccess(a, d.planTile);
            return s;
        }

        protected override AISubActivation res(Humanoid a, AIManager d)
        {
            return arrive(a, d);
        }

        public override bool con(Humanoid a, AIManager d)
        {
            return true;
        }

        public override void can(Humanoid a, AIManager d)
        {
        }
    };

    protected abstract AISubActivation arrive(Humanoid a, AIManager d);

    protected T blue(AIManager d)
    {
        return services.get(d.planByte3);
    }

    protected override void name(Humanoid a, AIManager d, Str string)
    {
        string.add(blue(d).service().verb);
        if (S.get().debug)
        {
            string.s().add('(');
            base.name(a, d, string);
            string.add(')');
        }
    }

    protected override void cancel(Humanoid a, AIManager d)
    {
        // blue(d).service().clearAccess(a);
        base.cancel(a, d);
    }

    protected FSERVICE get(Humanoid a, AIManager d)
    {
        T blue = blue(d);
        if (blue != null)
            return blue.service().service(d.planTile.x(), d.planTile.y());
        return null;
    }
}