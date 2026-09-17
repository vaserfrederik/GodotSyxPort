using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.save;
using init.race;
using init.sprite;
using init.sprite.UI;
using init.type;
using settlement.entity;
using settlement.entity.animal;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main;
using settlement.stats;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.interrupter;
using view.subview;

public class UIMinimapSettConfigExt : UIMinimapSettConfig
{
    private static readonly COLOR good = new ColorImp(0, 0, 127);
    private static readonly COLOR bad = new ColorImp(127, 0, 0);
    private static readonly COLOR soso = new ColorImp(127, 127, 0);

    private readonly BOOLEANImp showAnimals = new BOOLEANImp(true);
    private readonly BOOLEANImp showHumans = new BOOLEANImp(true);
    private readonly BOOLEANImp showBuildings = new BOOLEANImp(true);
    private readonly BOOLEANImp showEmployment = new BOOLEANImp(true);
    private readonly BOOLEANImp showRace = new BOOLEANImp(true);
    private readonly BOOLEANImp showStat = new BOOLEANImp(true);

    private readonly BITSET bitsAnimals = new BITSET(ANIMALS.ALL().Count);
    private readonly BITSET bitsHumans = new BITSET(HTYPES.ALL().Count);
    private readonly BITSET bitsBuildings = new BITSET(SETT.ROOMS().Count);
    private readonly BITSET bitsEmployment = new BITSET(SETT.ROOMS().Count);
    private readonly BITSET bitsRace = new BITSET(RACES.ALL().Count);

    private STAT statC = null;

    public UIMinimapSettConfigExt(string key) : base(key)
    {
        // Constructor logic if needed
    }

    public override COLOR col(ENTITY entity)
    {
        if (entity is ANIMAL animal)
        {
            if (!showAnimals.get() || !bitsAnimals.get(animal.type.index))
                return null;
            return animal.type.col;
        }
        else if (entity is HUMAN human)
        {
            if (!showHumans.get() || !bitsHumans.get(human.type.index) || !bitsRace.get(human.race.index))
                return null;
            if (statC != null)
                return statC.get(human).col();
            return human.type.col;
        }
        return null;
    }

    public override void addButtons(GuiSection s)
    {
        base.addButtons(s);

        s.addRightC(0, new GButt.Button(() => showAnimals.set(!showAnimals.get())), "Show Animals");
        s.addRightC(0, new GButt.Button(() => showHumans.set(!showHumans.get())), "Show Humans");
        s.addRightC(0, new GButt.Button(() => showBuildings.set(!showBuildings.get())), "Show Buildings");
        s.addRightC(0, new GButt.Button(() => showEmployment.set(!showEmployment.get())), "Show Employment");
        s.addRightC(0, new GButt.Button(() => showRace.set(!showRace.get())), "Show Race");
        s.addRightC(0, new GButt.Button(() => showStat.set(!showStat.get())), "Show Stat");
    }

    public override void save(FileOut file)
    {
        base.save(file);
        file.PutBoolean(showAnimals.get());
        file.PutBoolean(showHumans.get());
        file.PutBoolean(showBuildings.get());
        file.PutBoolean(showEmployment.get());
        file.PutBoolean(showRace.get());
        file.PutBoolean(showStat.get());
        bitsAnimals.save(file);
        bitsHumans.save(file);
        bitsBuildings.save(file);
        bitsEmployment.save(file);
        bitsRace.save(file);
        file.PutInt(statC == null ? -1 : STATS.indexOf(statC));
    }

    public override void load(FileIn file)
    {
        base.load(file);
        showAnimals.set(file.GetBoolean());
        showHumans.set(file.GetBoolean());
        showBuildings.set(file.GetBoolean());
        showEmployment.set(file.GetBoolean());
        showRace.set(file.GetBoolean());
        showStat.set(file.GetBoolean());
        bitsAnimals.load(file);
        bitsHumans.load(file);
        bitsBuildings.load(file);
        bitsEmployment.load(file);
        bitsRace.load(file);
        int statIndex = file.GetInt();
        statC = statIndex == -1 ? null : STATS.get(statIndex);
    }
}