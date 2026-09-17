using System;
using System.Collections.Generic;

namespace Settlement.Battle.Invasion
{
    public abstract class InvasionListener : GameDisposable
    {
        private static readonly List<InvasionListener> all = new List<InvasionListener>();

        static InvasionListener()
        {
            new GameDisposable
            {
                Dispose = () => all.Clear()
            };
        }

        public InvasionListener()
        {
            all.Add(this);
        }

        protected abstract void Register(WArmy a, int reference);
        protected abstract void Defeat(int losses, int kills, int reference);
        protected abstract void Victory(int losses, int kills, int reference);
        protected abstract void Weirdness(int reference);
    }
}