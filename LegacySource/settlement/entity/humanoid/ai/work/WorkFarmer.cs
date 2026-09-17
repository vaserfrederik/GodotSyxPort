using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.main;
using settlement.misc.util;
using settlement.room.food.farm;
using util.text;

namespace settlement.entity.humanoid.ai.work
{
    final class WorkFarmer : WorkAbs
    {
        private static readonly CharSequence ¤¤storing = "Storing Harvest";

        static
        {
            D.ts(typeof(WorkFarmer));
        }

        private readonly ROOM_FARM farm;

        protected WorkFarmer(AIModule_Work module, ROOM_FARM farm, PlanBlueprint[] map, Works w) : base(module, farm, map, w)
        {
            this.farm = farm;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            AISubActivation s = base.init(a, d);
            if (s != null)
                return s;

            RESOURCE_TILE res = farm.toStore(work(a).mX(), work(a).mY());
            if (res == null)
                return null;

            int resX = res.x();
            int resY = res.y();
            int reservable = res.reservable();

            TILE_STORAGE st = farm.toStoreTo(work(a).mX(), work(a).mY());
            if (st == null)
                return null;
            d.planTile.set(st.x(), st.y());
            int am = Math.Min(reservable, WorkAbs.maxCarry);
            am = Math.Min(am, st.storageReservable());

            d.planByte1 = (byte)am;
            d.planByte2 = st.resource().bIndex();

            s = AI.SUBS().walkTo.coo(a, d, resX, resY);
            if (s == null)
                return null;

            res = RESOURCE_TILE.GETTER.reservable(ress(d), false, false, resX, resY);
            for (int i = 0; i < am; i++)
                res.findableReserve();

            st = farm.toStoreTo(work(a).mX(), work(a).mY());

            st.storageReserve(am);

            start.set(a, d);
            return s;
        }

        private RESOURCE ress(AIManager d)
        {
            return RESOURCES.ALL().get(d.planByte2);
        }

        private int amount(AIManager d)
        {
            return d.planByte1;
        }

        private TILE_STORAGE storage(AIManager d)
        {
            return SETT.PATH().finders.storage.getter.get(d.planTile);
        }

        private readonly Resumer start = new Resumer(¤¤storing)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                RESOURCE_TILE res = RESOURCE_TILE.GETTER.reserved(ress(d), d.path.destX(), d.path.destY());
                if (res == null)
                {
                    can(a, d);
                    return null;
                }

                for (int i = 0; i < amount(d); i++)
                {
                    if (!res.findableReservedIs())
                        break;
                    res.resourcePickup();
                    d.resourceCarriedSet(ress(d));
                }

                return go.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                RESOURCE_TILE res = RESOURCE_TILE.GETTER.reserved(ress(d), d.path.destX(), d.path.destY());
                for (int i = 0; i < amount(d); i++)
                {
                    if (!res.findableReservedIs())
                        break;
                    res.findableReserveCancel();
                }
                go.can(a, d);
            }
        };

        private readonly Resumer go = new Resumer(¤¤storing)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                TILE_STORAGE st = storage(d);
                if (st == null)
                    return null;

                AISubActivation s = AI.SUBS().walkTo.coo(a, d, d.planTile);

                if (s == null)
                {
                    can(a, d);
                    return null;
                }
                return s;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                TILE_STORAGE st = storage(d);
                if (st == null)
                    return null;

                int am = amount(d);
                am = Math.Min(am, st.storageReserved());
                st.storageDeposit(am);
                d.resourceCarriedSet(null);
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                TILE_STORAGE st = storage(d);
                if (st == null)
                    return;
                int am = amount(d);
                am = Math.Min(am, st.storageReserved());
                st.storageUnreserve(am);
                d.resourceDrop(a);

            }
        };
    }
}