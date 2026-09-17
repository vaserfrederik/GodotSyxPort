using System;
using System.Collections.Generic;
using System.IO;
using game.time;
using init.constant;
using init.race;
using settlement.main;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.law;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.service.arena;
using settlement.room.service.module;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data;
using util.info;
using util.text;
using view.sett.ui.room;

public sealed class ROOM_FIGHTPIT : RoomBlueprintIns<ArenaInstance>, ROOM_SERVICE_NEED_HASER, ROOM_SPECTATOR.ROOM_SPECTATOR_HASER, PUNISHMENT_SERVICE
{
    public static readonly string ¤¤kill = "Strength and Honor";
    public static readonly string ¤¤killD = "Enable prisoners condemned to die to do so on the sands of the arena.";

    static
    {
        D.ts(typeof(ROOM_FIGHTPIT));
    }

    private readonly RoomServiceNeed data;
    private readonly Service ser;
    private const int EXECUTIONS = 1;
    private readonly ArenaConstructor constructor;

    private int gladiators = 0;
    private int gladiatorMax = 0;

    private readonly BooleanOEImp<Race> permission;

    public ROOM_FIGHTPIT(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
    {
        ser = new Service(this);
        data = new RoomServiceNeed(this, init)
        {
            service = (tx, ty) => ser.get(tx, ty)
        };
        constructor = new ArenaConstructor(this, init);
        employment().setShiftStart(ROOM_SPECTATOR.WORK_STARTSD, false);
        permission = new BooleanOEImp<Race>(RACES.all().size(), true);
        permission.info = new INFO(¤¤kill, ¤¤killD);
    }

    protected override void update(double ds)
    {
        // TODO Auto-generated method stub
    }

    public override void appendView(LISTE<UIRoomModule> mm)
    {
        mm.add(new RoomArenaGui(work));
    }

    void incG(int g, int m)
    {
        gladiators += g;
        gladiatorMax += m;
    }

    public override Furnisher constructor()
    {
        return constructor;
    }

    public override SFinderRoomService service(int tx, int ty)
    {
        return data.finder;
    }

    protected override void saveP(FilePutter saveFile)
    {
        data.saver.save(saveFile);
        saveFile.i(gladiators);
        saveFile.i(gladiatorMax);
        permission.save(saveFile);
    }

    protected override void loadP(FileGetter saveFile) throws IOException
    {
        data.saver.load(saveFile);
        gladiators = saveFile.i();
        gladiatorMax = saveFile.i();

        gladiators = 0;
        for (int i = 0; i < instancesSize(); i++)
        {
            ArenaInstance ins = getInstance(i);
            gladiators += ins.gladiators;
        }
        permission.load(saveFile);
    }

    protected override void clearP()
    {
        data.saver.clear();
        gladiators = 0;
        gladiatorMax = 0;
        permission.clear();
    }

    public override RoomServiceNeed service()
    {
        return data;
    }

    private readonly ROOM_SPECTATOR spec = new ROOM_SPECTATOR()
    {
        private Coo coo = new Coo();

        public RoomServiceAccess service()
        {
            return ROOM_FIGHTPIT.this.service();
        }

        public COORDINATE lookAt(int sx, int sy)
        {
            ArenaInstance ins = getter.get(sx, sy);
            if (ins == null)
                coo.set(sx, sy);
            else
            {
                coo.set(sx, sy);
                RECTANGLE rec = work.gladiatorArea(sx, sy);
                int w = Math.Min(4, rec.width());
                int h = Math.Min(4, rec.height());
                int a = w * h;
                int i = (sx + sy) % a;
                coo.set(rec.cX() - w / 2 + (i % (w)), rec.cY() - h / 2 + (i / h));
            }
            coo.set(coo.x() * C.TILE_SIZE + C.TILE_SIZEH, coo.y() * C.TILE_SIZE + C.TILE_SIZEH);
            return coo;
        }

        public bool is(int sx, int sy)
        {
            ArenaInstance ins = getter.get(sx, sy);
            return ins != null;
        }

        private int activity(int sx, int sy)
        {
            ArenaInstance ins = getter.get(sx, sy);
            if (ins == null)
                return 0;

            int d = (int)TIME.currentSecond() - ins.cheerTime;

            if (d > ArenaInstance.CHEER_TIME * 8)
            {
                ins.cheerTime = (int)TIME.currentSecond();
                ins.cheer = false;
                d = 0;
            }

            if (d <= ArenaInstance.CHEER_TIME)
            {
                if (ins.cheer)
                    return 1;
                return 2;
            }
            return 0;
        }

        public bool shouldCheer(int sx, int sy)
        {
            return activity(sx, sy) == 1;
        }

        public bool shouldBoo(int sx, int sy)
        {
            return activity(sx, sy) == 2;
        }

        public COORDINATE getDestination(COORDINATE roomT)
        {
            coo.set(roomT.x(), roomT.y());
            return coo;
        }

        public bool isSpot(int tx, int ty)
        {
            if (ser.init(tx, ty))
                return true;
            return base.isSpot(tx, ty);
        }

        public bool isOpenNow()
        {
            return TIME.hours().bitCurrent() > 11 || TIME.hours().bitCurrent() < 6;
        }

        public bool isActive(int sx, int sy)
        {
            ArenaInstance ins = getter.get(sx, sy);
            return ins != null && ins.employees().employed() > 0;
        }
    };

    public override ROOM_SPECTATOR spec()
    {
        return spec;
    }

    private readonly Coo coo = new Coo();
    private readonly Rec aArea = new Rec();

    public static bool gamesAreHeld()
    {
        return TIME.hours().bitCurrent() >= 8;
    }

    public override int punishTotal()
    {
        return gladiatorMax;
    }

    public override int punishUsed()
    {
        return gladiators;
    }

    public override BOOLEAN_OE<Race> punishEnabled()
    {
        return permission;
    }

    public readonly RoomArenaWork work = new RoomArenaWork()
    {
        public void unreserveDeath(int tx, int ty)
        {
            ArenaInstance ins = getter.get(tx, ty);
            if (ins != null)
                ins.reserveGladiator(-1);
        }

        public RoomInstance reserveDeath(int tx, int ty)
        {
            ArenaInstance ins = getter.get(tx, ty);
            if (ins != null && SETT.ROOMS().fData.tileData.get(tx, ty) == ArenaConstructor.ARENA)
                return ins;
            return null;
        }

        public COORDINATE gladiatorGetSpot(RoomInstance i)
        {
            ArenaInstance ins = (ArenaInstance)i;
            int w = ins.body().width() - ins.ax * 2;
            int h = ins.body().height() - ins.ay * 2;
            coo.set(ins.body().x1() + ins.ax + RND.rInt(w), ins.body().y1() + ins.ay + RND.rInt(h));
            if (!gladiatorInArena(coo.x(), coo.y()))
                throw new RuntimeException("" + coo);
            return coo;
        }

        public void gladiatorDrawMakeSheer(COORDINATE coo)
        {
            ArenaInstance ins = getter.get(coo);
            if (ins != null)
            {
                ins.cheerTime = (int)TIME.currentSecond();
                ins.cheer = !RND.oneIn(6);
            }
        }

        public RECTANGLE gladiatorArea(int tx, int ty)
        {
            ArenaInstance ins = getter.get(tx, ty);
            if (ins == null)
                return null;
            aArea.setDim(ins.body().width() - ins.ax * 2, ins.body().height() - ins.ay * 2);
            aArea.moveX1Y1(ins.body().x1() + ins.ax, ins.body().y1() + ins.ay);
            return aArea;
        }

        public int executions(RoomInstance ins)
        {
            if (ins is ArenaInstance i)
                return i.gladiators;
            return 0;
        }

        public int executionsMax(RoomInstance ins)
        {
            return EXECUTIONS;
        }

        public int executions()
        {
            return gladiators;
        }

        public int executionsMax()
        {
            return gladiatorMax;
        }
    };
}