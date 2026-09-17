using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using settlement.main;
using settlement.entity.animal.spawning;
using settlement.room.food.pasture;
using snake2d;
using util.keymap;
using util.rendering;
using view.sett;
using view.tool;

namespace settlement.entity.animal
{
    public sealed class Animals : SettResource
    {
        public readonly AnimalSpawning spawn;
        public readonly RMAPS<AnimalSpecies> map;
        public readonly LIST<AnimalSpecies> species;
        public readonly ArrayList<AnimalSpecies> caravans = new ArrayList<AnimalSpecies>();
        private LIST<AnimalSpecies> sett;
        public readonly Sprites sprites;

        public Animals() : base("ANIMALS", true)
        {
            ArrayListGrower<AnimalSpecies> all = new ArrayListGrower<AnimalSpecies>();

            PATH gData = PATHS.INIT().getFolder("animal");
            PATH gText = PATHS.TEXT().getFolder("animal");
            KeyMap<TILE_SHEET> sprites = new KeyMap<TILE_SHEET>();
            foreach (string key in gData.getFiles())
            {
                Json data = new Json(gData.gets(key));
                Json text = new Json(gText.gets(key));

                AnimalSpecies s = new AnimalSpecies(key, all.size(), data, text, sprites);
                all.add(s);
            }

            this.species = all;
            map = new RMAPS<AnimalSpecies>("ANIMAL", all);
            spawn = new AnimalSpawning(this);
            this.sprites = new Sprites();

            PLACABLE death = new PlacableSimple("kill animals")
            {
                public override void place(int x, int y)
                {
                    foreach (ENTITY e in ENTITIES().getAtPointL(x, y))
                    {
                        if (e is Animal)
                        {
                            ((Animal)e).kill(false, false);
                            return;
                        }
                    }
                }

                public override CharSequence isPlacable(int x, int y)
                {
                    foreach (ENTITY e in ENTITIES().getAtPointL(x, y))
                    {
                        if (e is Animal)
                        {
                            return null;
                        }
                    }
                    return E;
                }
            };

            IDebugPanelSett.add(death);

            foreach (AnimalSpecies s in species)
            {
                PlacableSimple p = new PlacableSimple(s.name)
                {
                    public override void place(int x, int y)
                    {
                        new Animal(x, y, s, null);
                    }

                    public override CharSequence isPlacable(int x, int y)
                    {
                        return Animals.this.isPlacable(s, x, y) ? null : E;
                    }
                };
                IDebugPanelSett.add("animal", p);
                if (s.caravanable)
                    caravans.add(s);
            }

            if (caravans.isEmpty())
                throw new Errors.DataError("No animals can be caravans");

            PlacableSimpleTile p = new PlacableSimpleTile("control animal")
            {
                public override void place(int tx, int ty)
                {
                    foreach (ENTITY e in SETT.ENTITIES().getAtTile(tx, ty))
                    {
                        if (e is Animal)
                        {
                            ((Animal)e).setState(State.CONTROLLED, 1);
                            e.physics.setMass(500);
                        }
                    }
                }

                public override CharSequence isPlacable(int tx, int ty)
                {
                    foreach (ENTITY e in SETT.ENTITIES().getAtTile(tx, ty))
                    {
                        if (e is Animal)
                            return null;
                    }
                    return E;
                }
            };

            IDebugPanelSett.add("animal", p);
        }

        protected override void save(FilePutter saveFile)
        {
            spawn.saver.save(saveFile);
        }

        protected override void update(double ds, Profiler profiler)
        {
            spawn.update(ds);
        }

        protected override void load(FileGetter saveFile) throws IOException
        {
            sett = exists(SETT.WORLD_AREA());
            spawn.saver.load(saveFile);
        }

        protected override void generate(CapitolArea area)
        {
            sett = exists(area);

            if (!area.isBattle)
                spawn.generate(this, area);
        }

        protected override void clear()
        {
            spawn.saver.clear();
        }

        public LIST<AnimalSpecies> sett()
        {
            return sett;
        }

        private LIST<AnimalSpecies> exists(CapitolArea area)
        {
            ArrayList<AnimalSpecies> res = new ArrayList<AnimalSpecies>(species.size());
            foreach (AnimalSpecies s in species)
            {
                if (exists(s, area))
                    res.add(s);
            }
            return res;
        }

        private bool exists(AnimalSpecies s, CapitolArea area)
        {
            foreach (ROOM_PASTURE p in SETT.ROOMS().PASTURES)
            {
                if (p.species == s && !p.isAvailable(area.climate()))
                    return false;
            }
            return true;
        }

        public void renderCaravan(SPRITE_RENDERER r, ShadowBatch s, double movement, int cx, int cy, RESOURCE res, int resAmount, bool inWater, int dir, int ran)
        {
            Sprite.renderCaravan(r, s, movement, cx, cy, res, resAmount, inWater, dir, ran);
        }

        public void renderMount(AnimalSpecies sp, SPRITE_RENDERER r, ShadowBatch s, double movement, int cx, int cy, bool inWater, int dir, int ran)
        {
            Sprite.renderMount(sp, r, s, movement, cx, cy, inWater, dir, ran);
        }

        public void renderCorpse(AnimalSpecies s, Renderer r, ShadowBatch shadows, float ds, int x, int y, int state, int rot, int ran, double statef, COLOR decay)
        {
            Sprite.renderCorpse(s, r, shadows, ds, x, y, state, rot, ran, statef, decay);
        }

        public bool isPlacable(AnimalSpecies s, int x, int y)
        {
            int x1 = (x - s.hitboxSize / 2);
            int x2 = (x + s.hitboxSize / 2);
            int y1 = (y - s.hitboxSize / 2);
            int y2 = (y + s.hitboxSize / 2);
            if (x1 < 0 || x2 >= PWIDTH || y1 < 0 || y2 >= PHEIGHT)
                return false;
            x1 /= C.TILE_SIZE;
            x2 /= C.TILE_SIZE;
            y1 /= C.TILE_SIZE;
            y2 /= C.TILE_SIZE;
            return !PATH().solidity.is(x1, y1) && !PATH().solidity.is(x2, y1) &&
                   !PATH().solidity.is(x1, y2) && !PATH().solidity.is(x2, y2) &&
                   ENTITIES().getAtPoint(x, y) == null;
        }
    }
}