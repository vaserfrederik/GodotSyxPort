using System;
using System.Collections.Generic;
using init.type;
using settlement.main;
using settlement.room.service.module;
using settlement.room.service.nursery;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.sets;
using util.text;

public sealed class StatsService : StatCollection
{
    public readonly LIST<StatServiceImp> ALL;
    public readonly LIST<StatServiceRoom> ROOMS;

    private readonly ArrayList<ArrayListGrower<StatService>> needMap = new ArrayList<ArrayListGrower<StatService>>(NEEDS.ALL().Count);
    private readonly ArrayListGrower<StatServiceImp> allNeeds = new ArrayListGrower<StatServiceImp>();
    private double[] needTot;
    public readonly StatServiceSimple skinnyDip;
    public readonly StatServiceBench bench;
    public readonly StatServiceHospital hospital;
    public readonly LIST<StatServiceChild> nurseries;
    // public readonly LIST<StatServiceChild> schools;
    private static readonly CharSequence ¤¤name = "Services";
    private static readonly CharSequence ¤¤descc = "Services are provided by building service rooms and allowing access to your subjects.";

    static
    {
        D.ts(typeof(StatsService));
    }

    public StatsService(StatsInit init) : base(init, "SERVICE", ¤¤name, ¤¤descc)
    {
        ArrayListGrower<StatServiceImp> all = new ArrayListGrower<StatServiceImp>();
        ArrayListGrower<StatServiceRoom> rooms = new ArrayListGrower<StatServiceRoom>();

        foreach (RoomServiceAccess a in RoomServiceAccess.ALL())
        {
            rooms.add(new StatServiceRoom(all, a, init));
        }

        skinnyDip = new StatServiceSkinny(all, init);
        bench = new StatServiceBench(all, init);
        this.ROOMS = rooms;

        hospital = new StatServiceHospital(all, SETT.ROOMS().HOSPITAL, init);

        // ArrayListGrower<StatServiceChild> schools = new ArrayListGrower<StatServiceChild>();
        // foreach (ROOM_SCHOOL s in SETT.ROOMS().SCHOOLS)
        // {
        //     schools.add(new StatServiceChild(all, s, init));
        // }
        // this.schools = schools;

        ArrayListGrower<StatServiceChild> nurs = new ArrayListGrower<StatServiceChild>();
        foreach (ROOM_NURSERY s in SETT.ROOMS().NURSERIES)
        {
            nurs.add(new StatServiceChild(all, s, init));
        }
        this.nurseries = nurs;

        this.ALL = all;

        while (needMap.HasRoom())
            needMap.add(new ArrayListGrower<StatService>());

        foreach (StatServiceImp s in ALL)
        {
            if (s.need != null)
            {
                needTot[s.need.index()] += s.usage;
                needMap.get(s.need.index()).add(s);
                allNeeds.add(s);
            }
        }
    }

    private LIST<StatService> shrine;
    private LIST<StatService> temple;

    public LIST<StatService> perNeed(NEED n)
    {
        if (shrine == null)
            shrine = new ArrayList<StatService>(STATS.RELIGION().SHRINE);
        if (temple == null)
            temple = new ArrayList<StatService>(STATS.RELIGION().TEMPLE);
        if (n == NEEDS.TYPES().SHRINE)
            return shrine;
        else if (n == NEEDS.TYPES().TEMPLE)
            return temple;
        return needMap.get(n.index());
    }

    public LIST<StatServiceImp> allNeeds()
    {
        return allNeeds;
    }

    public double needTot(NEED n)
    {
        return needTot[n.index()];
    }
}