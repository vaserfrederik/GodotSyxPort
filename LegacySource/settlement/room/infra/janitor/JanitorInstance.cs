using System;
using System.IO;
using settlement.room.infra.janitor;
using init.resources;
using settlement.main;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.util;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.rendering;

namespace settlement.room.infra.janitor
{
    [Serializable]
    public sealed class JanitorInstance : RoomInstance, JOBMANAGER_HASER
    {
        private static readonly long serialVersionUID = 1L;
        public bool searchForJobs = true;
        public long tableRes = 0;
        public bool auto = true;
        public readonly short rx, ry;
        public BITS bits = new BITS();

        protected JanitorInstance(ROOM_JANITOR b, TmpArea area, RoomInit init) : base(b, area, init)
        {
            employees().maxSet((int)blueprintI().constructor.workers.get(this));
            employees().neededSet((int)Math.Ceiling(blueprintI().constructor.workers.get(this) / 5.0));
            activate();
            int x = 0, y = 0;
            foreach (COORDINATE c in body())
            {
                if (is(c) && SETT.ROOMS().fData.tile.is(c, b.constructor.ta))
                {
                    x = c.x();
                    y = c.y();
                }
            }
            rx = (short)x;
            ry = (short)y;
            bits = new BITS();
        }

        protected override bool loadExtra(FileGetter file)
        {
            if (bits.fetchAms == null)
                bits.fetchAms = new Bitsmap1D(0, 5, RESOURCES.ALL().size());
            return base.loadExtra(file);
        }

        protected override void loadFix()
        {
            if (bits == null || !RESOURCES.map().loader().isSame())
                bits = new BITS();

            if (bits.fetchAms == null)
                bits.fetchAms = new Bitsmap1D(0, 5, RESOURCES.ALL().size());

            base.loadFix();
        }

        public override bool isBadMaintenanceTile(int tx, int ty)
        {
            return tx == rx && ty == ry;
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.lit();
            return base.render(r, shadowBatch, it);
        }

        protected override void activateAction()
        {
        }

        protected override void deactivateAction()
        {
        }

        protected override void updateAction(double updateInterval, bool day)
        {
            searchForJobs = true;
            bits.update();
        }

        public override JOB_MANAGER getWork()
        {
            return blueprintI().jm.get(this);
        }

        protected override void dispose()
        {
            foreach (RESOURCE res in RESOURCES.ALL())
            {
                if (bits.resAm(res) > 0)
                {
                    SETT.THINGS().resources.create(rx, ry, res, bits.resAm(res));
                }
            }
        }

        public override ROOM_JANITOR blueprintI()
        {
            return (ROOM_JANITOR)blueprint();
        }
    }
}