using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Law.Guard
{
    public class Patrols : SAVABLE
    {
        private readonly Patrol[] patrols = new Patrol[16];
        private bool debug = false;

        private readonly IntegerStack free = new IntegerStack(patrols.Length * Patrol.MAX);

        private readonly Addable a = new Addable(false, true)
        {
            InitAbove = (data) =>
            {
                COLOR.RED100.Bind();
                for (int pi = 0; pi < patrols.Length; pi++)
                {
                    Patrol p = patrols[pi];
                    COLOR.UNIQUE.Get(pi).Bind();
                    for (int i = 0; i < p.Posses(); i++)
                    {
                        Coo coo = p.Pos(i);
                        int px = data.TransformGX(coo.X() - C.TILE_SIZEH);
                        int py = data.TransformGY(coo.Y() - C.TILE_SIZEH);
                        SPRITES.Cons().BIG.Outline.Render(CORE.Renderer(), 0, px, py);
                    }
                }

                COLOR.Unbind();
                base.InitAbove(data);
                Add();
            }
        };

        public Patrols()
        {
            for (int i = 0; i < patrols.Length; i++)
                patrols[i] = new Patrol();

            free.Fill();

            IDebugPanelSett.Add("show patrols", new ACTION
            {
                Exe = () =>
                {
                    Coo coo = patrols[0].Pos(0);
                    VIEW.S().GetWindow().Centerer.Set(coo.X(), coo.Y());
                    debug = true;
                }
            });
        }

        public CLICKABLE DebugButt()
        {
            return new GButt.ButtPanel(UI.Icons().M.Crossair)
            {
                Pi = 0,

                ClickA = () =>
                {
                    if (Pi >= patrols.Length)
                        Pi = 0;
                    Patrol p = patrols[Pi];
                    Coo coo = p.Pos(0);
                    VIEW.S().GetWindow().Centerer.Set(coo.X(), coo.Y());
                    debug = true;
                }
            };
        }

        public void Update(double ds)
        {
            foreach (Patrol p in patrols)
                p.Update(ds);
            if (debug)
                a.Add();
        }

        public int ReservePosition()
        {
            if (!free.IsEmpty())
            {
                int p = free.Pop();
                return p;
            }
            return -1;
        }

        public void ReturnPosition(int pos)
        {
            free.Push(pos);
        }

        public Coo Pos(int position)
        {
            int p = position / Patrol.MAX;
            int pp = position % Patrol.MAX;

            return patrols[p].Pos(pp);
        }

        public DIR Dir(int position)
        {
            int p = position / Patrol.MAX;
            int pp = position % Patrol.MAX;
            return patrols[p].Dir(pp);
        }

        public override void Save(FilePutter file)
        {
            free.Save(file);
            foreach (Patrol p in patrols)
                p.Save(file);
        }

        public override void Load(FileGetter file)
        {
            free.Load(file);
            foreach (Patrol p in patrols)
                p.Load(file);
        }

        public override void Clear()
        {
            free.Clear();
            free.Fill();
            foreach (Patrol p in patrols)
                p.Clear();
        }
    }
}