using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using settlement.stats.equip;
using game.battle;
using init.constant;
using init.paths;
using init.race;
using init.resources;
using init.sprite.UI;
using settlement.entity.animal;
using settlement.main;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.rendering;
using util.spritecomposer;
using util.text;

public class EquipBattle : Equip
{
    private readonly Bitsmap1D tars;
    private readonly int iMil;
    public readonly int amountInGarrison;
    public readonly double[] slotUse;

    public readonly DivSprite[] sprites;
    public static readonly CharSequence ¤¤combineProblem = "Can not be combined with current equipment.";
    public static readonly CharSequence ¤¤raceProblem = "This equipment is not applicable for the selected race.";
    public readonly HumanSprite sprite;
    public readonly AnimalSpecies mount;
    public readonly int formationAdd;
    public const int SLOTS = 8;

    static
    {
        D.ts(typeof(EquipBattle));
    }

    public EquipBattle(string coll, string key, PATH path, LISTE<Equip> all, LISTE<EquipBattle> mil, StatsInit init, KeyMap<TILE_SHEET> spriteMap) : base(coll, key, path, all, init)
    {
        iMil = mil.add(this);

        Json j = new Json(path.gets(key));
        if (RESOURCES.SUP().get(resource) != null)
            j.error("Can not have an equippable that is also a regular army supply!", resource.key);
        amountInGarrison = j.i("AMOUNT_IN_GARRISON", 0, equipMax);
        init.savers.put(coll + "_" + key + "_tars", tars);
        slotUse = j.ds("SLOT_USAGE", SLOTS);
        stat.info().setMatters(false, true);

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = new DivSprite();
            sprites[i].read(j.json("DIV_SPRITE"));
        }

        if (j.has("SPRITE"))
        {
            this.sprite = new HumanSprite(j, spriteMap);
        }
        else
            this.sprite = null;
        if (j.has("MOUNTED_ANIMAL"))
        {
            mount = SETT.ANIMALS().map.read("MOUNTED_ANIMAL", j);
        }
        else
            mount = null;

        formationAdd = j.i("ADD_TO_FORMATION_SIZE", 0, 100, 0);
    }

    public override int target(Induvidual h)
    {
        Div i = STATS.BATTLE().DIV.get(h);
        if (i != null)
        {
            if (SETT.ROOMS().GUARD.activeDuty.is(i) || i.settings().mustering())
                return target(i);
        }

        return 0;
    }

    public override double bValue(double equipped)
    {
        equipped = CLAMP.d(equipped, 0, 1);
        return equipped;
    }

    public override int max(Induvidual i)
    {
        return equipMax;
    }

    public int target(Div d)
    {
        return CLAMP.i(tars.get(d.index()), 0, equipMax);
    }

    public void targetSet(Div d, int t)
    {
        tars.set(d.index(), CLAMP.i(t, 0, equipMax));
    }

    public int max()
    {
        return equipMax;
    }

    public int indexMilitary()
    {
        return iMil;
    }

    public int garrisonAmount()
    {
        return amountInGarrison;
    }

    public double slotUse(int slot)
    {
        return slotUse[slot];
    }

    protected override void hoverP(GUI_BOX box)
    {
        base.hoverP(box);
    }

    public class DivSprite
    {
        public int ox = 0;
        public int oy = 0;
        public int z = 0;
        public SPRITE icon = UI.icons().s.cancel;
        public LIST<ColorImp> cols = new ArrayListGrower<ColorImp>();

        public void read(Json json)
        {
            ox = json.i("X", -100, 100, 0);
            oy = json.i("Y", -100, 100, 0);
            z = json.i("Z", -100, 100, 0);
            icon = UI.icons().get(json, UI.icons().s.cancel);
            cols = ColorImp.cols(json);
        }
    }

    public class HumanSprite
    {
        public readonly double offsetX;
        public readonly double offsetY;
        public readonly double animationX;
        public readonly double animationY;

        private TILE_SHEET sheet;
        public LIST<ColorImp> cols = new ArrayListGrower<ColorImp>();

        private HumanSprite(Json json, KeyMap<TILE_SHEET> map)
        {
            json = json.json("SPRITE");

            offsetX = json.d("OFFSET_X", -100, 100);
            offsetY = json.d("OFFSET_Y", -100, 100);
            animationX = json.d("ANIMATION_DX", -100, 100);
            animationY = json.d("ANIMATION_DY", -100, 100);

            string file = json.value("FILE");
            if (!map.containsKey(file))
            {
                sheet = new ITileSheet(PATHS.SPRITE().getFolder("race").getFolder("battle").get(file), 132, 36)
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.singles.init(0, 0, 1, 1, 2, 1, d.s24);
                        s.singles.setSkip(0, 2).paste(3, true);
                        return d.s24.saveGame();
                    }
                }.get();
                map.put(file, sheet);
            }
            sheet = map.get(file);
            cols = ColorImp.cols(json);
        }

        public void render(Induvidual a, SPRITE_RENDERER r, DIR dir, double forward, int x, int y, ShadowBatch s)
        {
            double am = get(a);
            if (am == 0)
                return;

            ColorImp.TMP.interpolate(cols, am / max());
            ColorImp.TMP.bind();

            int t = dir.id();

            x += C.SCALE * 12;
            y += C.SCALE * 12;

            double rotY = dir.xN() * offsetX + dir.yN() * offsetY;
            double rotX = -dir.yN() * offsetX + dir.xN() * offsetY;

            double aY = dir.xN() * animationX + dir.yN() * animationY;
            double aX = -dir.yN() * animationX + dir.xN() * animationY;

            int cx = (int)((rotX + aX * forward) * (a.race().physics.hitBoxsize()));
            int cy = (int)((rotY + aY * forward) * (a.race().physics.hitBoxsize()));

            sheet.renderC(r, t, x + cx, y + cy);
            s.setHeight(0).setDistance2Ground(a.race().physics.height() / 2);

            sheet.renderC(s, t, x + cx, y + cy);
        }
    }
}