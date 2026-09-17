using System;
using System.Collections.Generic;
using init.resources;
using settlement.main;
using settlement.path.finders;
using settlement.room.infra.station;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.main.util;
using view.sett.ui.room;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;

namespace settlement.room.infra.station
{
    public sealed class ROOM_STATION : RoomBlueprintIns<StationInstance>, ROOM_EMPLOY_AUTO
    {
        public const int MAX_EMPLOYEES = 15;

        private readonly Constructor constructor;
        private readonly Job job;
        private readonly Crate crate;
        private readonly StationTally.Total[] tallies;
        private readonly int[] ri;

        public ROOM_STATION(RoomInitData init, RoomCategorySub cat) : base(0, init, "_STATION", cat)
        {
            constructor = new Constructor(this, init);
            tallies = new StationTally.Total[RESOURCES.ALL().size()];
            for (int i = 0; i < tallies.Length; i++)
                tallies[i] = new Total(RESOURCES.ALL().get(i));
            ri = Alloc.ii(RESOURCES.ALL().size());
        }

        protected override void update(double ds)
        {
            
        }

        public override Furnisher constructor()
        {
            return constructor;
        }

        public override SFinderRoomService service(int tx, int ty)
        {
            return null;
        }

        protected override void saveP(FilePutter saveFile)
        {
            RESOURCES.map().saver().save(ri, saveFile);
        }

        protected override void loadP(FileGetter saveFile)
        {
            RESOURCES.map().loader().load(ri, saveFile, 0);

            foreach (Total t in tallies)
                t.clear();

            for (int i = 0; i < instancesSize(); i++)
            {
                StationInstance ins = getInstance(i);
                ins.bamount = new RBITImp();
                ins.bcapacity = new RBITImp();
                ins.tally = new StationTally[RESOURCES.ALL().size()];

                foreach (RESOURCE res in RESOURCES.ALL())
                    ins.tally[res.index()] = new StationTally();
                foreach (COORDINATE c in ins.body())
                {
                    if (ins.is(c) && crate.get(c.x(), c.y()) != null && crate.get(c.x(), c.y()).resource() != null)
                    {
                        ins.tally(crate.resource()).add(crate.resource(), crate, ins);
                    }
                }
            }

            foreach (Total t in tallies)
                t.clear();
            for (int i = 0; i < instancesSize(); i++)
            {
                StationInstance ins = getInstance(i);
                foreach (Total t in tallies)
                    t.add(ins.tally(t.res), ins);
            }

            foreach (Total t in tallies)
                t.debug();
        }

        protected override void clearP()
        {
            
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new Gui(this).make());
        }

        public bool autoEmploy(Room r)
        {
            return ((StationInstance)r).auto;
        }

        public void autoEmploy(Room r, bool b)
        {
            ((StationInstance)r).auto = b;
        }

        public StationTally.Total tally(RESOURCE res)
        {
            return tallies[res.index()];
        }

        private readonly Coo coo = new Coo();

        public COORDINATE reserve(RESOURCE res)
        {
            if (tally(res).accepting() <= 0)
                return null;
            int oi = ri[res.index()];

            for (int i = 0; i < instancesSize(); i++)
            {
                if (oi >= instancesSize())
                    oi = 0;
                StationInstance ins = getInstance(oi);
                oi++;
                if (ins.accepting(res))
                {
                    foreach (COORDINATE c in ins.body())
                    {
                        if (ins.is(c) && (SETT.ROOMS().fData.tileData.get(c) & Constructor.BIT_DEST) != 0)
                        {
                            ins.reserve(res);
                            coo.set(c);
                            return coo;
                        }
                    }
                    throw new RuntimeException();
                }
            }

            int k = 0;
            for (int i = 0; i < instancesSize(); i++)
                k += getInstance(i).accepting(res) ? 1 : 0;

            throw new RuntimeException(res + " " + tally(res).accepting() + " " + k + " " + instancesSize());
        }

        public void reserveCancel(RESOURCE res, int tx, int ty)
        {
            StationInstance ins = get(tx, ty);
            if (ins != null)
                ins.unreserve(res);
        }

        public void deliver(RESOURCE res, int am, int tx, int ty)
        {
            StationInstance ins = get(tx, ty);
            if (ins != null)
                ins.deliver(res, am);
        }

        public double workersPerload(int tx, int ty)
        {
            StationInstance ins = get(tx, ty);
            if (ins == null)
                return MAX_EMPLOYEES;
            double bo = SETT.ROOMS().STOCKPILE.bonus().get(HCLASS_RACE.clP()) / SETT.ROOMS().STOCKPILE.bonus().baseValue;
            return MAX_EMPLOYEES / (bo * ins.efficiency() * MAX_EMPLOYEES);
        }
    }
}