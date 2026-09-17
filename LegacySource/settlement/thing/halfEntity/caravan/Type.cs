using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.gui.misc;

namespace settlement.thing.halfEntity.caravan
{
    abstract class Type
    {
        static readonly ArrayList<Type> all = new ArrayList<Type>(10);
        static Type()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    all.Clear();
                }
            };
        }

        readonly int index;
        readonly string name;

        protected Type(string name)
        {
            this.name = name;
            index = all.Add(this);
        }

        internal static readonly Coo coo = new Coo();
        public abstract bool Init(Caravan c, int amount);
        public abstract bool Update(Caravan c, double ds);
        public abstract void Cancel(Caravan c, bool dump);

        public abstract void HoverInfo(GBox box, Caravan c);
        protected abstract void Load(Caravan caravan);
    }
}