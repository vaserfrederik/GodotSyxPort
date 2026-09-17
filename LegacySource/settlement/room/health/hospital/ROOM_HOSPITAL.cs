using System;
using System.Collections.Generic;
using game.faction;
using init.value;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using view.sett.ui.room;

public sealed class ROOM_HOSPITAL : RoomBlueprintIns<HospitalInstance>, ROOM_SERVICE_HASER, INDUSTRY_HASER
{
    private RoomService service;
    private Constructor constructor;
    private Industry consumtion;
    private LIST<Industry> indus;
    private ArrayListGrower<Lockable<Faction>> resLocks = new ArrayListGrower<Lockable<Faction>>();

    public ROOM_HOSPITAL(RoomInitData init, RoomCategorySub block) : base(0, init, "_HOSPITAL", block)
    {
        service = new RoomService(this, init, null)
        {
            public FSERVICE service(int tx, int ty)
            {
                return Bed.service(tx, ty);
            }

            public double totalMultiplier()
            {
                return 1;
            }
        };

        constructor = new Constructor(this, init);
        consumtion = new Industry(this, init.data(), null)
        {
            public double consumptionRate(RoomInstance ins, Humanoid h, IndustryResource oo)
            {
                HospitalInstance ii = (HospitalInstance)ins;
                double n = ii.service().load() * ii.service().total();
                if (!ii.fetch[oo.index()])
                {
                    n = 0;
                }
                if (ii.employees().employed() == 0)
                {
                    return 0;
                }
                return n / ii.employees().employed();
            }
        };
        indus = new ArrayList<Industry>(consumtion);

        employment().countInputSet();

        foreach (IndustryResource i in consumtion.ins())
        {
            resLocks.add(GVALUES.FACTION.LOCK.push("ROOM_HOSPITAL_USE_" + i.resource.key, info.name + ": " + i.resource.name, i.resource.desc, i.resource.icon()));
        }
    }

    protected override void update(double ds)
    {
        // TODO Auto-generated method stub
    }

    public Furnisher constructor()
    {
        return constructor;
    }

    public SFinderRoomService service(int tx, int ty)
    {
        return service.finder;
    }

    public RoomService service()
    {
        return service;
    }

    protected override void saveP(FilePutter file)
    {
        consumtion.save(file);
        service.saver.save(file);
    }

    protected override void loadP(FileGetter saveFile)
    {
        consumtion.load(saveFile);
        service.saver.load(saveFile);
    }

    protected override void clearP()
    {
        consumtion.clear();
        service.saver.clear();
    }

    public void appendView(LISTE<UIRoomModule> mm)
    {
        mm.add(new Gui(this).make());
    }

    public LIST<Industry> industries()
    {
        return indus;
    }

    public DIR layCoo(int tx, int ty)
    {
        return DIR.ORTHO.get(SETT.ROOMS().fData.spriteData.get(tx, ty) & 0b011);
    }

    public double recoverRate(int tx, int ty)
    {
        double bo = get(tx, ty).quality();
        if (Bed.res1(tx, ty))
        {
            bo += 1;
        }
        if (Bed.res2(tx, ty))
        {
            bo += 1;
        }
        return 1 - 0.8 / (bo);
    }

    public double industryFormatConsumptionRate(GText text, IndustryResource i, RoomInstance ins)
    {
        HospitalInstance ii = (HospitalInstance)ins;
        double n = ii.service().load() * ii.service().total();
        if (!ii.fetch[i.index()])
        {
            n = 0;
        }
        GFORMAT.f0(text, -n);
        return n;
    }

    public void industryHoverConsumptionRate(GBox b, IndustryResource i, RoomInstance ins)
    {
    }
}