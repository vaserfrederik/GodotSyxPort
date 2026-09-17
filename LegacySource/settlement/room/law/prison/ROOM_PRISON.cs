using System;
using System.Collections.Generic;
using System.IO;
using settlement.main;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.industry.module;
using settlement.room.law;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using view.sett.ui.room;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;

namespace settlement.room.law.prison
{
    public sealed class ROOM_PRISON : RoomBlueprintIns<PrisonInstance>, PUNISHMENT_SERVICE
    {
        public static readonly double WORKER_PER_PRISONER = 1d / 4d;
        private readonly Constructor constructor;
        private int prisonersCurrent;
        private int prisonersMax;
        private readonly Industry indu;

        public ROOM_PRISON(RoomInitData init, RoomCategorySub block) : base(0, init, "_PRISON", block)
        {
            constructor = new Constructor(this, init);
            indu = new Industry(this, RESOURCES.EDI().makeArray(), new double[RESOURCES.EDI().all().size()], null);
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public int punishTotal()
        {
            return prisonersMax;
        }

        public int punishUsed()
        {
            return prisonersCurrent;
        }

        public Furnisher constructor()
        {
            return constructor;
        }

        void incPrisoners(int p, int total)
        {
            prisonersCurrent += p;
            prisonersMax += total;
        }

        protected override void saveP(FilePutter saveFile)
        {
            indu.save(saveFile);
        }

        protected override void loadP(FileGetter saveFile)
        {
            indu.load(saveFile);
            prisonersMax = 0;
            prisonersCurrent = 0;
            foreach (PrisonInstance ins in all())
            {
                prisonersCurrent += ins.prisoners();
                if (ins.active())
                {
                    prisonersMax += ins.prisonersMax();
                }
            }
        }

        protected override void clearP()
        {
            indu.clear();
            prisonersCurrent = 0;
            prisonersMax = 0;
        }

        public SFinderRoomService service(int tx, int ty)
        {
            // TODO Auto-generated method stub
            return null;
        }

        public COORDINATE registerPrisoner(COORDINATE last, COORDINATE current)
        {
            if (prisonersCurrent >= prisonersMax)
                return null;

            if (SETT.IN_BOUNDS(last))
            {
                PrisonInstance ins = getter.get(last);
                if (ins != null && ins.active() && ins.prisoners() < ins.prisonersMax())
                {
                    if (SETT.PATH().comps.superComp.get(current) == SETT.PATH().comps.superComp.get(ins.mX(), ins.mY()))
                    {
                        return ins.registerPrisoner(last);
                    }
                }
            }

            int i = RND.rInt(instancesSize());
            for (int k = 0; k < instancesSize(); k++)
            {
                PrisonInstance ins = getInstance((k + i) % instancesSize());
                if (ins.active() && ins.prisoners() < ins.prisonersMax())
                {
                    if (SETT.PATH().comps.superComp.get(current) == SETT.PATH().comps.superComp.get(ins.mX(), ins.mY()))
                    {
                        return ins.registerPrisoner(last);
                    }
                }
            }
            for (int k = 0; k < instancesSize(); k++)
            {
                PrisonInstance ins = getInstance((k + i) % instancesSize());
                if (ins.active() && ins.prisoners() < ins.prisonersMax())
                {
                    Console.WriteLine("exists!");
                    return null;
                }
            }

            throw new Exception();
        }

        public void unregisterPrisoner(COORDINATE c)
        {
            if (is(c))
            {
                getter.get(c).removePrisoner(c.x(), c.y());
            }
        }

        public FSERVICE getFood(COORDINATE cell)
        {
            if (is(cell))
            {
                for (int di = 0; di < DIR.ORTHO.size(); di++)
                {
                    DIR dir = DIR.ORTHO.get(di);
                    FSERVICE f = Food.init(cell.x() + dir.x(), cell.y() + dir.y());
                    if (f != null)
                    {
                        int dx = cell.x() + dir.perpendicular().x();
                        int dy = cell.y() + dir.perpendicular().y();
                        if (Latrine.init(dx, dy) != null)
                            return f;
                    }
                }
            }
            return null;
        }

        public FSERVICE getLatrine(COORDINATE cell)
        {
            if (is(cell))
            {
                for (int di = 0; di < DIR.ORTHO.size(); di++)
                {
                    DIR dir = DIR.ORTHO.get(di);
                    FSERVICE f = Latrine.init(cell.x() + dir.x(), cell.y() + dir.y());
                    if (f != null)
                    {
                        int dx = cell.x() + dir.perpendicular().x();
                        int dy = cell.y() + dir.perpendicular().y();
                        if (Food.init(dx, dy) != null)
                            return f;
                    }
                }
            }
            return null;
        }

        public bool isWithinCell(int nx, int ny, COORDINATE cell)
        {
            if (is(nx, ny) && is(cell.x(), cell.y()))
            {
                return constructor.isWithinCell(nx, ny, cell.x(), cell.y());
            }
            return false;
        }

        public bool isDoor(COORDINATE cell)
        {
            return is(cell) && SETT.ROOMS().fData.tileData.get(cell) == Constructor.CODE_ENTRANCE;
        }

        public bool isreserved(COORDINATE cell)
        {
            return is(cell) && getter.get(cell).isReserved(cell.x(), cell.y());
        }

        public void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(new Gui(this).make());
        }
    }
}