using System.Collections.Generic;
using init.paths;
using init.structure;
using settlement.main;
using settlement.tilemap.terrain;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.keymap;

public sealed class BUILDING_PREFS
{
    private static BUILDING_PREFS self;

    private readonly BUILDING_PREF MOUNTAIN;
    private readonly BUILDING_PREF OUTDOORS;
    private readonly LIST<BUILDING_PREF> BUILDING;
    private readonly LIST<BUILDING_PREF> ALL;
    private readonly RMAP<BUILDING_PREF> map;

    private BUILDING_PREFS()
    {
        self = this;
        LinkedList<BUILDING_PREF> all = new LinkedList<BUILDING_PREF>();
        MOUNTAIN = new BUILDING_PREF("_MOUNTAIN", all)
        {
            public override SPRITE icon()
            {
                return SETT.TERRAIN().MOUNTAIN.getIcon();
            }
        };
        OUTDOORS = new BUILDING_PREF("_OUTDOORS", all)
        {
            public override SPRITE icon()
            {
                return CLIMATES.ALL()[CLIMATES.ALL().Count / 2].icon;
            }
        };
        LinkedList<string> keys = new LinkedList<string>();
        keys.Add("_MUD");
        keys.AddRange(PATHS.INIT_SETTLEMENT().getFolder("structure").getFiles());
        ArrayList<BUILDING_PREF> buildings = new ArrayList<BUILDING_PREF>(keys.Count);
        int in = 0;
        foreach (string k in keys)
        {
            final int ind = in++;
            buildings.Add(new BUILDING_PREF(k, all)
            {
                public override SPRITE icon()
                {
                    return SETT.TERRAIN().BUILDINGS.get(STRUCTURES.all()[ind]).iconCombo;
                }
            });
        }
        this.BUILDING = buildings;
        this.ALL = new ArrayList<BUILDING_PREF>(all);

        map = new RMAP<BUILDING_PREF>("STRUCTURE", ALL);
    }

    public static BUILDING_PREF get(int tx, int ty)
    {
        if (SETT.TERRAIN().MOUNTAIN.isMountain(tx, ty))
            return self.MOUNTAIN;
        if (SETT.TERRAIN().get(tx, ty) is TBuilding.BuildingComponent)
        {
            return self.BUILDING.get(((TBuilding.BuildingComponent)SETT.TERRAIN().get(tx, ty)).building().structure.index());
        }
        return self.OUTDOORS;
    }

    public static BUILDING_PREF get(Structure building)
    {
        return self.BUILDING.get(building.index());
    }

    public static RMAP<BUILDING_PREF> MAP()
    {
        return self.map;
    }

    public static LIST<BUILDING_PREF> ALL()
    {
        return self.ALL;
    }
}