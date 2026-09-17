using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using snake2d.util.sets;
using System.Collections.Generic;

namespace settlement.entity.humanoid.ai.main
{
    public abstract class AISUB : AIElement, AIEventListeners.Default
    {
        public static class AISubActivation
        {
            private static AISubActivation i = new AISubActivation();
            private AISUB sub;
            private AISTATE state;
            private readonly AISubActivationI inter = new AISubActivationI();

            private AISubActivation()
            {
            }

            public AISUB Get()
            {
                AISUB s = sub;
                sub = null;
                return s;
            }

            public AISUB Peek()
            {
                return sub;
            }

            public AISTATE State()
            {
                return this.state;
            }

            public AISubActivation SetState(AISTATE state)
            {
                this.state = state;
                return this;
            }

            public static AISubActivation Make(AISUB s, AISTATE state)
            {
                i.sub = s;
                i.state = state;
                if (i.state == null)
                    throw new System.RuntimeException(i.sub.GetType().Name);
                return i;
            }

            public AISubActivationI I()
            {
                return inter;
            }

            private class AISubActivationI
            {
                private AISubActivationI()
                {
                }

                public AISUB Get()
                {
                    AISUB s = sub;
                    sub = null;
                    return s;
                }

                public AISTATE State()
                {
                    return state;
                }
            }
        }

        protected AISUB(string key) : base("SUB_" + key)
        {
        }

        /**
         * Called after an interruption has finished. Returning null is considered a failure
         * @param a
         * @param d
         * @return
         */
        public abstract AISTATE ResumeInterrupted(Humanoid a, AIManager d, HEvent @event);

        public abstract AISubActivation Activate(Humanoid a, AIManager d);

        /**
         * Called when this sub has finished. If false is returned the plan will be cancelled
         * Will not cancel the sub. This should have been done by the sub
         * @param a
         * @param d
         * @return
         */
        public abstract bool IsSuccessful(Humanoid a, AIManager d);

        public abstract AISTATE Resume(Humanoid a, AIManager d);

        public abstract void Cancel(Humanoid a, AIManager d);

        public abstract System.CharSequence Name(Humanoid a, AIManager d);

        public static abstract class Simple : AISUB
        {
            private readonly System.CharSequence name;

            protected Simple(string key) : base(key)
            {
                this.name = this.GetType().Name;
            }

            protected Simple(string key, System.CharSequence name) : base(key)
            {
                this.name = name;
            }

            protected override AISTATE ResumeInterrupted(Humanoid a, AIManager d, HEvent @event)
            {
                return null;
            }

            public override AISubActivation Activate(Humanoid a, AIManager d)
            {
                d.subByte = 0;
                AISTATE s = Resume(a, d);
                return AISubActivation.Make(this, s);
            }

            public AISubActivation Activate(Humanoid a, AIManager d, AISTATE s)
            {
                d.subByte = 0;
                return AISubActivation.Make(this, s);
            }

            protected override bool IsSuccessful(Humanoid a, AIManager d)
            {
                return true;
            }

            protected abstract AISTATE Resume(Humanoid a, AIManager d);

            protected override void Cancel(Humanoid a, AIManager d)
            {
            }

            protected override System.CharSequence Name(Humanoid a, AIManager d)
            {
                return name;
            }

            protected bool IsBattleReady()
            {
                return false;
            }
        }

        public static abstract class Resumable : AISUB, HEventListener
        {
            private readonly System.CharSequence name;
            private readonly LISTE<Resumer> all = new ArrayList<Resumer>(20);

            protected Resumable(string key, System.CharSequence name) : base(key)
            {
                this.name = name;
            }

            protected Resumable(string key) : base(key)
            {
                this.name = this.GetType().Name;
            }

            protected override AISTATE ResumeInterrupted(Humanoid a, AIManager d, HEvent @event)
            {
                return all.Get(d.subByte).ResI(a, d, @event);
            }

            public override AISubActivation Activate(Humanoid a, AIManager d)
            {
                d.subByte = -1;
                return AISubActivation.Make(this, Resume(a, d));
            }

            protected AISubActivation Activate(Humanoid a, AIManager d, Resumer res)
            {
                d.subByte = res.index;
                return AISubActivation.Make(this, res.Set(a, d));
            }

            protected abstract AISTATE Init(Humanoid a, AIManager d);

            protected override AISTATE Resume(Humanoid a, AIManager d)
            {
                if (d.subByte == -1)
                    return Init(a, d);
                return all.Get(d.subByte).Res(a, d);
            }

            protected override bool IsSuccessful(Humanoid a, AIManager d)
            {
                if (d.subByte == -1)
                    return true;
                return all.Get(d.subByte).Success(a, d);
            }

            protected override void Cancel(Humanoid a, AIManager d)
            {
                if (d.subByte == -1)
                    return;
                all.Get(d.subByte).Can(a, d);
            }

            protected Resumer GetResumer(Humanoid a, AIManager d)
            {
                if (d.subByte < 0)
                    return null;
                return all.Get(d.subByte);
            }

            protected override System.CharSequence Name(Humanoid a, AIManager d)
            {
                return name;
            }

            public bool Event(Humanoid a, AIManager d, HEventData e)
            {
                return all.Get(d.subByte).Event(a, d, e);
            }

            public double Poll(Humanoid a, AIManager d, HPollData e)
            {
                return all.Get(d.subByte).Poll(a, d, e);
            }

            public abstract class Resumer : AIEventListeners.Default
            {
                protected readonly byte index;

                public Resumer()
                {
                    this.index = (byte)all.Add(this);
                }

                public AISTATE Set(Humanoid a, AIManager d)
                {
                    d.subByte = index;
                    return SetAction(a, d);
                }

                protected abstract AISTATE SetAction(Humanoid a, AIManager d);
                protected abstract AISTATE Res(Humanoid a, AIManager d);
                protected AISTATE ResI(Humanoid a, AIManager d, HEvent @event)
                {
                    return null;
                }
                protected bool Success(Humanoid a, AIManager d)
                {
                    return true;
                }
                protected void Can(Humanoid a, AIManager d)
                {
                }
            }

            private Resumer Get(Humanoid a, AIManager d)
            {
                return all.Get(d.subByte);
            }

            private class Success : Resumer
            {
                public override AISTATE Res(Humanoid a, AIManager d)
                {
                    return null;
                }

                public override AISTATE ResI(Humanoid a, AIManager d, HEvent @event)
                {
                    return AI.STATES().STAND.Activate(a, d, 0.2f);
                }

                public override bool Success(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override AISTATE SetAction(Humanoid a, AIManager d)
                {
                    return AI.STATES().STAND.Activate(a, d, 0.2f);
                }
            }

            private class Fail : Resumer
            {
                public override AISTATE Res(Humanoid a, AIManager d)
                {
                    return null;
                }

                public override AISTATE ResI(Humanoid a, AIManager d, HEvent @event)
                {
                    return AI.STATES().STAND.Activate(a, d, 0.2f);
                }

                public override bool Success(Humanoid a, AIManager d)
                {
                    return false;
                }

                public override AISTATE SetAction(Humanoid a, AIManager d)
                {
                    return AI.STATES().STAND.Activate(a, d, 0.2f);
                }
            }
        }
    }
}