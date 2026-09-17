using System;
using System.Collections.Generic;
using System.IO;

namespace game.events
{
    public class EVENTS : GameResource
    {
        public readonly EventCitizen riot = new EventCitizen();
        public readonly EventUprising uprising = new EventUprising();
        public readonly EventDisease disease = new EventDisease();
        public readonly EventDiseaseMild diseaseM = new EventDiseaseMild();
        public readonly EventKiller killer = new EventKiller();
        public readonly EventTemperature temperature = new EventTemperature();
        public readonly EventAdvisor advice = new EventAdvisor();
        public readonly EventAccident accident = new EventAccident();
        public readonly EventWorld world = new EventWorld();
        public readonly EventWorldRebellion rebellion = new EventWorldRebellion();
        public readonly EventGeneral general = new EventGeneral();

        private readonly SuperSaver<EventResource> saver = new SuperSaver<EventResource>(typeof(EVENTS), all)
        {
            Save = (t, f) => t.Save(f),
            Load = (t, f) => t.Load(f),
            Key = t => t.key,
            Clear = t => t.Clear()
        };

        public EVENTS() : base("EVENTS", false)
        {
        }

        protected override void Save(FilePutter file)
        {
            saver.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            saver.Load(file);
        }

        public void Generate()
        {
            saver.Clear();
        }

        protected override void LoadFail()
        {
            saver.Clear();
        }

        private static LinkedList<EventResource> all = new LinkedList<EventResource>();
        static EVENTS()
        {
            new GameDisposable
            {
                Dispose = () => all = new LinkedList<EventResource>()
            };
        }

        public LIST<EventResource> All()
        {
            return all;
        }

        public abstract class EventResource
        {
            private bool supress;
            public readonly string key;

            protected EventResource(string key)
            {
                all.Add(this);
                this.key = key;
            }

            protected abstract void Update(double ds);

            protected abstract void Save(FilePutter file);

            protected abstract void Load(FileGetter file);

            protected abstract void Clear();

            /**
             * will stop the event from updating.
             */
            public void Supress(bool supress)
            {
                this.supress = supress;
            }
        }

        protected override void Update(double ds, Profiler prof)
        {
            if (!SETT.Exists())
                return;
            prof.LogStart(typeof(EVENTS));
            foreach (EventResource e in all)
                if (!e.supress)
                    e.Update(ds);
            prof.LogEnd(typeof(EVENTS));
        }
    }
}