using System;
using System.Collections.Generic;

namespace game.event.actions
{
    public abstract class EventActionConstructor
    {
        public readonly string key;

        public EventActionConstructor(string key)
        {
            this.key = key;
        }

        public abstract EventAction Action(Data data);

        public class Data
        {
            public Json json;
            public Event parent;
            public EChoice choice;
            public LISTE<EventAction> all;
            public EventCollection engine;
        }
    }
}