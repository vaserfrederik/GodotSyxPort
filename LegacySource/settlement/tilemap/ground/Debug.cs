using System;
using System.Collections.Generic;
using init.type;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data.GETTER;
using util.gui.misc;
using view.interrupter;
using view.main;
using view.sett;
using view.tool;

namespace settlement.tilemap.ground
{
    class Debug
    {
        Debug(Ground g)
        {
            LinkedList<CLICKABLE> bs = new LinkedList<CLICKABLE>();
            GETTER_IMP<GroundType> get = new GETTER_IMP<GroundType>(g.types.NORMAL);

            foreach (GroundType t in g.types.ALL)
            {
                GButt b = new GButt.ButtPanel(t.icon)
                {
                    protected override void clickA()
                    {
                        get.set(t);
                    }

                    protected override void renAction()
                    {
                        selectedSet(get.get() == t);
                    }
                }.hoverSet(t);
                bs.add(b);
            }

            PlacableMulti p = new PlacableMulti("GROUND: Types")
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    g.MAP.set(tx, ty, get.get());
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }

                public override LIST<CLICKABLE> getAdditionalButt()
                {
                    return bs;
                }
            };

            IDebugPanelSett.add(p);

            PlacableMulti undo = new PlacableMulti("GROUND: Moisture down")
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    g.MOISTURE_CURRENT.increment(tx, ty, -Ground.MOISTURE_MAXI);
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }
            };

            p = new PlacableMulti("GROUND: Moisture")
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    g.MOISTURE_CURRENT.increment(tx, ty, Ground.MOISTURE_MAXI);
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }

                public override PLACABLE getUndo()
                {
                    return undo;
                }
            };
            IDebugPanelSett.add(p);

            IDebugPanelSett.add("GROUND: set color", new ACTION()
            {
                int i = 0;

                public override void exe()
                {
                    i++;
                    i %= CLIMATES.ALL().size();
                    COLOR wet = CLIMATES.ALL().get(i).colorGroundWet;
                    COLOR dry = CLIMATES.ALL().get(i).colorGroundDry;
                    LOG.ln(i);
                    g.setColors(dry, wet, 0);
                }
            });

            IDebugPanelSett.add("GROUND: set color", new ACTION()
            {
                public override void exe()
                {
                    VIEW.s().panels.add(new DebugCol(), true);
                }
            });
        }

        private static class DebugCol : ISidePanel
        {
            private readonly ColorImp dry = new ColorImp(COLOR.WHITE50);
            private readonly ColorImp wet = new ColorImp(COLOR.WHITE50);

            DebugCol()
            {
                titleSet("ground color");
                dry.set(SETT.GROUND().dry);
                wet.set(SETT.GROUND().wet);
                section.addDown(2, new GColorPicker(false, "dry")
                {
                    public override ColorImp color()
                    {
                        return dry;
                    }

                    public override void change()
                    {
                        SETT.GROUND().setColors(dry, wet, 0);
                    }
                });

                section.addDown(2, new GColorPicker(false, "wet")
                {
                    public override ColorImp color()
                    {
                        return wet;
                    }

                    public override void change()
                    {
                        SETT.GROUND().setColors(dry, wet, 0);
                    }
                });
            }
        }
    }
}