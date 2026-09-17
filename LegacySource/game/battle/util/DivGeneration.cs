using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.battle.util;
using game.faction;
using init.constant;
using init.race;
using init.type;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using settlement.stats.stat;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using world.army;

namespace game.battle.util
{
    public sealed class DivGeneration
    {
        public Induvidual[] indus;
        public readonly short race;
        public readonly string name;
        public readonly int bannerI;
        public readonly bool isRange;
        public readonly DIV_SETTINGImp target = new DIV_SETTINGImp();

        public DivGeneration(DIV_SPEC div, DIV_SETTING target)
        {
            indus = new Induvidual[div.men()];
            race = (short)div.race().index();
            this.name = "" + div.name();
            this.bannerI = div.bannerI();
            if (div.men() > Config.battle().MEN_PER_DIVISION)
                throw new Exception();

            for (int i = 0; i < indus.Length; i++)
            {
                Induvidual ii = new Induvidual(HTYPES.SUBJECT(), div.race());
                init(div, ii, true);
                indus[i] = ii;
            }
            isRange = range(div);

            this.target.copySettings(target);
            this.target.men = div.men();
        }

        private DivGeneration(DIV_SPEC div, string name, int bannerI, LIST<Induvidual> all, DIV_SETTING target)
        {
            indus = new Induvidual[div.men()];
            race = (short)div.race().index();
            this.name = "" + name;
            this.bannerI = bannerI;

            for (int i = 0; i < indus.Length; i++)
            {
                indus[i] = all.get(i);
                init(div, indus[i], false);
            }
            isRange = range(div);
            this.target.copySettings(target);
            this.target.men = div.men();
        }

        public DivGeneration(WDIV div, LIST<Induvidual> all, DIV_SETTING target)
            : this(div, div.name(), div.bannerI(), all, tar(div, target))
        {
        }

        private static DIV_SETTING tar(WDIV div, DIV_SETTING target)
        {
            DIV_SETTINGImp tar = new DIV_SETTINGImp();
            tar.copySettings(div);
            tar.men = div.men();
            return tar;
        }

        public static DivGeneration rnd()
        {
            DIV_SPEC stats = new DIV_SPEC()
            {
                Race = RACES.all().rnd(),
                Dd = GAME.battle().types.rnd(stats.Race, FACTIONS.player(), RND.rFloat()),

                Men = CLAMP.i(RND.rInt(Config.battle().MEN_PER_DIVISION) + 25, 1, Config.battle().MEN_PER_DIVISION),
                Name = "" + stats.Race.info.armyNames.rnd(),
                BannerI = RND.rInt(GAME.ARMIES().banners.size()),

                Tr = RND.rFloat(),
                Eq = RND.rFloat(),
                Ex = RND.rFloat()
            };

            return new DivGeneration(stats, stats);
        }

        public void setMen(int men)
        {
            Induvidual[] in = new Induvidual[Math.Min(men, indus.Length)];
            for (int i = 0; i < in.Length; i++)
                in[i] = indus[i];
            indus = in;
        }

        public DivGeneration(FileGetter file) : this(file)
        {
        }

        private DivGeneration(FileGetter file)
        {
            race = (short)file.i();
            name = file.chars();
            bannerI = file.i();
            isRange = file.bool();

            indus = new Induvidual[file.i()];
            for (int i = 0; i < indus.Length; i++)
                indus[i] = new Induvidual(file);

            if (!VERSION.versionIsBefore(70, 25))
                target.load(file);
            else
                target.men = indus.Length;
        }

        private static bool range(DIV_SPEC div)
        {
            foreach (EquipRange r in STATS.EQUIP().RANGED())
                if (div.equip(r) > 0)
                    return true;
            return false;
        }

        public void save(FilePutter file)
        {
            file.i(race);
            file.chars(name);
            file.i(bannerI);
            file.bool(isRange);

            file.i(indus.Length);
            foreach (Induvidual a in indus)
                a.save(file);
            target.save(file);
        }

        public Race race()
        {
            return RACES.all().get(race);
        }

        private void init(DIV_SPEC div, Induvidual ii, bool training)
        {
            if (training)
            {
                set(ii, STATS.BATTLE().COMBAT_EXPERIENCE, div.experience());
                foreach (StatTraining tt in STATS.BATTLE().TRAINING_ALL)
                    set(ii, tt.stat, div.training(tt));
            }

            foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
            {
                set(ii, e.stat(), div.equip(e));
            }
        }

        private static void set(Induvidual ii, STAT s, double d)
        {
            double ex = d * s.indu().max(ii);
            if (ex != (int)ex && (ex - (int)ex) > RND.rFloat())
                ex++;
            ex = CLAMP.d(ex, 0, s.indu().max(ii));

            s.indu().set(ii, (int)ex);
        }

        public DIV_SPECImp makeSpec()
        {
            DIV_SPECImp spec = new DIV_SPECImp();

            spec.raceSet(race());
            spec.menSet(indus.Length);

            foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
            {
                double am = 0;
                foreach (Induvidual a in indus)
                {
                    am += e.get(a);
                }
                am /= (indus.Length * e.max());
                spec.equipSet(e, am);
            }

            double exp = 0;
            foreach (StatTraining t in STATS.BATTLE().TRAINING_ALL)
            {
                double am = 0;
                foreach (Induvidual a in indus)
                {
                    am += t.stat.indu().getD(a);
                    exp += STATS.BATTLE().COMBAT_EXPERIENCE.indu().getD(a);
                }
                am /= (indus.Length);
                spec.trainingSet(t, am);
            }

            spec.experienceSet(exp / indus.Length);
            return spec;
        }
    }
}