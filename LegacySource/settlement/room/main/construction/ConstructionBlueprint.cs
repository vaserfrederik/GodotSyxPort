using System;
using System.Collections.Generic;
using settlement.main;
using settlement.job;
using settlement.path.finders;
using settlement.room.main;
using settlement.tilemap.terrain;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using view.sett;
using view.sett.ui.room;
using view.tool;

namespace settlement.room.main.construction
{
    final class ConstructionBlueprint : RoomBlueprint
    {
        final ConstructionHoverer hoverer = new ConstructionHoverer();
        ArrayListResize<ConstructionInstance> all = new ArrayListResize<ConstructionInstance>(256);

        public ConstructionBlueprint(ROOMS r) : base("_CONSTRUCTION")
        {
            PLACABLE q = new PlacableMulti("Finish room")
            {
                public void place(int tx, int ty, AREA a, PLACER_TYPE t)
                {
                    if (ROOMS().map.is(tx, ty))
                        construct(tx, ty);
                    else
                    {
                        Job j = SETT.JOBS().getter.get(tx, ty);
                        if (j != null)
                        {
                            TerrainTile tt = j.becomes(tx, ty);
                            SETT.JOBS().clearer.set(tx, ty);
                            tt.placeFixed(tx, ty);
                        }
                    }
                }

                public CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
                {
                    return (is(tx, ty) || SETT.JOBS().getter.get(tx, ty) != null) ? null : "";
                }

                public bool expandsTo(int fromX, int fromY, int toX, int toY)
                {
                    if (is(fromX, fromY))
                    {
                        if (is(toX, toY))
                            return true;
                        if (SETT.JOBS().getter.get(toX, toY) != null)
                            return true;
                    }
                    return false;
                }
            };
            IDebugPanelSett.add(q);
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public override SFinderFindable service(int tx, int ty)
        {
            return null;
        }

        public ConstructionInstance create(TmpArea area, ConstructionInit init)
        {
            ConstructionInstance ins = new ConstructionInstance(this, area, init);
            if (SETT.ROOMS().map.get(ins.mX(), ins.mY()) == ins)
                all.add(ins);
            return ins;
        }

        void remove(ConstructionInstance ins)
        {
            all.remove(ins);
        }

        public override ConstructionInstance get(int tx, int ty)
        {
            Room r = SETT.ROOMS().map.get(tx, ty);
            if (r != null && r is ConstructionInstance)
                return (ConstructionInstance)r;
            return null;
        }

        public override COLOR miniC(int tx, int ty)
        {
            return get(tx, ty).blueprint.miniColor(tx, ty);
        }

        public override COLOR miniCPimped(ColorImp origional, int tx, int ty, bool northern, bool southern)
        {
            return get(tx, ty).blueprint.miniColorPimped(origional, tx, ty, northern, southern);
        }

        protected override void save(FilePutter saveFile)
        {
            saveFile.object(all);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        protected override void load(FileGetter saveFile) throws IOException
        {
            all.clear();
            object a = saveFile.object(true);
            if (a != null)
            {
                ArrayListResize<ConstructionInstance> li = (ArrayListResize<ConstructionInstance>)a;
                foreach (ConstructionInstance i in li)
                {
                    if (!i.constructing)
                        all.add(i);
                }
            }
            else
                clear();
        }

        protected override void clear()
        {
            all.clear();
        }

        public override void appendView(LISTE<UIRoomModule> mm)
        {
            mm.add(hoverer);
        }

        private Rec rec = new Rec();
        public void construct(int tx, int ty)
        {
            ConstructionInstance r = get(tx, ty);
            if (r == null)
                return;
            if (r.mX() != tx || r.mY() != ty)
                return;
            rec.set(r.body());
            foreach (COORDINATE c in rec)
            {
                if (!r.is(c))
                    continue;
                r.jobClear(c.x(), c.y());
                if (r.blueprint.removeFertility())
                    GRASS().current.set(c.x(), c.y(), 0);
                if (!TERRAIN().CAVE.is(c) && r.structureI != -1 && !TERRAIN().BUILDINGS.all().get(r.structureI).roof.is(c))
                    TERRAIN().BUILDINGS.all().get(r.structureI).roof.placeFixed(c.x(), c.y());
                if (!TERRAIN().get(c).clearing().isStructure() && r.blueprint.removeTerrain(c.x(), c.y()))
                    TERRAIN().NADA.placeFixed(c.x(), c.y());
                r.blueprint.putFloor(c.x(), c.y(), r.upgrade(), r);
            }
            r.finish();
        }
    }
}