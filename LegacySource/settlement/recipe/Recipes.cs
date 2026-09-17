using System;
using System.Collections.Generic;
using System.IO;
using game.VERSION;
using game.boosting;
using game.debug;
using game.faction;
using game.faction.npc;
using init.resources;
using init.sprite.UI;
using init.trade;
using settlement.main;
using settlement.room.industry.module;
using settlement.room.main;
using snake2d.util.file;
using snake2d.util.sets;
using util.text;

namespace settlement.recipe
{
    public class Recipes : SettResource
    {
        private readonly LIST<Recipe> all;
        private readonly Li[] map;
        private static readonly CharSequence ¤¤sname = "Captives";
        private static readonly CharSequence ¤¤sdesc = "Ability to produce captive";
        public static readonly CharSequence ¤¤realm = "Realm";
        public static readonly CharSequence ¤¤faction = "Faction";

        public readonly BoostableCat boostsSlave = new BoostableCat("SLAVE_PRODUCTION_", ¤¤sname, ¤¤sdesc, BoostableCat.TYPE_WORLD, UI.icons().m.slave);
        private readonly ArrayListGrower<FBoost> fboosts = new ArrayListGrower<FBoost>();

        static Recipes()
        {
            D.ts(typeof(Recipes));
        }

        public readonly RecipeRates rates;
        public readonly RecipeRatesVanilla ratesV;
        public readonly RecipeRatesPlayer player;

        public Recipes()
            : base("RECIPES", false)
        {
            all = Creator.all(boostsSlave, ¤¤sname, ¤¤sdesc, fboosts);
            map = new Li[TR.ALL().size()];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = new Li();
            }

            foreach (Recipe fi in all)
            {
                map[fi.out.index()].add(fi);
            }

            ArrayListGrower<ArrayListGrower<RoomBlueprintImp>> iiis = new ArrayListGrower<ArrayListGrower<RoomBlueprintImp>>();
            foreach (RESOURCE res in RESOURCES.ALL())
                iiis.add(new ArrayListGrower<RoomBlueprintImp>());

            foreach (Industry ins in SETT.ROOMS().industries.all)
            {
                foreach (IndustryResource ii in ins.ins())
                {
                    RESOURCE res = ii.resource;
                    if (iiis.get(res.index()).contains(ins.blue))
                        continue;
                    iiis.get(res.index()).add(ins.blue);
                }
            }

            rates = new RecipeRates(this);
            ratesV = new RecipeRatesVanilla(this);
            player = new RecipeRatesPlayer(this);
        }

        protected override void save(FilePutter file)
        {
            file.i(fboosts.size());
            foreach (FBoost b in fboosts)
                file.d(b.mul);
            player.saver.save(file);
            base.save(file);
        }

        protected override void load(FileGetter file)
        {
            clear();
            int am = file.i();
            for (int i = 0; i < am && i < fboosts.size(); i++)
                fboosts.get(i).mul = file.d();
            if (!VERSION.versionIsBefore(71, 14))
                player.saver.load(file);
            base.load(file);
        }

        protected override void clear()
        {
            foreach (FBoost b in fboosts)
                b.randomize();
        }

        public LIST<Recipe> all()
        {
            return all;
        }

        public LIST<Recipe> get(TRADABLE t)
        {
            return map[t.index()];
        }

        protected override void update(double ds, Profiler profiler)
        {
        }

        private class Li : ArrayListGrower<Recipe>
        {
            private static readonly long serialVersionUID = 1L;
        }

        public void randomizeAIBoosts()
        {
            foreach (FactionNPC f in FACTIONS.NPCs())
            {
                f.bonus.randomize();
            }
            foreach (FBoost b in fboosts)
                b.randomize();
        }
    }
}