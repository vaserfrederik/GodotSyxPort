using System;
using System.Collections.Generic;
using Game.Event.Engine;
using JsonLibrary;
using Snake2D.Util.Sets;

namespace Game.Event.Actions
{
    internal class _EVENT : EventActionConstructor
    {
        _EVENT() : base("EVENT")
        {
        }

        public override EventAction Action(Data data)
        {
            return new Imp(Key, data.Parent, data.Json, data.All, data.Engine);
        }

        public class Imp : EventAction
        {
            public readonly Event Other;
            private readonly bool KeepInfo;
            private readonly bool ClearContent;
            private readonly bool Message;
            private readonly bool Duration;

            public Imp(string key, Event parent, Json data, LISTE<EventAction> all, EventCollection engine) : base(key, all)
            {
                Other = engine.Read(parent, data.Value("EVENT"), data, "EVENT");
                KeepInfo = data.Bool("KEEP_INFO", false);
                ClearContent = data.Bool("CLEAR_CONTEXT", false);
                Message = data.Bool("MESSAGE", true);
                Duration = data.Bool("KEEP_TIME", false);
                data.CheckUnused();
            }

            public override void Exe(Event event, EContext data)
            {
                if (Other != null)
                    GAME.EVENT().Set(Other, KeepInfo, Duration, ClearContent, Message);
            }
        }
    }
}