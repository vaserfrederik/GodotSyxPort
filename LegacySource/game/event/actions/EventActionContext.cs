using System;
using System.Collections.Generic;

namespace Game.Event.Actions
{
    public class EventActionContext
    {
        private EventActionContext()
        {
        }

        public static int[] MakeData(Event a, int[] old)
        {
            int ii = Make(a, 0);

            if (old == null || old.Length < ii)
            {
                old = Alloc.Ii(ii + 16);
            }
            return old;
        }

        private static int Make(Event eventObj, int ii)
        {
            if (ii < 0)
            {
                throw new Errors.DataError("Something is wrong with event " + eventObj.Key + ". Either the event chain is too long, or the event has a cyclic behaviour.");
            }

            for (int ai = 0; ai < eventObj.Actions.Size; ai++)
            {
                EventAction a = eventObj.Actions.Get(ai);
                foreach (CInt i in a.Ints)
                {
                    i.Di = ii++;
                }
            }

            for (int ai = 0; ai < eventObj.Actions.Size; ai++)
            {
                EventAction a = eventObj.Actions.Get(ai);
                if (a is _EVENT.Imp)
                {
                    ii = Make((( _EVENT.Imp)a).Other, ii);
                }
            }

            return ii;
        }

        public static void SetData(Event eventObj, EContext con)
        {
            for (int ai = 0; ai < eventObj.Actions.Size; ai++)
            {
                EventAction a = eventObj.Actions.Get(ai);
                a.SetContext(eventObj, con);
            }

            // for (int ai = 0; ai < eventObj.Actions.Size; ai++)
            // {
            //     EventAction a = eventObj.Actions.Get(ai);
            //     if (a is _EVENT.Imp)
            //     {
            //         SetData((( _EVENT.Imp)a).Other, con);
            //     }
            // }
        }

        static void Check(List<Event> events)
        {
            foreach (Event o in events)
            {
                for (int ai = 0; ai < o.Actions.Size; ai++)
                {
                    EventAction a = o.Actions.Get(ai);
                    if (a is _EVENT.Imp && ((_EVENT.Imp)a).Other == o)
                    {
                        throw new Errors.DataError("Event: " + o.Key + " Has a cyclic nature and this is bad!");
                    }
                }
            }
        }
    }
}