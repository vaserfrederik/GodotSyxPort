using System;
using settlement.main;
using settlement.room.infra.logistics;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using util.colors;
using util.rendering;

namespace settlement.overlay
{
    public sealed class OverlayPull : Addable
    {
        private MoveOrderPullInstance special;

        public OverlayPull() : base(null, null, null, null, true, false)
        {
            exclusive = true;
        }

        public void Add(MoveOrderPullInstance ins)
        {
            base.Add();
            this.special = ins;
        }

        public override void RenderBelow(Renderer r, RenderIterator it)
        {
            COLOR c = COLOR.WHITE10;

            Room room = SETT.ROOMS().map.Get(it.tx(), it.ty());
            if (room != null && room != special && room is MoveJob.ROOM_MOVE_SOURCE)
            {
                MoveJob.ROOM_MOVE_SOURCE s = (ROOM_MOVE_SOURCE)room;
                if (s.moveCapacity().Has(special.moveOrderPullAccepted()))
                {
                    c = GCOLOR.MAP().OVERLAY_GOOD;
                }
                else
                {
                    c = GCOLOR.MAP().OVERLAY_BAD;
                }
            }

            RenderUnder(c, r, it);
        }

        public override void FinishBelow()
        {
            special = null;
        }
    }
}