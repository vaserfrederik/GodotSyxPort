using System;
using System.Collections.Generic;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.resources
{
    public interface RES_AMOUNT
    {
        RESOURCE resource();
        int amount();
        
        public static LIST<RES_AMOUNT> Make(Json json)
        {
            LinkedList<RES_AMOUNT> li = new LinkedList<RES_AMOUNT>();
            foreach (string k in json.Keys())
            {
                RESOURCE r = RESOURCES.Map().TryGet(k);
                if (r != null)
                {
                    li.Add(new Imp(r, json.I(k)));
                }
            }
            return new ArrayList<RES_AMOUNT>(li);
        }
        
        public class Imp : RES_AMOUNT, ISerializable
        {
            private static readonly long serialVersionUID = 1L;
            private byte cIndex;
            private int amount;
            
            public Imp()
            {
                this((RESOURCE)null, 0);
            }
            
            public Imp(RESOURCE c)
            {
                this(c, 0);
            }
            
            public Imp(RESOURCE c, int amount)
            {
                if (c != null)
                    cIndex = (byte)c.bIndex();
                this.amount = amount;
            }
            
            public Imp(RES_AMOUNT wa, float factor)
            {
                cIndex = (byte)wa.resource().bIndex();
                this.amount = (int)(wa.amount() * factor);
            }
            
            public RESOURCE resource()
            {
                return RESOURCES.ALL()[cIndex];
            }
            
            public int amount()
            {
                return amount;
            }
            
            public void add(int amount)
            {
                this.amount += amount;
            }
            
            public void set(int amount)
            {
                this.amount = amount;
            }
            
            public Imp setResource(RESOURCE res)
            {
                this.cIndex = res.bIndex();
                return this;
            }
            
            public Imp setResource(RESOURCE res, int amount)
            {
                this.cIndex = res.bIndex();
                this.amount = amount;
                return this;
            }
        }
        
        public class Abs : RES_AMOUNT
        {
            private readonly byte cIndex;
            private readonly int amount;
            
            public Abs(RESOURCE c, int amount)
            {
                cIndex = (byte)c.bIndex();
                this.amount = amount;
            }
            
            public RESOURCE resource()
            {
                return RESOURCES.ALL()[cIndex];
            }
            
            public int amount()
            {
                return amount;
            }
        }
    }
}