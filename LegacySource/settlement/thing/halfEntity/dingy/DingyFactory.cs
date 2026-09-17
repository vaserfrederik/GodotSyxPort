using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Thing.HalfEntity.Dingy
{
    using Game.Time;
    using Init.Constant;
    using Init.Resources;
    using Settlement.Entity;
    using Settlement.Entity.Humanoid;
    using Settlement.Main;
    using Settlement.Thing.HalfEntity;
    using Snake2D.Renderer;
    using Snake2D.Util.Maths;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sets;
    using Util.Rendering;
    using View.Sett;
    using View.Tool;

    public class DingyFactory : Factory<Dingy>
    {
        public readonly Sprite Sprite = new Sprite();

        public DingyFactory(LISTE<Factory<?>> all) : base(all)
        {
            PLACABLE pp = new PlacableSimpleTile("Dingy place")
            {
                Place = (tx, ty) =>
                {
                    Humanoid h = H();
                    if (h != null)
                        Make(h, tx, ty, SETT.ROOMS().FISHERIES.Get(0).Industries().Get(0).Outs().Get(0).Resource, RND.rInt(), DIR.ALL.Rnd());
                },

                IsPlacable = (tx, ty) => SETT.TERRAIN().WATER.DEEP.Is(tx, ty) || SETT.TERRAIN().WATER.BRIDGE.Is(tx, ty) ? null : E
            };

            IDebugPanelSett.Add(pp);
        }

        protected override void Save(FilePutter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void Load(FileGetter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void Clear()
        {
            // TODO Auto-generated method stub
        }

        protected override Dingy Make()
        {
            return new Dingy();
        }

        public bool Make(Humanoid h, int tx, int ty, RESOURCE rCatch, int up, DIR dir)
        {
            Dingy e = Create();
            return e.Init(h, tx, ty, rCatch, up, dir);
        }

        public void RenderBoat(Renderer r, ShadowBatch s, int cx, int cy, DIR dir, int ran, int up)
        {
            int x1 = cx - 16 * C.SCALE;
            int y1 = cy - 16 * C.SCALE;

            double sp = 10.0 / (1 + (ran & 0b1111));
            ran = ran >> 4;
            int f = (ran & 0b1111) + (int)(sp * TIME.CurrentSecond());
            ran = ran >> 4;
            int df = MATH.DistanceC(8, f, 16);
            x1 += df;

            sp = 10.0 / (1 + (ran & 0b1111));
            ran = ran >> 4;
            f = (ran & 0b1111) + (int)(sp * TIME.CurrentSecond());
            ran = ran >> 4;
            df = MATH.DistanceC(8, f, 16);
            y1 += df;

            Sprite.Render(r, s, dir.Id(), x1, y1, ran >> 1, up);
        }

        private Humanoid H()
        {
            for (int i = 0; i < SETT.ENTITIES().GetAllEnts().Length; i++)
            {
                ENTITY e = SETT.ENTITIES().GetAllEnts()[i];
                if (e is Humanoid)
                    return (Humanoid)e;
            }
            return null;
        }
    }
}