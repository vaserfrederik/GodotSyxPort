using System;
using System.Collections.Generic;
using System.Linq;
using init.constant;
using init.sprite;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using util;
using util.colors;
using util.gui.misc;
using util.text;
using view.keyboard;
using view.main;
using view.subview;
using view.tool;

namespace view.tool
{
    internal sealed class PlacableMultiTool : PlaceFunc
    {
        private PlacableMulti placable;
        private int size = 0;
        private readonly Coo hTile = new Coo();
        private readonly Coo clickedTile = new Coo();
        private bool newTile;
        private PLACER_TYPE type = PLACER_TYPE.SQUARE;
        private readonly PlacerArea a = PlacerArea.self;
        //private bool pressed;

        {
            D.gInit(this);
        }

        private readonly ArrayList<CLICKABLE> butts = new ArrayList<CLICKABLE>(2 + PLACER_TYPE.all.size());
        private readonly CLICKABLE bIncrease = KeyButt.Wrap(new GButt.Panel(SPRITES.icons().m.plus)
        {
            {
                hoverInfoSet("" + KEYS.MAIN().MOD.repr() + Dic.¤¤MouseWheelAdd);
            }
            protected override void renAction()
            {
                activeSet(type.usesSize && size < 15);
            }

            protected override void clickA()
            {
                radius(1);
            }
        }, KEYS.MAIN().GROW);
        private readonly CLICKABLE bDecrease = KeyButt.Wrap(new GButt.Panel(SPRITES.icons().m.minus)
        {
            {
                hoverInfoSet("" + KEYS.MAIN().MOD.repr() + Dic.¤¤MouseWheelAdd);
            }
            protected override void renAction()
            {
                activeSet(type.usesSize && size > 0);
            }

            protected override void clickA()
            {
                radius(-1);
            }
        }, KEYS.MAIN().SHRINK);
        private readonly GButt.Panel[] buttsTypes = new GButt.Panel[PLACER_TYPE.all.size()];
        {
            for (int i = 0; i < buttsTypes.Length; i++)
            {
                PLACER_TYPE t = PLACER_TYPE.all.Get(i);
                buttsTypes[i] = new GButt.Panel(t.icon())
                {
                    {
                        hoverInfoSet(t.name);
                    }
                    protected override void renAction()
                    {
                        selectedSet(type == t);
                    }

                    protected override void clickA()
                    {
                        type = t;
                        placable.previous = t;
                        clear();
                        VIEW.inters().popup.close();
                    }
                };
            }
        }
        private readonly GuiSection typeButts = new GuiSection();
        private readonly GButt.Panel buttType = new GButt.Panel(SPRITES.icons().m.cancel, D.g("type"))
        {
            protected override void renAction()
            {
                replaceLabel(type.icon(), DIR.C);
            }

            protected override void clickA()
            {
                VIEW.inters().popup.show(typeButts, this);
            }
        };

        private void radius(int d)
        {
            size = CLAMP.i(size + d, 0, 15);
            placable.prevSize = size;
        }

        public override void updateHovered(float ds, GameWindow window, bool pressed)
        {
            newTile |= hTile.set(window.tile());
            if (MButt.RIGHT.isDown())
                clear();

            if (type.usesSize)
            {
                double s = MButt.clearWheelSpin();
                if (KEYS.MAIN().MOD.isPressed() && s != 0)
                {
                    if (s > 0)
                    {
                        radius(1);
                    }
                    else if (s < 0)
                    {
                        radius(-1);
                    }
                    MButt.clearWheelSpin();
                }

                if (KEYS.MAIN().GROW.consumeClick())
                {
                    radius(1);
                }
                else if (KEYS.MAIN().SHRINK.consumeClick())
                {
                    radius(-1);
                }
            }

            PlacerArea.self.clear();
            if (type == PLACER_TYPE.FILL)
            {
                specialFill(hTile.x(), hTile.y(), size, a.set, placable);
            }
            else if (type.drag && pressed)
            {
                type.paint(hTile.x(), hTile.y(), clickedTile.x(), clickedTile.y(), size, a.set);
            }
            else
            {
                type.paint(hTile.x(), hTile.y(), hTile.x(), hTile.y(), size, a.set);
            }

            GUTIL.filler().init(this);

            foreach (COORDINATE c in a.body())
            {
                if (a.is(c))
                    GUTIL.filler().fill(c);
            }

            while (GUTIL.filler().hasMore())
            {
                COORDINATE c = GUTIL.filler().poll();
                for (int i = 0; i < DIR.ALL.size(); i++)
                {
                    DIR d = DIR.ALL.Get(i);
                    int dx = c.x() + d.x();
                    int dy = c.y() + d.y();
                    if (!SETT.IN_BOUNDS(dx, dy))
                        continue;
                    if (placable.expandsTo(c.x(), c.y(), dx, dy))
                    {
                        GUTIL.filler().fill(dx, dy);
                        a.set.set(dx, dy);
                    }
                }
            }

            GUTIL.filler().done();

            if (!type.drag)
            {
                if (pressed && newTile)
                {
                    place();
                }
            }

            newTile = false;
        }

        public override void update(float ds, GameWindow window, bool pressed)
        {
            placable.updateRegardless(window, PlacerArea.self);
            base.update(ds, window, pressed);
        }

        private void specialFill(int x1, int y1, int size, MAP_SET set, PlacableMulti placer)
        {
            // Implementation of specialFill
        }

        public override void renderPlaceHolder(RENDER render, int mask, int x, int y, int tileX, int tileY, MAP_SET set, PLACER_TYPE type, bool isPlacable, bool areaIsPlacable)
        {
            // Implementation of renderPlaceHolder
        }

        public override void click(GameWindow window)
        {
            clickedTile.set(window.tile());
            if (!type.drag)
            {
                place();
            }
        }

        public override void activate(PLACABLE placer, GameWindow window)
        {
            hTile.set(window.tile());
            placable = (PlacableMulti)placer;
            if (placable.previous != null)
            {
                type = placable.previous;
            }
            else
            {
                type = PLACER_TYPE.SQUARE;
            }
            if (!placable.canBePlacedAs(type))
            {
                foreach (PLACER_TYPE t in PLACER_TYPE.all)
                {
                    if (placable.canBePlacedAs(t))
                    {
                        type = t;
                        break;
                    }
                }
            }
            if (placable.prevSize != -1)
            {
                size = placable.prevSize;
            }

            clear();
        }

        public override void clickRelease(GameWindow window)
        {
            if (type.drag)
            {
                place();
            }
        }

        private void place()
        {
            placable.finishChecking(a);

            if (placable.isPlacable(a, type) != null)
                return;

            for (int y = a.body().y1(); y < a.body().y2(); y++)
            {
                for (int x = a.body().x1(); x < a.body().x2(); x++)
                {
                    if (!a.is(x, y) || placable.isPlacable(x, y, a, type) != null)
                        continue;
                    placable.place(x, y, a, type);
                }
            }
            placable.finishPlacing(a);
        }

        public override LIST<CLICKABLE> gui()
        {
            butts.clear();
            int i = 0;
            bool any = false;
            int x1 = typeButts.body().x1();
            int y1 = typeButts.body().y1();
            typeButts.clear();
            typeButts.body().moveX1Y1(x1, y1);
            foreach (GButt.Panel p in buttsTypes)
            {
                if (placable.canBePlacedAs(PLACER_TYPE.all.Get(i)))
                {
                    any |= !PLACER_TYPE.all.Get(i).usesSize;
                    typeButts.addDown(0, p);
                }
                i++;
            }
            butts.add(buttType);
            if (any)
            {
                butts.add(bIncrease);
                butts.add(bDecrease);
            }

            return butts;
        }
    }
}