using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Settlement.Entity.Humanoid.AI.Main
{
    public sealed class AI
    {
        private static AI s;
        private readonly AISTATES STATES;
        private readonly AIEventListeners listeners;
        private readonly AISUBS SUBS;
        private readonly AIPlans plans;
        private readonly AIModules modules;
        private readonly AIPLAN first;
        private readonly AIData data = new AIData();
        private readonly KeyMap<AIElement> map = new KeyMap<AIElement>();
        private readonly ArrayListGrower<AIElement> all = new ArrayListGrower<AIElement>();
        private int[] loadOrder = null;

        private AI()
        {
            AI.s = this;
            STATES = new AISTATES();
            listeners = new AIEventListeners();
            SUBS = new AISUBS();
            plans = new AIPlans();
            modules = new AIModules();
            first = new AIPLAN.PLANRES("planFirst")
            {
                protected override AISubActivation Init(Humanoid a, AIManager d)
                {
                    return resumer.Set(a, d);
                }

                private readonly Resumer resumer = new Resumer("standing")
                {
                    protected override AISubActivation SetAction(Humanoid a, AIManager d)
                    {
                        return AI.SUBS().STAND.ActivateTime(a, d, 1);
                    }

                    protected override AISubActivation Res(Humanoid a, AIManager d)
                    {
                        // TODO Auto-generated method stub
                        return null;
                    }

                    public override bool Con(Humanoid a, AIManager d)
                    {
                        return true;
                    }

                    public override void Can(Humanoid a, AIManager d)
                    {
                        // TODO Auto-generated method stub
                    }
                };
            };

            GAME.Saver().AddSpecialSaver(new Savable("HAI")
            {
                public void Save(FilePutter file)
                {
                    file.I(all.Count);
                    foreach (AIElement e in all)
                        file.Chars(e.key);
                }

                public void Load(FileGetter file) throws IOException
                {
                    int am = file.I();
                    loadOrder = Alloc.Ii(am);
                    Array.Fill(loadOrder, -1);
                    for (int i = 0; i < am; i++)
                    {
                        string k = file.Chars();
                        if (map.ContainsKey(k))
                        {
                            loadOrder[i] = map.Get(k).index;
                        }
                    }
                }
            });
        }

        public static AIElement Load(int i)
        {
            if (i < 0 || i >= s.loadOrder.Length || s.loadOrder[i] == -1)
                return null;
            return s.all.Get(s.loadOrder[i]);
        }

        public static int Save(AIElement e)
        {
            return e != null ? e.index : -1;
        }

        public static AISTATES STATES()
        {
            return s.STATES;
        }

        public static AISUBS SUBS()
        {
            return s.SUBS;
        }

        public static AIPlans Plans()
        {
            return s.plans;
        }

        public static AIModules Modules()
        {
            return s.modules;
        }

        public static AIEventListeners Listeners()
        {
            return s.listeners;
        }

        public static AIPLAN First()
        {
            return s.first;
        }

        public static void Init()
        {
            new AI();
        }

        public static AIData Data()
        {
            return s.data;
        }

        public static AIData.AIDataSuspender Suspender(string key)
        {
            return s.data.NewAIDataSuspender(key);
        }

        public static AIDataBit Bit(string key)
        {
            return s.data.NewAIDataBit(key);
        }

        public class AIElement
        {
            public readonly string className;
            private readonly int index;
            public readonly string key;

            static AIElement()
            {
                string cn = typeof(AIElement).FullName;
                string[] ss = cn.Split('.');

                string match = ss[ss.Length - 1];
                foreach (StackTraceElement e in new StackTrace().GetFrames())
                    if (e.ToString().Contains("." + match))
                    {
                        match += "_" + e.GetFileLineNumber();
                    }

                className = match;
            }

            protected AIElement(string key)
            {
                AI.s.map.Put(key, this);
                index = AI.s.all.Add(this);
                this.key = key;
            }

            protected string GetClassLine(Humanoid a, AIManager d)
            {
                string ss = this.GetType().ToString();
                return ss;
            }
        }
    }
}