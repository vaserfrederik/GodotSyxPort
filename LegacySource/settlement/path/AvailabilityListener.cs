using System;
using System.Collections.Generic;

namespace Settlement.Path
{
    public abstract class AvailabilityListener
    {
        private static readonly List<AvailabilityListener> listeners = new List<AvailabilityListener>(20);

        static AvailabilityListener()
        {
            new GameDisposable
            {
                Dispose = () => listeners.Clear()
            };
        }

        private static bool listening = true;

        public static void ListenAll(bool listening)
        {
            AvailabilityListener.listening = listening;
        }

        public static void Notify(int tx, int ty, AVAILABILITY a, AVAILABILITY old, bool playerChange)
        {
            if (!listening)
                return;

            foreach (AvailabilityListener l in listeners)
                l.Changed(tx, ty, a, old, playerChange);
        }

        protected AvailabilityListener()
        {
            listeners.Add(this);
        }

        protected abstract void Changed(int tx, int ty, AVAILABILITY a, AVAILABILITY old, bool playerChange);
    }
}