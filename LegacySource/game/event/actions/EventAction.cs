using System;
using System.Collections.Generic;
using game.event.engine;
using snake2d.util.datatypes;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;

namespace game.event.actions
{
    public abstract class EventAction
    {
        public readonly ArrayListGrower<CInt> ints = new ArrayListGrower<CInt>();
        public readonly string key;
        public bool hideUI = false;

        protected EventAction(string key, LISTE<EventAction> all)
        {
            this.key = key;
            all.Add(this);
        }

        public virtual void Exe(Event e, EContext data)
        {
        }

        private void SetContext(Event e, EContext data)
        {
        }

        public virtual void Hover(GBox b, Event e, EContext context)
        {
        }

        public virtual CharSequence Problem(Event e, EContext context)
        {
            return null;
        }

        public virtual void AddToMessageBody(LISTE<RENDEROBJ> rows, Event e, EContext context, RECTANGLE messBody)
        {
        }

        public virtual void Update(Event e, EContext context, double ds, double second)
        {
        }

        private static string levent = null;

        public class CInt
        {
            private int di = -1;

            public CInt(string key)
            {
                ints.Add(this);
            }

            public int Get(Event e, EContext t)
            {
                if (levent != e.key)
                {
                    t.actionContext = EventActionContext.MakeData(e, t.actionContext);
                    levent = e.key;
                }
                if (t.actionContext != null && di >= 0 && di < t.actionContext.Length)
                    return t.actionContext[di];
                return 0;
            }

            public void Set(Event e, EContext t, int i)
            {
                if (levent != e.key)
                {
                    t.actionContext = EventActionContext.MakeData(e, t.actionContext);
                    levent = e.key;
                }
                if (t.actionContext != null && di >= 0 && di < t.actionContext.Length)
                    t.actionContext[di] = i;
            }
        }
    }
}