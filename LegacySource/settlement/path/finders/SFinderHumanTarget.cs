using System;
using System.Collections.Generic;
using static Settlement.Main.SETT;

namespace Settlement.Path.Finders
{
    public sealed class SFinderHumanTarget
    {
        public SFinderHumanTarget()
        {
            IDebugPanelSett.Add(new PlacableSimpleTile("kill targets")
            {
                private readonly List<Humanoid> all = new List<Humanoid>(1);

                public override void Place(int tx, int ty)
                {
                    all.Clear();
                    Add(all, tx, ty, true, 128, Humanoid.TARGET_MAX);

                    foreach (Humanoid h in all)
                    {
                        if (!h.IsRemoved())
                            h.Kill(true, CAUSE_LEAVES.MURDER());
                    }
                }

                public override string IsPlacable(int tx, int ty)
                {
                    return SETT.PATH().Comps.Zero.Get(tx, ty) != null ? null : "E";
                }
            });
        }

        private FindableDataSingle ff;

        private readonly SCompPatherFinder fin = new SCompPatherFinder
        {
            public override bool IsInComponent(SComponent c, double distance)
            {
                return ff.Get(c) > 0;
            }
        };

        public void Add(List<Humanoid> res, int cx, int cy, bool player, int distance, int targetLimit)
        {
            ff = SETT.PATH().Comps.Data.People(player);
            LIST<SComponent> ls = SETT.PATH().Comps.Pather.Fill(cx, cy, fin, distance).Path();
            foreach (SComponent c in ls)
            {
                int x1 = CLAMP.i((c.CentreX() & ~(c.Level().Size() - 1)) - 1, 0, TWIDTH);
                int x2 = CLAMP.i(x1 + c.Level().Size() + 2, 0, TWIDTH);
                int y1 = CLAMP.i((c.CentreY() & ~(c.Level().Size() - 1)) - 1, 0, THEIGHT);
                int y2 = CLAMP.i(y1 + c.Level().Size() + 2, 0, THEIGHT);
                bool found = false;
                for (int y = y1; y < y2; y++)
                {
                    for (int x = x1; x < x2; x++)
                    {
                        foreach (ENTITY e in SETT.ENTITIES().GetAtTile(x, y))
                        {
                            if (e is Humanoid)
                            {
                                Humanoid h = (Humanoid)e;
                                if (h.Indu().Hostile() != player)
                                {
                                    found = true;
                                    if (h.Targets() < targetLimit)
                                    {
                                        res.Add(h);
                                        targetLimit--;
                                        if (targetLimit <= 0 || !res.HasRoom())
                                            return;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!found)
                    GAME.Notify(c.CentreX() + " " + c.CentreY() + " " + ff.Name + " " + x1 + " " + y1);

                if (targetLimit <= 0 || !res.HasRoom())
                    return;
            }
        }
    }
}