using game.boosting;
using game.boosting.superb;
using game.faction.royalty;
using snake2d.util.sprite;

abstract class ROpper : SuperSpecImp<Royalty>
{
    public ROpper(string key, string name, string desc, SPRITE icon, double to, bool isMul)
        : base(ROPINION.BOOST(), key, new BSourceInfo(name, desc, null, icon), desc, to, isMul)
    {
    }

    public override double pget(Royalty bo)
    {
        return value.getD(bo) * getModifier(bo);
    }

    public override double get(Royalty o)
    {
        return base.get(o);
    }

    protected abstract double ptarget(Royalty bo);

    public override void update(Royalty bo, double time)
    {
        double inc = time * increase(bo);
        double t = ptarget(bo);
        double v = value.getD(bo);
        v += inc;

        if (inc >= 0)
        {
            if (v > t)
                v = t;
        }
        else if (inc < 0)
        {
            if (v < t)
                v = t;
        }

        value.setD(bo, v);
    }

    public override double increase(Royalty roy)
    {
        return 0;
    }

    public override void activate(Royalty bo, bool active)
    {
    }

    public override bool activated(Royalty bo)
    {
        return get(bo) > 0;
    }

    public override double secondsRemaining(Royalty bo)
    {
        return 0;
    }

    public class ROpperDown : ROpper
    {
        private readonly double dc;

        public ROpperDown(string key, string name, string desc, SPRITE icon, double to, bool isMul, double downSpeed)
            : base(key, name, desc, icon, to, isMul)
        {
            dc = 1.0 / downSpeed;
        }

        public override double increase(Royalty roy)
        {
            return -dc;
        }

        protected override double ptarget(Royalty bo)
        {
            return 0;
        }
    }
}