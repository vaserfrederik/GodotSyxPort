using System;
using System.Collections.Generic;
using snake2d.util.sets;
using settlement.room.main;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using init.type;

namespace settlement.entity.humanoid.ai.main
{
    public abstract class AIModule : INDEXED
    {
        private static readonly ArrayList<AIModule> all = new ArrayList<AIModule>(200);
        static AIModule()
        {
            all.Add(null);
        }

        private readonly byte index;
        static AIModule()
        {
            GameDisposable.Add(() =>
            {
                all.Clear();
                all.Add(null);
            });
        }

        public readonly string name;
        public readonly string desc;
        private readonly SPRITE icon;

        private readonly Bitmap1D hasType = new Bitmap1D(HTYPES.ALL().Size(), false);

        public AIModule(SPRITE icon, string name, string desc)
        {
            index = (byte)all.Add(this);
            if (index < 0)
                throw new RuntimeException();
            this.icon = icon;
            this.name = name;
            this.desc = desc;
        }

        public bool Has(HTYPE t)
        {
            return hasType.Get(t.Index());
        }

        public abstract AiPlanActivation GetPlan(Humanoid a, AIManager d);
        protected virtual void Init(Humanoid a, AIManager d, HTYPE prev, HTYPE current)
        {

        }
        protected virtual void Cancel(Humanoid a, AIManager d)
        {

        }

        protected virtual void Finish(Humanoid a, AIManager d)
        {

        }

        protected abstract void Update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateOfDay);
        public abstract int GetPriority(Humanoid a, AIManager d);

        public AiPlanActivation Resume(Humanoid a, AIManager d, int timesResumedBefore)
        {
            return null;
        }

        public bool Is(Humanoid a, AIManager d)
        {
            return AIModules.Current(d) == this;
        }

        public bool ModuleCanContinue(Humanoid a, AIManager d)
        {
            AIModule m = AIModules.Next(d);
            return m == null || m == this;
        }

        public virtual void EvictFromRoom(Humanoid a, AIManager d, ROOMA r)
        {

        }

        public int Index()
        {
            return index;
        }

        public SPRITE Icon()
        {
            return icon;
        }
    }
}