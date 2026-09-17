using System;
using System.Collections.Generic;
using game.faction;
using init.type.WGROUP;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.job;
using settlement.room.main.util;

namespace settlement.room.main.util
{
    [Serializable]
    public abstract class RoomState
    {
        private static readonly long serialVersionUID = 1L;

        public abstract void Apply(Room r, int tx, int ty);
        public abstract void ApplyRepaired(Room r, int tx, int ty);

        public static class RoomStateInstance : RoomState
        {
            private static readonly long serialVersionUID = 1L;
            private readonly int workersTarget;
            private readonly int industry;
            private bool auto;
            private byte radius;
            private int consumptionMask;
            private string name;
            private readonly HTypeBitsImp pref = new HTypeBitsImp(false);

            public RoomStateInstance(RoomInstance ins)
            {
                this.name = "" + ins.Name();

                pref.Copy(ins.Employees().Preferred());

                this.workersTarget = ins.Employees().HardTarget();
                if (ins.BlueprintI() is ROOM_EMPLOY_AUTO)
                {
                    ROOM_EMPLOY_AUTO a = (ROOM_EMPLOY_AUTO)ins.BlueprintI();
                    auto = a.AutoEmploy(ins);
                }
                if (ins is ROOM_PRODUCER_INSTANCE)
                {
                    industry = ((ROOM_PRODUCER_INSTANCE)ins).IndustryI();
                }
                else
                {
                    industry = 0;
                }
                if (ins is ROOM_RADIUS_INSTANCE)
                    radius = ((ROOM_RADIUS_INSTANCE)ins).RadiusRaw();
                if (ins.BlueprintI() is ROOM_CONSUMPTION_HASER)
                {
                    RoomConsumption ii = ((ROOM_CONSUMPTION_HASER)ins.BlueprintI()).Consumption();
                    for (int i = 0; i < ii.Ins().Count; i++)
                    {
                        if (ii.Enabled(ii.Ins()[i], (ROOM_IDATA_INSTANCE)ins))
                        {
                            consumptionMask |= 1 << i;
                        }
                    }
                }
            }

            public override void Apply(Room room, int tx, int ty)
            {
                if (!(room is RoomInstance))
                    return;

                RoomInstance ins = (RoomInstance)room;
                // ins.name().clear().add(name);
                if (ins.BlueprintI().Employment() != null)
                {
                    ins.Employees().NeededSet(workersTarget);
                }
                if (ins.BlueprintI() is ROOM_EMPLOY_AUTO)
                {
                    ROOM_EMPLOY_AUTO a = (ROOM_EMPLOY_AUTO)ins.BlueprintI();
                    a.AutoEmploy(ins, auto);
                }
                if (ins is ROOM_PRODUCER_INSTANCE && ins.Blueprint() is INDUSTRY_HASER)
                {
                    INDUSTRY_HASER h = (INDUSTRY_HASER)ins.Blueprint();
                    if (industry >= 0 && industry < h.Industries().Size() && h.Industries().Get(industry).Lockable().Passes(FACTIONS.Player()))
                        ((ROOM_PRODUCER_INSTANCE)ins).SetIndustry(industry);
                }
                if (ins is ROOM_RADIUS_INSTANCE)
                    ((ROOM_RADIUS_INSTANCE)ins).RadiusRawSet(radius);
                if (ins.BlueprintI() is ROOM_CONSUMPTION_HASER)
                {
                    RoomConsumption ii = ((ROOM_CONSUMPTION_HASER)ins.BlueprintI()).Consumption();
                    for (int i = 0; i < ii.Ins().Count; i++)
                    {
                        bool e = (consumptionMask & (1 << i)) != 0;
                        if (e != ii.Enabled(ii.Ins()[i], (ROOM_IDATA_INSTANCE)ins))
                        {
                            ii.EnabledToggle(ii.Ins()[i], (ROOM_IDATA_INSTANCE)ins, ins);
                        }
                    }
                }
                ApplyIns(ins);
            }

            protected void ApplyIns(RoomInstance ins)
            {

            }

            public override void ApplyRepaired(Room room, int tx, int ty)
            {
                if (!(room is RoomInstance))
                    return;
                RoomInstance ins = (RoomInstance)room;
                ins.Name().Clear().Add(name);
                if (ins.BlueprintI().Employment() != null)
                {
                    ins.Employees().PreferredSet(pref);
                }
            }
        }

        public static readonly RoomState DUMMY = new RoomState()
        {
            private static readonly long serialVersionUID = 1L;

            public override void Apply(Room r, int tx, int ty)
            {

            }

            public override void ApplyRepaired(Room r, int tx, int ty)
            {
                // TODO Auto-generated method stub

            }
        };
    }
}