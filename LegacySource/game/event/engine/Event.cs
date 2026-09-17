using System;
using System.Collections.Generic;
using game;
using game.event.actions;
using snake2d.util.file;
using snake2d.util.sets;
using util.text;

namespace game.event.engine
{
    public sealed class Event
    {
        static readonly ArrayListGrower<Event> all = new ArrayListGrower<Event>();
        static Event()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    all.Clear();
                }
            };
        }

        int savedIndex;
        public readonly int allIndex;
        public readonly string key;

        public ETags tags;
        public EOccurence occurence;
        public EInfo info;
        public EDuration duration;

        public ESelection selection;
        public ECondition condition;
        public LIST<EChoice> choices;
        public LIST<EventAction> on_spawn;
        public LIST<ECondition> aborters;

        readonly ArrayListGrower<EventAction> allActions = new ArrayListGrower<EventAction>();

        public Event(LISTE<Event> coll, string key, Json data, Json text) : base()
        {
            this.key = key;
            allIndex = all.Add(this);
            if (allIndex >= short.MaxValue)
                throw new RuntimeException("Too many events!");
            savedIndex = allIndex;
            info = new EInfo(data, text);

            coll.Add(this);
        }

        public void Read(Json data, Json text, EventActions actions, EventCollection engine)
        {
            on_spawn = EActions.actions("ON_SPAWN", this, null, actions, data, false);
            occurence = new EOccurence(data, engine, this);
            duration = new EDuration(data, actions, this);
            data.Has("ICON");

            tags = new ETags(data);

            if (data.Has("CHOICES"))
            {
                Json[] js = data.Jsons("CHOICES");

                CharSequence[] names;
                if (js.Length == 0)
                    names = new CharSequence[0];
                else if (text == null || !text.Has("CHOICES"))
                {
                    if (js.Length <= 2)
                    {
                        names = new CharSequence[]
                        {
                            Dic.¤¤Accept,
                            Dic.¤¤Decline
                        };
                    }
                    else
                    {
                        names = new CharSequence[0];
                    }
                }
                else
                {
                    names = text.Texts("CHOICES");
                }

                ArrayList<EChoice> cs = new ArrayList<EChoice>(js.Length);

                for (int i = 0; i < js.Length; i++)
                {
                    cs.Add(new EChoice(this, i, actions, js[i], i < names.Length ? names[i] : ("" + i)));
                }
                choices = cs;
            }
            else
            {
                choices = new ArrayList<EChoice>(0);
            }
            if (data.Has("CONDITION"))
            {
                condition = new ECondition("CONDITION", data, actions, this);
            }
            if (data.Has("ABORTS"))
            {
                Json[] jj = data.Jsons("ABORTS", 0);
                ArrayList<ECondition> aborters = new ArrayList<ECondition>(jj.Length);
                foreach (Json j in jj)
                {
                    aborters.Add(new ECondition(null, j, actions, this));
                }
                this.aborters = aborters;
            }
            else
            {
                aborters = new ArrayList<ECondition>(0);
            }

            selection = new ESelection(this, actions, data);
            data.CheckUnused();
        }

        public LIST<EventAction> Actions()
        {
            return allActions;
        }
    }
}