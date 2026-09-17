using settlement.main;
using settlement.room.main;
using settlement.room.law.execution;
using snake2d.util.datatypes;

namespace settlement.room.law.execution
{
    public class ExecutionSingle : RoomSingleton
    {
        /**
         * 
         */
        private static readonly long serialVersionUID = 1L;

        protected ExecutionSingle(ROOMS m, RoomBlueprint p) : base(m, p)
        {
        }

        public override ROOM_EXECTUTION BlueprintI()
        {
            return SETT.ROOMS().EXECUTION;
        }

        protected override void AddAction(ROOMA ins)
        {
            foreach (COORDINATE c in ins.body())
            {
                if (ins.Is(c))
                {
                    BlueprintI().stations.Init(c.x(), c.y());
                }
            }
            base.AddAction(ins);
        }

        protected override void RemoveAction(ROOMA ins)
        {
            foreach (COORDINATE c in ins.body())
            {
                if (ins.Is(c))
                {
                    BlueprintI().stations.Dispose(c.x(), c.y());
                }
            }
            base.RemoveAction(ins);
        }

        public override void UpdateTileDay(int tx, int ty)
        {
            BlueprintI().stations.Update(tx, ty);
            base.UpdateTileDay(tx, ty);
        }
    }
}