using System;
using System.Collections.Generic;
using game.boosting;
using game.room.industry.module;
using game.room.infra.admin;
using game.room.knowledge.laboratory;
using game.room.knowledge.library;
using game.room.main;
using init.sprite.UI;
using settlement.main;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;

class NobleOfficeUtil
{
    private static readonly CharSequence ¤¤Governing = "Governor";
    private static readonly CharSequence ¤¤GoverningD = "Gives you {0} gov points to use to develop regions with. Each additional rank yields {1} more points.";

    private static readonly CharSequence ¤¤rBoost = "Boosts {0} workers in your {1} with + {2}. Each additional rank boosts {3} more workers.";
    private static readonly CharSequence ¤¤name = "Master of {0}";

    static NobleOfficeUtil()
    {
        D.ts(typeof(NobleOfficeUtil));
    }

    public static LIST<NobleOffice> Make()
    {
        int workers = 50;
        int gov = 20;

        ArrayListGrower<NobleOffice> all = new ArrayListGrower<NobleOffice>();
        foreach (INDUSTRY_HASER m in SETT.ROOMS().MINES)
            Make(all, m, workers);
        Make(all, SETT.ROOMS().WOOD_CUTTER, workers);
        foreach (INDUSTRY_HASER m in SETT.ROOMS().FARMS)
            Make(all, m, workers);
        foreach (INDUSTRY_HASER m in SETT.ROOMS().ORCHARDS)
            Make(all, m, workers);
        foreach (INDUSTRY_HASER m in SETT.ROOMS().PASTURES)
            Make(all, m, workers);
        foreach (INDUSTRY_HASER m in SETT.ROOMS().FISHERIES)
            Make(all, m, workers);
        foreach (INDUSTRY_HASER m in SETT.ROOMS().REFINERS)
            Make(all, m, workers);
        foreach (INDUSTRY_HASER m in SETT.ROOMS().WORKSHOPS)
            Make(all, m, workers);

        CharSequence desc = "" + Str.TMP.Clear().Add(¤¤GoverningD).Insert(0, gov).Insert(1, NOBLES.RANK_INCREASE * gov);

        new NobleOffice(all, gov * 1000, BOOSTABLES.CIVICS().GOV, ¤¤Governing, desc, UI.icons().l.admin)
        {
            public override double Value(int slots)
            {
                return slots / 1000.0;
            }

            public override bool LeavesMap()
            {
                return true;
            }

            public override void HoverValue(GBox b, int slots)
            {
                b.Add(target.icon);
                b.TextLL(target.name);
                b.Tab(6);
                b.Add(GFORMAT.iIncr(b.Text(), slots * gov));
                b.NL();
            }

            public override int PopBoosted(int slots)
            {
                return -1;
            }
        };
        all.Get(all.Size() - 1).special = true;

        Make(all, SETT.ROOMS().EMBASSY, SETT.ROOMS().EMBASSY.bonus(), workers);
        foreach (ROOM_LIBRARY l in SETT.ROOMS().LIBRARIES)
            Make(all, l, l.bonus(), workers);
        foreach (ROOM_LABORATORY l in SETT.ROOMS().LABORATORIES)
            Make(all, l, l.bonus(), workers);
        foreach (ROOM_ADMIN l in SETT.ROOMS().ADMINS)
            Make(all, l, l.bonus(), workers);

        return all;
    }

    public static void Make(ArrayListGrower<NobleOffice> all, INDUSTRY_HASER i, int workers)
    {
        Make(all, i.industries().Get(0).blue, i.industries().Get(0).bonus(), workers);
    }

    public static void Make(ArrayListGrower<NobleOffice> all, RoomBlueprintImp blue, Boostable bo, int workers)
    {
        double inc = 2.5;
        CharSequence desc = "" + Str.TMP.Clear().Add(¤¤rBoost).Insert(0, workers).Insert(1, blue.info.names).Insert(2, inc, 1).Insert(3, NOBLES.RANK_INCREASE * workers);
        CharSequence name = "" + Str.TMP.Clear().Add(¤¤name).Insert(0, blue.info.names);
        new NobleOffice(all, inc, bo, name, desc, blue.icon)
        {
            public override double Value(int slots)
            {
                if (blue.employment().employed() <= 0)
                    return slots > 0 ? 1 : 0;
                return CLAMP.d((double)slots * workers / blue.employment().employed(), 0, 1);
            }

            public override RoomBlueprintIns<?> Room()
            {
                if (blue is RoomBlueprintIns<?>)
                    return (RoomBlueprintIns<?>)blue;
                return null;
            }

            public override void HoverValue(GBox b, int slots)
            {
                b.Add(target.icon);
                b.TextLL(target.name);
                b.Tab(6);
                b.Add(GFORMAT.f0(b.Text(), 2));
                b.Add(UI.icons().s.human);
                b.Add(GFORMAT.iofkInv(b.Text(), slots * workers, blue.employment().employed()));
                b.Add(GFORMAT.f0(b.Text(), value(slots) * add));
                b.NL();
            }

            public override int PopBoosted(int slots)
            {
                return slots * workers;
            }
        };
    }
}