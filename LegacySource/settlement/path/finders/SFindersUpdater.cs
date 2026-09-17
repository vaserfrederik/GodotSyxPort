using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Path.Finders
{
    public class SFindersUpdater
    {
        private readonly IUpdater upper;
        public const int Quad = 16;
        private const int W = SETT.TWIDTH / Quad;
        private const int H = SETT.THEIGHT / Quad;

        public SFindersUpdater()
        {
            int am = SFinderFindableMap.W * SFinderFindableMap.H;

            upper = new IUpdater(am, TIME.Days().BitSeconds() * 2)
            {
                Update = (i, timeSinceLast) =>
                {
                    bool d = (TIME.Days().BitsSinceStart() & 1) == 1;
                    foreach (var a in SFinderFindable.All)
                    {
                        a.Map.Update(i, d);
                    }
                }
            };
        }

        public void Update(double ds)
        {
            upper.Update(ds);
        }

        public readonly SAVABLE Saver = new SAVABLE
        {
            Save = (file) =>
            {
                file.WriteInt(SFinderFindable.All.Count);
                foreach (var a in SFinderFindable.All)
                {
                    a.Map.Save(file);
                }
            },

            Load = (file) =>
            {
                int am = file.ReadInt();
                if (am != SFinderFindable.All.Count)
                {
                    for (int i = 0; i < am; i++)
                    {
                        SFinderFindable.All[0].Map.Load(file);
                    }
                    Clear();
                }
                else
                {
                    foreach (var a in SFinderFindable.All)
                    {
                        a.Map.Load(file);
                    }
                }
            },

            Clear = () =>
            {
                for (int i = 0; i < SFinderFindable.All.Count; i++)
                {
                    SFinderFindable.All[i].Map.Clear();
                }
            }
        };
    }
}