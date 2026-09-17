using settlement.environment;
using snake2d;
using util.rendering;

namespace settlement.overlay
{
    class Env : Addable
    {
        private readonly SettEnv envThing;

        public Env(SettEnv env, bool above) : base(env.icon, env.key, env.info.name, env.info.desc, true, above)
        {
            this.envThing = env;
        }

        public override void renderBelow(Renderer r, RenderIterator it)
        {
            renderUnder(envThing.getView(it.tx(), it.ty()), r, it);
        }
    }
}