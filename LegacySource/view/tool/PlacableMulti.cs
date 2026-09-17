using System;
using System.Text;
using init.sprite;
using init.type;
using settlement.main;
using settlement.tilemap.floor.Floor;
using settlement.tilemap.terrain.Terrain;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sprite;
using util.gui.misc;
using view.main;
using view.subview;
using view.world.generator;

namespace view.tool
{
    public abstract class PlacableMulti : PLACABLE
    {
        private readonly CharSequence name;
        public readonly CharSequence desc;
        private readonly SPRITE icon;
        private readonly PLACABLE undo;
        PLACER_TYPE previous;
        int prevSize = -1;

        public PlacableMulti(CharSequence name)
            : this(name, null, null, null)
        {
        }

        public PlacableMulti(CharSequence name, CharSequence desc, SPRITE icon)
            : this(name, desc, icon, null)
        {
        }

        public PlacableMulti(CharSequence name, CharSequence desc, SPRITE icon, PLACABLE undo)
        {
            this.name = name;
            this.desc = desc;
            if (icon == null)
                icon = SPRITES.icons().m.cancel;
            this.icon = icon;
            this.undo = undo;
        }

        public void updateRegardless(GameWindow window, AREA selected)
        {
        }

        public override SPRITE getIcon()
        {
            return icon;
        }

        public override CharSequence name()
        {
            return name;
        }

        public override PLACABLE getUndo()
        {
            return undo;
        }

        public override void hoverDesc(GBox box)
        {
            box.title(name);
            box.text(desc);
        }

        public CharSequence desc()
        {
            return desc;
        }

        public bool canBePlacedAs(PLACER_TYPE t)
        {
            return true;
        }

        public bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            return false;
        }

        public bool magicExpandTo(int fromX, int fromY, int toX, int toY)
        {
            if (VIEW.current() == VIEW.world() || VIEW.current() == VIEW.world().editor || VIEW.current() is WorldViewGenerator)
            {
                return TERRAINS.world.get(fromX, fromY) == TERRAINS.world.get(toX, toY);
            }
            else
            {
                TerrainTile t = SETT.TERRAIN().get(fromX, fromY);
                if (t.clearing().isEasilyCleared())
                {
                    Floor f = SETT.FLOOR().getter.get(fromX, fromY);
                    if (f != null)
                    {
                        return (SETT.FLOOR().getter.get(toX, toY) == f);
                    }
                    else
                    {
                        return (SETT.FLOOR().getter.get(toX, toY) == f) && SETT.TERRAIN().get(toX, toY).clearing().isEasilyCleared();
                    }
                }
                else
                {
                    if (SETT.TERRAIN().TREES.isTree(fromX, fromX) && SETT.TERRAIN().TREES.isTree(toX, toY))
                        return true;
                    return t == SETT.TERRAIN().get(toX, toY);
                }
            }
        }

        public void finishPlacing(AREA placedArea)
        {
        }

        public void finishChecking(AREA placedArea)
        {
        }

        public abstract CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type);
        public CharSequence isPlacable(AREA area, PLACER_TYPE type)
        {
            return null;
        }
        public abstract void place(int tx, int ty, AREA area, PLACER_TYPE type);
        public void renderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, AREA area, PLACER_TYPE type, bool isPlacable, bool areaIsPlacable)
        {
            if (isPlacable)
                SPRITES.cons().BIG.dashedThick.render(r, mask, x, y);
            else
                SPRITES.cons().BIG.dashed_hollow.render(r, mask, x, y);
        }

        public void placeInfo(GBox b, int oktiles, AREA a)
        {
            if (a.body().width() > 1 && a.body().height() > 1)
            {
                GText t = b.text();
                t.add(a.body().width()).add('x').add(a.body().height()).adjustWidth();
                t.s().add('(').add(oktiles).add(')');
                b.add(t);
            }
        }
    }
}