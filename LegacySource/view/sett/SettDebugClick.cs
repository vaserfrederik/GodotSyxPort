using System.Collections.Generic;

namespace View.Sett
{
    public abstract class SettDebugClick
    {
        static LinkedList<SettDebugClick> all = new LinkedList<SettDebugClick>();

        static SettDebugClick()
        {
            GameDisposable.Add(new GameDisposable()
            {
                protected override void Dispose()
                {
                    all.Clear();
                }
            });
        }

        public abstract bool Debug(int px, int py, int tx, int ty);

        public void Add()
        {
            all.AddLast(this);
        }
    }
}