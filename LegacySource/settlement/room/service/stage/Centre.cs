using System;
using settlement.room.service.stage;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.misc.util;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.service.stage
{
    final class Centre
    {
        private readonly Bits dused = new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001);
        private readonly Bits dreserved = new Bits(0b0000_0000_0000_0000_0000_0000_0000_0010);
        private StageInstance ins;
        private readonly Coo coo = new Coo();
        private int data;
        private readonly ROOM_STAGE b;

        Centre(ROOM_STAGE b)
        {
            this.b = b;
        }

        public SETT_JOB job(int tx, int ty)
        {
            ins = b.getter.get(tx, ty);
            if (ins != null && SETT.ROOMS().fData.tileData.get(tx, ty) == StageConstructor.STATION)
            {
                coo.set(tx, ty);
                data = SETT.ROOMS().data.get(tx, ty);
                return job;
            }
            return null;
        }

        public FSERVICE service(int tx, int ty)
        {
            ins = b.getter.get(tx, ty);
            if (ins != null && ins.body().cX() == tx && ins.body().cY() == ty)
            {
                coo.set(tx, ty);
                data = SETT.ROOMS().data.get(tx, ty);
                return service;
            }
            return null;
        }

        private void save()
        {
            // int ndata = data;
            // data = SETT.ROOMS().data.get(coo);
            // if (service.findableReservedCanBe())
            // {
            //     ins.service.report(service, ins.blueprintI().data, -dservices.get(data));
            // }
            // data = ndata;
            // if (service.findableReservedCanBe())
            // {
            //     ins.service.report(service, ins.blueprintI().data, dservices.get(data));
            // }
            SETT.ROOMS().data.set(ins, coo, data);
        }

        private readonly FSERVICE service = new FSERVICE
        {
            Consume = () => { }
            ,
            X = () => ins.body().cX()
            ,
            Y = () => ins.body().cY()
            ,
            FindableReservedCanBe = () => ins.services() > 0
            ,
            FindableReserve = () =>
            {
                if (!FindableReservedCanBe())
                {
                    throw new RuntimeException();
                }
                ins.incServices(-1);
            }
            ,
            FindableReservedIs = () => ins.hasService()
            ,
            FindableReserveCancel = () => ins.incServices(1)
        };

        private readonly SETT_JOB job = new SETT_JOB
        {
            JobUseTool = () => false
            ,
            JobStartPerforming = () =>
            {
                data = dused.set(data, 1);
                save();
            }
            ,
            JobSound = () => null
            ,
            JobResourceBitToFetch = () => null
            ,
            JobReservedIs = (RESOURCE r) => dreserved.get(data) == 1
            ,
            JobReserveCancel = (RESOURCE r) =>
            {
                data = dused.set(data, 0);
                data = dreserved.set(data, 0);
                save();
            }
            ,
            JobReserveCanBe = () => !JobReservedIs(null)
            ,
            JobReserve = (RESOURCE r) =>
            {
                data = dreserved.set(data, 1);
                save();
            }
            ,
            JobPerformTime = (Humanoid a) => 0
            ,
            JobPerform = (Humanoid skill, RESOURCE r, int rAm) =>
            {
                JobReserveCancel(r);
                return null;
            }
            ,
            JobName = () => b.employment().verb
            ,
            JobCoo = () => coo
        };
    }
}