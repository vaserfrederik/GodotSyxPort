using System;
using System.Collections.Generic;
using settlement.room.main.category;
using init.sprite;
using init.sprite.UI;
using settlement.room.main;
using snake2d.util.color;
using snake2d.util.sets;
using util.text;

public static class RoomCategories
{
    static CharSequence ¤¤other = "¤Other";

    static RoomCategories()
    {
        D.t(typeof(RoomCategories));
    }

    public RoomCategories(ROOMS r)
    {
        // TODO Auto-generated constructor stub
    }

    private readonly ArrayListGrower<RoomCategorySub> all = new ArrayListGrower<RoomCategorySub>();

    public readonly RoomCategorySub MINES = new RoomCategorySub(all, D.g("Mines"), SPRITES.icons().l.mine, new ColorImp(241 / 2, 94 / 2, 0));
    public readonly RoomCategorySub REFINERS = new RoomCategorySub(all, D.g("Refining"), SPRITES.icons().l.refiner, new ColorImp(226 / 2, 195 / 2, 38 / 2));
    public readonly RoomCategorySub CRAFTING = new RoomCategorySub(all, D.g("Crafting"), SPRITES.icons().l.workshop, new ColorImp(255 / 2, 155 / 2, 38 / 2));
    public readonly RoomCategorySub LAW = new RoomCategorySub(all, D.g("Law"), SPRITES.icons().l.law, new ColorImp(180 / 2, 180 / 2, 180 / 2));
    public readonly RoomCategorySub FARMS = new RoomCategorySub(all, D.g("Farms"), SPRITES.icons().l.farm, new ColorImp(74 / 2, 119 / 2, 14 / 2));
    public readonly RoomCategorySub FISH = new RoomCategorySub(all, D.g("Aquaculture"), SPRITES.icons().l.fish, new ColorImp(74 / 2, 119 / 2, 14 / 2));
    public readonly RoomCategorySub HUSBANDRY = new RoomCategorySub(all, D.g("Husbandry"), SPRITES.icons().l.pasture, new ColorImp(74 / 2, 119 / 2, 14 / 2));
    public readonly RoomCategorySub MILITARY = new RoomCategorySub(all, D.g("Military"), SPRITES.icons().l.trainig, new ColorImp(127, 0, 0));
    public readonly RoomCategorySub ADMIN = new RoomCategorySub(all, D.g("Administration"), SPRITES.icons().l.admin, new ColorImp(0, 127, 127));
    public readonly RoomCategorySub BREEDING = new RoomCategorySub(all, D.g("Procreation"), SPRITES.icons().l.breeding, new ColorImp(70 / 2, 0, 127));
    public readonly RoomCategorySub DECOR = new RoomCategorySub(all, D.g("Decorations"), SPRITES.icons().l.decor, new ColorImp(70 / 2, 0, 127));
    public readonly RoomCategorySub LOGISTICS = new RoomCategorySub(all, D.g("Logistics"), SPRITES.icons().l.logistics, new ColorImp(70 / 2, 0, 127));
    public readonly RoomCategorySub WATER = new RoomCategorySub(all, D.g("Water"), SPRITES.icons().l.water, new ColorImp(70 / 2, 0, 127));

    public readonly RoomCategorySub SER_REL = new RoomCategorySub(all, D.g("Religion"), SPRITES.icons().l.religion, new ColorImp(70 / 2, 0, 127));
    public readonly RoomCategorySub SER_CONSUMPTION = new RoomCategorySub(all, D.g("Distribution"), SPRITES.icons().l.dist, new ColorImp(70 / 2, 0, 127));
    public readonly RoomCategorySub SER_HEALTH = new RoomCategorySub(all, D.g("Health"), SPRITES.icons().l.health, new ColorImp(70 / 2, 0, 127));
    public readonly RoomCategorySub SER_ENTERTAIN = new RoomCategorySub(all, D.g("Entertainment"), SPRITES.icons().l.entertain, new ColorImp(70 / 2, 0, 127));
    public readonly RoomCategorySub SER_DEATH = new RoomCategorySub(all, D.g("Afterlife"), SPRITES.icons().l.death, new ColorImp(70 / 2, 0, 127));
    public readonly RoomCategorySub SER_HOME = new RoomCategorySub(all, D.g("Home"), SPRITES.icons().l.home, new ColorImp(70 / 2, 0, 127));
    public readonly LIST<RoomCategorySub> ALL = all;

    public readonly RoomCategoryMain MAIN_AGRIULTURE = new RoomCategoryMain(D.g("Agriculture"), SPRITES.icons().l.agri, new ArrayList<RoomCategorySub>
    {
        FARMS,
        HUSBANDRY,
        FISH
    });

    public readonly RoomCategoryMain MAIN_INDUSTRY = new RoomCategoryMain(D.g("Work"), SPRITES.icons().l.work, new ArrayList<RoomCategorySub>
    {
        MINES,
        REFINERS,
        CRAFTING
    });

    public readonly RoomCategoryMain MAIN_SERVICE = new RoomCategoryMain(D.g("Service"), SPRITES.icons().l.service, new ArrayList<RoomCategorySub>
    {
        SER_REL,
        SER_CONSUMPTION,
        SER_HEALTH,
        SER_ENTERTAIN,
        SER_DEATH,
        SER_HOME
    });

    public readonly RoomCategoryMain MAIN_INFRA = new RoomCategoryMain(D.g("Government"), SPRITES.icons().l.gov, new ArrayList<RoomCategorySub>
    {
        ADMIN,
        LAW,
        MILITARY,
        BREEDING,
        LOGISTICS,
        WATER,
        DECOR
    });

    // public readonly RoomCategoryMain MAIN_MISC = new RoomCategoryMain(D.g("Jobs"), SPRITES.icons().l.jobs, new ArrayList<RoomCategorySub>
    // {
    //     DECOR
    // });

    public readonly ArrayList<RoomCategoryMain> MAINS = new ArrayList<RoomCategoryMain>
    {
        MAIN_AGRIULTURE,
        MAIN_INDUSTRY,
        MAIN_SERVICE,
        MAIN_INFRA
    };

    public class RoomCategoryMain
    {
        public readonly CharSequence name;
        public readonly Icon icon;
        public readonly RoomCategorySub misc;
        public readonly LIST<RoomCategorySub> subs;
        private LIST<RoomBlueprintImp> all;

        public RoomCategoryMain(CharSequence name, Icon icon, LIST<RoomCategorySub> subs)
        {
            this.name = name;
            this.icon = icon;
            this.subs = subs;
            misc = new RoomCategorySub(RoomCategories.all, ¤¤other, SPRITES.icons().m.questionmark, COLOR.WHITE100);
            misc.main = this;
            foreach (RoomCategorySub s in subs)
                s.main = this;
        }

        private void n()
        {
            LinkedList<RoomBlueprintImp> all = new LinkedList<RoomBlueprintImp>();
            foreach (RoomCategorySub s in subs)
            {
                foreach (RoomBlueprintImp p in s.rooms())
                    all.add(p);
            }
            foreach (RoomBlueprintImp p in misc.rooms())
                all.add(p);
            this.all = new ArrayList<RoomBlueprintImp>(all);
        }

        public LIST<RoomBlueprintImp> all()
        {
            if (all == null)
            {
                n();
            }
            return all;
        }
    }
}