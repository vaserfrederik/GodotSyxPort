using System;
using util.data;

namespace settlement.entity.humanoid.ai.main
{
    public sealed class AIData : DataO<AIManager>
    {
        AIData() : base("AI_DATA")
        {
        }

        protected override long[] data(AIManager t)
        {
            return t.longs;
        }

        public class AIDataBit : DataO<AIManager>.DataBit, BOOLEAN_OE<AIManager>
        {
            public AIDataBit(string key) : base(key + "_BIT")
            {
            }

            public bool is(AIManager d)
            {
                return get(d) == 1;
            }

            public BOOLEAN_OE<AIManager> set(AIManager d, bool s)
            {
                set(d, s ? 1 : 0);
                return this;
            }
        }

        public sealed class AIDataSuspender : util.data.DataO<AIManager>.DataCrumb
        {
            public AIDataSuspender(string key) : base(key + "_sus")
            {
            }

            public bool is(AIManager d)
            {
                return get(d) != 0;
            }

            public void suspend(AIManager d)
            {
                set(d, 2);
            }

            public void update(AIManager d)
            {
                if (is(d))
                {
                    set(d, get(d) - 1);
                }
            }
        }
    }
}