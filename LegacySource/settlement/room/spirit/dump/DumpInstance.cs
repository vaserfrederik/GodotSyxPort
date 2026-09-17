using System;
using System.Collections.Generic;
using settlement.room.main;
using settlement.room.service.module;
using snake2d;
using snake2d.util.datatypes;
using util.rendering;
using view.main;

namespace settlement.room.spirit.dump
{
    [Serializable]
    internal class DumpInstance : RoomInstance, ROOM_SERVICER
    {
        private readonly RoomServiceInstance service;

        private static readonly long serialVersionUID = 1L;

        protected DumpInstance(ROOM_DUMP blueprint, TmpArea area, RoomInit init) : base(blueprint, area, init)
        {
            int am = 0;
            foreach (COORDINATE c in body())
            {
                if (Is(c))
                {
                    if (!BlueprintI().Constructor.IsEdge(c.X, c.Y, this))
                    {
                        Dump.Init(this, c.X, c.Y);
                        am++;
                    }
                }
            }
            service = new RoomServiceInstance(am, BlueprintI().Service);
            Activate();
        }

        public override ROOM_DUMP BlueprintI()
        {
            return (ROOM_DUMP)Blueprint();
        }

        protected override void ActivateAction()
        {
            foreach (COORDINATE c in body())
            {
                if (Is(c))
                {
                    Dump.Activate(c.X, c.Y);
                }
            }
            service.ClearLoad();
        }

        protected override void DeactivateAction()
        {
            foreach (COORDINATE c in body())
            {
                if (Is(c))
                {
                    Dump.Deactivate(c.X, c.Y);
                }
            }
        }

        protected override void Dispose()
        {
            service.Dispose(BlueprintI().Service);
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            Dump.Render(r, shadowBatch, i);
            return false;
        }

        protected override bool RenderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            BlueprintI().Constructor.RenderTileBelow(r, shadowBatch, i, true);
            return false;
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            if (day)
                service.UpdateDay();
        }

        public void UpdateTileDay(int tx, int ty)
        {
            Dump d = Dump.Get(tx, ty);
            if (d != null)
                d.Update();
        }

        public RoomServiceInstance Service()
        {
            return service;
        }

        public double Quality()
        {
            return ROOM_SERVICER.DefQuality(this, 1);
        }

        protected override bool CanRemoveAndRemoveAction(int tx, int ty, bool scatter, object obj, bool force)
        {
            if (force || !Prompt())
                return true;
            return false;
        }

        private bool Prompt()
        {
            int time = 0;
            int am = 0;
            foreach (COORDINATE c in body())
            {
                if (Is(c))
                {
                    int t = Dump.DaysTillDecompose(c.X, c.Y);
                    if (t > 0)
                    {
                        am++;
                        if (t > time)
                            time = t;
                    }
                }
            }

            if (am > 0)
            {
                Str.TMP.Clear();
                Str.TMP.Add(ROOM_DUMP.¤¤RemoveProblem);
                Str.TMP.Insert(0, am);
                Str.TMP.Insert(1, time);
                VIEW.Inters().YesNo.Activate(Str.TMP, null, null, false);
                return true;
            }
            return false;
        }
    }
}