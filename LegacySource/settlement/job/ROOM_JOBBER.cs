using init.resources;
using init.sprite;
using settlement.main;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.colors;
using util.rendering;

namespace settlement.job
{
    public interface ROOM_JOBBER : AREA
    {
        /**
         * 
         * @param tx
         * @param ty
         * @return false job will dissapear before this. Must place anew if continue you wish (yoda)
         */
        void jobFinsih(int tx, int ty, RESOURCE r, int ram);

        void jobToggle(bool toggle);

        bool jobToggleIs();

        default void jobSet(int tx, int ty, bool active, RESOURCE res)
        {
            bool dd = SETT.JOBS().planMode.is();

            SETT.JOBS().planMode.set(active);

            if (res == null)
                Placer.place(tx, ty, SETT.JOBS().room);
            else
                Placer.place(tx, ty, SETT.JOBS().rooms[res.bIndex()]);
            if (!active)
                PlacerDormant.place(tx, ty);
            else
                PlacerActivate.place(tx, ty);

            SETT.JOBS().planMode.set(dd);
        }

        static void render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it, bool repairing)
        {
            if (repairing)
            {
                it.lit();
                GCOLOR.MAP().JOB_ACTIVE.bind();
                SPRITES.cons().BIG.dashed.render(r, 0x0F, it.x(), it.y());
                COLOR.unbind();
            }
        }

        default void jobClear(int tx, int ty)
        {
            PlacerDelete.place(tx, ty);
        }

        static ROOM_JOBBER get(int tx, int ty)
        {
            Room r = SETT.ROOMS().map.get(tx, ty);
            if (r != null && r is ROOM_JOBBER)
                return (ROOM_JOBBER)r;
            return null;
        }

        bool needsFertilityToBeCleared(int tx, int ty);

        default bool needsTerrainToBeCleared(int tx, int ty)
        {
            return true;
        }

        bool becomesSolid(int tx, int ty);

        int totalResourcesNeeded(int x, int y);

        bool isJobActive();
    }
}