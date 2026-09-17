using System;
using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using snake2d.util.sets;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.main
{
    public abstract class AIPLAN : AIElement, HEventListener
    {
        private static readonly AiPlanActivation activation = new AiPlanActivation();

        private AIPLAN(string key) : base("PLAN_" + key)
        {
        }

        public static class AiPlanActivation
        {
            private AIPLAN plan;
            private AISubActivation sub;

            public AIPLAN Plan()
            {
                AIPLAN p = plan;
                plan = null;
                return p;
            }

            public AISubActivation Sub()
            {
                AISubActivation s = sub;
                sub = null;
                return s;
            }
        }

        public abstract AiPlanActivation Activate(Humanoid a, AIManager d);
        public abstract void Cancel(Humanoid a, AIManager d);
        protected void Remove(Humanoid a, AIManager d)
        {
        }

        public abstract AISubActivation Resume(Humanoid a, AIManager d);
        public abstract bool ShouldContinue(Humanoid a, AIManager d);
        public abstract void Name(Humanoid a, AIManager d, Str string);
        public abstract AISubActivation ResumeFailed(Humanoid a, AIManager d, HEvent event);

        public bool NotifyIfSubFails()
        {
            return true;
        }

        public override bool Event(Humanoid a, AIManager d, HEventData e)
        {
            return d.plansub().Event(a, d, e);
        }

        public override double Poll(Humanoid a, AIManager d, HPollData e)
        {
            return d.plansub().Poll(a, d, e);
        }

        private static string empty = "";
        protected string Debug(Humanoid a, AIManager d)
        {
            return empty;
        }

        protected static AISubActivation TrySub(Humanoid a, AIManager d, AISubActivation trial, AIDataSuspender suspender)
        {
            if (trial == null)
            {
                if (suspender != null)
                    suspender.suspend(d);
                return AI.SUBS().failure.activate(a, d);
            }
            return trial;
        }

        public static abstract class PLANRES : AIPLAN
        {
            internal readonly ArrayListResize<ResumerRaw> resumers = new ArrayListResize<ResumerRaw>(10, 100);

            protected readonly Resumer WAIT_AND_EXIT = new Resumer("waiting")
            {
                protected override AISubActivation SetAction(Humanoid a, AIManager d)
                {
                    return AI.SUBS().STAND.activate(a, d);
                }

                protected override AISubActivation Res(Humanoid a, AIManager d)
                {
                    return null;
                }

                public override bool Con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void Can(Humanoid a, AIManager d)
                {
                }
            };

            public PLANRES(string key) : base(key)
            {
            }

            public override AiPlanActivation Activate(Humanoid a, AIManager d)
            {
                d.planResumerByte = -1;
                AISubActivation ac = Init(a, d);

                if (ac != null)
                {
                    activation.plan = this;
                    activation.sub = ac;
                    if (d.planResumerByte < 0)
                        GAME.Error("" + this.GetType());
                    return activation;
                }
                return null;
            }

            protected override void Cancel(Humanoid a, AIManager d)
            {
                if (resumers.size() <= d.planResumerByte)
                    System.err.println(key + this);
                resumers.get(d.planResumerByte).Can(a, d);
                d.planResumerByte = -5;
            }

            protected override AISubActivation Resume(Humanoid a, AIManager d)
            {
                return resumers.get(d.planResumerByte).Res(a, d);
            }

            protected override bool ShouldContinue(Humanoid a, AIManager d)
            {
                return resumers.get(d.planResumerByte).Con(a, d);
            }

            protected override void Name(Humanoid a, AIManager d, Str string)
            {
                resumers.get(d.planResumerByte).Name(a, d, string);
            }

            protected override AISubActivation ResumeFailed(Humanoid a, AIManager d, HEvent event)
            {
                return resumers.get(d.planResumerByte).ResFailed(a, d, event);
            }

            protected override string Debug(Humanoid a, AIManager d)
            {
                if (d.planResumerByte < 0)
                {
                    return empty;
                }
                return this.GetType().Name + " " + d.planResumerByte + " " + resumers.get(d.planResumerByte).name + " " + resumers.get(d.planResumerByte);
            }

            internal ResumerRaw GetResumer(AIManager d)
            {
                if (d.planResumerByte < 0 || d.planResumerByte >= resumers.size())
                    return null;
                return resumers.get(d.planResumerByte);
            }

            public override bool Event(Humanoid a, AIManager d, HEventData e)
            {
                return resumers.get(d.planResumerByte).Event(a, d, e);
            }

            public override double Poll(Humanoid a, AIManager d, HPollData e)
            {
                if (resumers.size() <= d.planResumerByte)
                    System.err.println(this);
                return resumers.get(d.planResumerByte).Poll(a, d, e);
            }

            protected abstract AISubActivation Init(Humanoid a, AIManager d);

            public static abstract class ResumerRaw : HEventListener
            {
                public readonly CharSequence name;
                readonly byte index;
                public readonly string className;

                public ResumerRaw(PLANRES daddy, CharSequence verb)
                {
                    this.name = verb;
                    this.index = (byte)daddy.resumers.add(this);
                    string cn = this.GetType().Name;
                    string[] ss = cn.Split('.');

                    string match = ss[ss.Length - 1];
                    foreach (StackTraceElement e in new System.Diagnostics.StackFrame().GetStackTrace())
                        if (e.ToString().Contains("." + match))
                        {
                            match += "_" + e.GetFileLineNumber();
                        }

                    className = match;
                }

                public ResumerRaw(PLANRES daddy) : this(daddy, "")
                {
                }

                public AISubActivation ResFailed(Humanoid a, AIManager d, HEvent event)
                {
                    return null;
                }

                public final AISubActivation Set(Humanoid a, AIManager d)
                {
                    d.planResumerByte = index;
                    return SetAction(a, d);
                }

                public final AISubActivation TrySet(Humanoid a, AIManager d)
                {
                    byte old = d.planResumerByte;
                    d.planResumerByte = index;
                    AISubActivation s = SetAction(a, d);
                    if (s != null)
                        return s;
                    d.planResumerByte = old;
                    return null;
                }

                protected abstract AISubActivation SetAction(Humanoid a, AIManager d);
                protected abstract AISubActivation Res(Humanoid a, AIManager d);
                public abstract bool Con(Humanoid a, AIManager d);
                public abstract void Can(Humanoid a, AIManager d);

                protected void Name(Humanoid a, AIManager d, Str string)
                {
                    string.Add(name);
                }

                public override bool Event(Humanoid a, AIManager d, HEventData e)
                {
                    return d.plansub().Event(a, d, e);
                }

                public override double Poll(Humanoid a, AIManager d, HPollData e)
                {
                    if (d.plansub() == null)
                        System.err.println(d.plan().key + " " + d.plan().className + " " + d.planResumerByte);
                    return d.plansub().Poll(a, d, e);
                }
            }

            public abstract class Resumer : ResumerRaw
            {
                public Resumer(CharSequence verb) : base(PLANRES.this, verb)
                {
                }

                public Resumer() : base(PLANRES.this)
                {
                }
            }
        }
    }
}