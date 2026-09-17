using System;
using System.Collections.Generic;
using game;
using init.resources;
using init.sprite.game;
using settlement.main;
using settlement.room.home;
using settlement.tilemap.floor.Floors;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.race.home
{
    public sealed class RaceHomeClass
    {
        private readonly LIST<RES_AMOUNT> amounts;
        private readonly int[] ramounts;
        private readonly int amountTotal;

        public readonly RaceHomeSheet bedTop;
        public readonly RaceHomeSheet bedBottom;
        public readonly RaceHomeSheet carpet;
        public readonly RaceHomeSheet table;
        public readonly RaceHomeSheet nightStand;
        public readonly RaceHomeSheet storage;
        public readonly RaceHomeSheet chair;
        public readonly RaceHomeSheet nick1;
        public readonly RaceHomeSheet nickTop1;
        public readonly RaceHomeSheet nick2;
        public readonly RaceHomeSheet mat;
        public readonly RaceHomeSheet masterBed;
        public readonly RaceHomeSheet statue;

        private readonly int[][] fneeded;
        private Floor[] floors;

        public RaceHomeClass() : this((Json)null)
        {
        }

        private RaceHomeClass(Json json) : this(json, new ArrayList<RES_AMOUNT>(0), 0, null)
        {
        }

        private RaceHomeClass(Json json, ArrayList<RES_AMOUNT> amounts, int amountTotal, Floor[] floors)
        {
            this.amounts = amounts;
            this.amountTotal = amountTotal;
            this.floors = floors;

            bedTop = new RaceHomeSheet();
            bedBottom = new RaceHomeSheet();
            carpet = new RaceHomeSheet();
            table = new RaceHomeSheet();
            nightStand = new RaceHomeSheet();
            storage = new RaceHomeSheet();
            chair = new RaceHomeSheet();
            nick1 = new RaceHomeSheet();
            nickTop1 = new RaceHomeSheet();
            nick2 = new RaceHomeSheet();
            mat = new RaceHomeSheet();
            masterBed = new RaceHomeSheet();
            statue = new RaceHomeSheet();
            fneeded = Alloc.i2(0, 0);

            if (json != null)
            {
                InitializeFromJson(json);
            }
        }

        private void InitializeFromJson(Json json)
        {
            ArrayList<RESOURCE> resses = new ArrayList<RESOURCE>(RESOURCES.ALL().size());
            ramounts = Alloc.ii(RESOURCES.ALL().size());

            foreach (string key in json.keys())
            {
                Json[] js = json.jsons(key);
                foreach (Json jj in js)
                {
                    Json j = jj.json(RESOURCES.KEYS);
                    foreach (string k in j.keys())
                    {
                        RESOURCE res = RESOURCES.map().tryGet(k);
                        if (res == null)
                            GAME.WarnLight(j.errorGet("No resource with this key! ", k));
                        else
                        {
                            int am = j.i(k, 1, 15);

                            if (ramounts[res.index()] == 0)
                            {
                                resses.add(res);
                            }
                            if (am > ramounts[res.index()])
                                ramounts[res.index()] = am;
                        }
                    }
                }
            }

            if (resses.size() > 8)
            {
                json.error("Only 8 distinct resources are allowed", "");
            }

            ArrayList<RES_AMOUNT> ams = new ArrayList<RES_AMOUNT>(resses.size());
            int tot = 0;
            for (int ri = 0; ri < resses.size(); ri++)
            {
                ams.add(new RES_AMOUNT.Abs(resses.get(ri), ramounts[resses.get(ri).index()]));
                tot += ramounts[resses.get(ri).index()];
            }

            amounts = ams;
            amountTotal = tot;

            bedTop = new RaceHomeSheet(amounts, json, "BED_1x1_TOP", SheetType.s1x1);
            bedBottom = new RaceHomeSheet(amounts, json, "BED_1x1_BOTTOM", SheetType.s1x1);
            carpet = new RaceHomeSheet(amounts, json, "CARPET_COMBO", SheetType.sCombo);
            table = new RaceHomeSheet(amounts, json, "TABLE_COMBO", SheetType.sCombo);
            nightStand = new RaceHomeSheet(amounts, json, "NIGHTSTAND_1x1", SheetType.s1x1);
            storage = new RaceHomeSheet(amounts, json, "STORAGE_1x1", SheetType.s1x1);
            chair = new RaceHomeSheet(amounts, json, "CHAIR_1x1", SheetType.s1x1);
            nick1 = new RaceHomeSheet(amounts, json, "NICKNACK_A_1x1", SheetType.s1x1);
            nickTop1 = new RaceHomeSheet(amounts, json, "NICKNACK_A_ONTOP_1x1", SheetType.s1x1);
            nick2 = new RaceHomeSheet(amounts, json, "NICKNACK_B_1x1", SheetType.s1x1);
            mat = new RaceHomeSheet(amounts, json, "MAT_1x1", SheetType.s1x1);
            masterBed = new RaceHomeSheet(amounts, json, "BED_MASTER_2x2", SheetType.s2x2);
            statue = new RaceHomeSheet(amounts, json, "STATUE_2x2", SheetType.s2x2);

            if (json.has("FLOORS"))
            {
                Json[] jsons = json.jsons("FLOORS");
                floors = new Floor[jsons.Length];
                fneeded = Alloc.i2(jsons.Length, ams.size());

                for (int i = 0; i < jsons.Length; i++)
                {
                    Json j = jsons[i];
                    RaceHomeSheet.addResource(ams, j, i, fneeded);
                    floors[i] = SETT.FLOOR().map.read(j);
                }
            }
            else
            {
                floors = new Floor[0];
                fneeded = Alloc.i2(0, 0);
            }
        }

        public LIST<RES_AMOUNT> resources()
        {
            return amounts;
        }

        public int amount(RESOURCE res)
        {
            return ramounts[res.index()];
        }

        public int amountTotal()
        {
            return amountTotal;
        }

        public Floor floor(HOME data)
        {
            outer:
            for (int ai = floors.Length - 1; ai >= 0; ai--)
            {
                int[] amounts = fneeded[ai];
                for (int i = 0; i < amounts.Length; i++)
                {
                    if (data.resourceAm(i) < amounts[i])
                        continue outer;
                }
                return floors[ai];
            }
            return null;
        }
    }
}