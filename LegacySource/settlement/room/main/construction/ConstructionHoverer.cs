using System;
using System.Collections.Generic;
using System.Linq;
using settlement.room.main.construction;
using init.resources;
using init.settings;
using init.sprite.UI;
using settlement.job;
using settlement.main;
using settlement.room.main;
using settlement.room.main.furnisher;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using util.gui.misc;
using util.info;
using util.text;
using view.main;
using view.sett.ui.room;

namespace settlement.room.main.construction
{
    internal sealed class ConstructionHoverer : UIRoomModule
    {
        private static readonly string ¤¤prog = "Construction";
        private static readonly string ¤¤clear = "Cleared";
        private static readonly string ¤¤Mat = "Materials";
        private static readonly string ¤¤Act = "Dormant. Activate to commence work";
        private static readonly string ¤¤Broken = "The room is broken. You can order your minions to repair it with the repair or activate job tool.";
        private static readonly string ¤¤resources = "¤This construction needs {0} to complete, which is unobtainable in your city.";

        static ConstructionHoverer()
        {
            D.ts(typeof(ConstructionHoverer));
        }

        private readonly int[] resNeeded = Alloc.ii(RESOURCES.ALL().Count);
        private readonly int[] resAllocated = Alloc.ii(RESOURCES.ALL().Count);

        public ConstructionHoverer()
        {
            // TODO Auto-generated constructor stub
        }

        public override void hover(GBox box, Room r, int rx, int ry)
        {
            ConstructionInstance k = (ConstructionInstance)r;
            box.clear();
            box.add(k.constructor().blue().icon);
            box.textLL(k.name(rx, ry));
            box.NL(8);

            if (!k.active)
            {
                if (k.broken)
                {
                    box.add(box.text().errorify().add(¤¤Broken));
                }
                else
                    box.add(box.text().errorify().add(¤¤Act));
            }

            for (int i = 0; i < k.blueprint.resources(); i++)
            {
                resNeeded[i] = 0;
                resAllocated[i] = 0;
            }

            int clearNeeded = 0;
            int floorNeeded = 0;
            int structuresNeeded = 0;
            int structureResources = 0;
            int itemNeeded = 0;
            int itemTotal = 0;

            foreach (COORDINATE c in k.body())
            {
                if (!k.is(c))
                    continue;
                if (k.needsClear(c))
                    clearNeeded++;
                if (k.structureI >= 0 && !SETT.TERRAIN().CAVE.is(c) && !SETT.TERRAIN().MOUNTAIN.isMountain(c.x(), c.y()) && !SETT.TERRAIN().BUILDINGS.all().get(k.structureI).roof.is(c))
                {
                    structuresNeeded++;
                    structureResources += dWorkAmount.get(c);
                }
                if (dFloored.is(c, 0))
                    floorNeeded++;
                FurnisherItem it = SETT.ROOMS().fData.item.get(c);
                if (it != null)
                    itemTotal++;
                if (it != null && (dConstructed.is(c, 0) || dBroken.is(c, 1)))
                {
                    itemNeeded++;
                }
                if (SETT.ROOMS().fData.isMaster.is(c))
                {
                }

                int am = dResAllocated.get(c);
                for (int i = 0; i < k.blueprint.resources(); i++)
                {
                    int b = dResourceNeeded[i].get(c);
                    resNeeded[i] += b;
                    if (am > 0)
                    {
                        int a = CLAMP.i(am, 0, b);
                        resAllocated[i] += a;
                        am -= a;
                    }
                }
            }

            box.NL();
            box.textLL(¤¤clear);
            box.tab(5);
            box.add(GFORMAT.perc(box.text(), (k.area() - clearNeeded) / (double)k.area(), 1));

            box.NL();
            box.textLL(¤¤Mat);
            box.tab(5);

            if (k.structureI >= 0)
            {
                RESOURCE sRes = SETT.TERRAIN().BUILDINGS.all().get(k.structureI).structure.resource;
                int sResA = SETT.TERRAIN().BUILDINGS.all().get(k.structureI).structure.resAmount;
                int kkkk = -1;
                for (int i = 0; i < k.blueprint.resources(); i++)
                {
                    if (k.blueprint.resource(i) == sRes)
                    {
                        kkkk = i;
                        resAllocated[i] += structureResources;
                        resAllocated[i] += (k.area() - structuresNeeded) * sResA;
                        resNeeded[i] += k.area() * sResA;
                    }
                }
                if (kkkk == -1 && sRes != null && structuresNeeded > 0)
                {
                    box.setResource(sRes, structureResources + (k.area() - structuresNeeded) * sResA, k.area() * sResA);
                }
            }

            for (int i = 0; i < k.blueprint.resources(); i++)
            {
                if (resNeeded[i] > 0)
                {
                    RESOURCE res = k.blueprint.resource(i);
                    box.setResource(res, resAllocated[i], resNeeded[i]);
                }
            }
            box.NL();

            for (int i = 0; i < k.blueprint.resources(); i++)
            {
                if (resNeeded[i] > 0 && resAllocated[i] < resNeeded[i] && !SETT.PATH().finders.resource.has(rx, ry, k.blueprint.resource(i).bit))
                {
                    RESOURCE res = k.blueprint.resource(i);
                    GText t = box.text();
                    t.add(¤¤resources);
                    t.insert(0, res.names);
                    t.errorify();
                    if (res.specialHelpText != null)
                    {
                        t.s().add(res.specialHelpText);
                    }
                    box.add(res.icon().big);
                    box.add(t);
                    box.NL();
                }
            }
            box.NL();

            box.NL();
            box.textLL(¤¤prog);
            box.tab(5);
            double total = k.area() + itemTotal;
            double p = floorNeeded + k.builtNeeded;
            double t = (total - p) / total;
            box.add(GFORMAT.perc(box.text(), t));

            box.NL();

            if (S.get().developer)
            {
                box.NL();
                k.debug(box);
                box.NL(); box.NL();
                box.add(box.text().add("nClear: ").add(clearNeeded));
                box.add(box.text().add("nFloor: ").add(floorNeeded));
                box.add(box.text().add("nStruc: ").add(structuresNeeded));
                box.add(box.text().add("nItem: ").add(itemNeeded).add('/').add(itemTotal));

                Job j = SETT.JOBS().getter.get(VIEW.s().getWindow().tile());
                if (j != null)
                {
                    box.NL(8);
                    j.hover(box);
                }
            }
        }

        private readonly GText t = new GText(UI.FONT().M, 16);

        public void renderButt(ConstructionInstance k, SPRITE_RENDERER r, int x1, int cy)
        {
            k.icon().renderCY(r, x1, cy);

            int clearNeeded = 0;
            int floorNeeded = 0;
            int structNeeded = 0;
            int itemTotal = 0;

            foreach (COORDINATE c in k.body())
            {
                if (!k.is(c))
                    continue;
                if (k.needsClear(c))
                    clearNeeded++;
                if (k.structureI >= 0 && !SETT.TERRAIN().CAVE.is(c) && !SETT.TERRAIN().MOUNTAIN.isMountain(c.x(), c.y()) && !SETT.TERRAIN().BUILDINGS.all().get(k.structureI).roof.is(c))
                {
                    structNeeded++;
                }
                if (dFloored.is(c, 0))
                {
                    floorNeeded++;
                }
                FurnisherItem it = SETT.ROOMS().fData.item.get(c);
                if (it != null)
                    itemTotal++;
            }

            double total = k.area() + k.area() + itemTotal;
            double p = floorNeeded + k.builtNeeded + clearNeeded;
            if (k.structureI >= 0)
            {
                total += k.area();
                p += structNeeded;
            }
            double prog = (total - p) / total;

            t.clear();
            GFORMAT.percGood(t, prog);

            if (!k.active)
                t.errorify();

            t.renderCY(r, x1 + Icon.L + 8, cy);
        }
    }
}