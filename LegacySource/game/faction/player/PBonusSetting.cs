using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.data;
using util.text;

namespace game.faction.player
{
    public class PBonusSetting : SAVABLE
    {
        private static readonly CharSequence ¤¤name = "¤Setting";
        private static readonly CharSequence ¤¤mode = "¤Mode:";
        private readonly ArrayListGrower<ParseValue> boosts = new ArrayListGrower<ParseValue>();

        public readonly StockpileImp startResources = new StockpileImp();
        public int startLevel = 0;
        public string mode = "";

        static PBonusSetting()
        {
            D.ts(typeof(PBonusSetting));
        }

        protected PBonusSetting()
        {
            foreach (var k in PATHS.PLAYER().init.getFolder("mode").getFiles())
            {
                var j = new Json(PATHS.PLAYER().text.getFolder("mode").get(k));
                string n = j.text("NAME");
                GVALUES.FACTION.push("PLAY_MODE_" + k, ¤¤mode + " " + n, UI.icons().s.cog, new BOOLEANO<Faction>()
                {
                    public override bool is(Faction t)
                    {
                        return k.Equals(mode);
                    }
                });
            }
        }

        public void add(string key, double am, bool isMul)
        {
            if (isMul && am == 1 || am == 0)
                return;
            ParseValue a = new ParseValue(key, am, isMul);
            if (add(a))
            {
                boosts.add(a);
            }
        }

        private bool add(ParseValue a)
        {
            string key = a.key;
            double am = a.am;
            bool isMul = a.isMul;
            LIST<Boostable> bb = BOOSTING.MAP().get(key);
            if (bb == null)
            {
                LOG.err(key);
                return false;
            }
            foreach (Boostable b in bb)
            {
                BValue v = new BValue.BValuePlayerOnly()
                {
                    public override double vGet(FactionNPC f)
                    {
                        return 0;
                    }

                    public override double vGet(Player f)
                    {
                        return 1.0;
                    }

                    public override double vGet(HCLASS_RACE t)
                    {
                        if (b == BOOSTABLES.BEHAVIOUR().HAPPI)
                        {
                            if (t.cl == HCLASSES.CITIZEN() && STATS.POP().POP.data(HCLASSES.CITIZEN()).get(t.race) == 0)
                                return 0;
                        }
                        return 1;
                    }
                };

                Booster bo = new BoosterValue(v, new BSourceInfo(¤¤name, UI.icons().s.alert), am, isMul);
                bo.add(b);
            }
            return true;
        }

        public override void save(FilePutter file)
        {
            file.i(boosts.size());

            foreach (ParseValue a in boosts)
            {
                file.chars(a.key);
                file.d(a.am);
                file.bool(a.isMul);
            }
            startResources.save(file);
            file.i(startLevel);
            file.chars(mode);
        }

        public override void load(FileGetter file) throws IOException
        {
            boosts.clear();
            int am = file.i();

            for (int i = 0; i < am; i++)
            {
                ParseValue a = new ParseValue(file.chars(), file.d(), file.bool());
                if (add(a))
                    boosts.add(a);
            }
            startResources.load(file);
            startLevel = file.i();
            mode = file.chars();

            if (mode.Length > 0)
            {
                setMode(mode);
            }
        }

        public void copy(PBonusSetting old)
        {
            boosts.clear();
            foreach (ParseValue a in old.boosts)
                add(a);
            for (int ri = 0; ri < RESOURCES.ALL().size(); ri++)
            {
                startResources.set(RESOURCES.ALL().get(ri), old.startResources.get(ri));
            }
            startLevel = old.startLevel;
        }

        public override void clear()
        {
            //boosts.clear();
        }

        public void apply()
        {
            if (startLevel > 0)
                FACTIONS.player().level().set(startLevel);

            Flooder f = GUTIL.flooder();
            f.init(this);
            int ri = 0;

            f.pushSloppy(THRONE.coo(), 0);

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();

                if (!SETT.PATH().solidity.is(t) && SETT.THINGS().resources.get(t.x(), t.y()) == null)
                {
                    while (ri < RESOURCES.ALL().size())
                    {
                        int am = startResources.get(ri);
                        if (am > 0)
                        {
                            if (am > 100)
                                am = 100;
                            startResources.inc(RESOURCES.ALL().get(ri), -am);
                            SETT.THINGS().resources.create(t, RESOURCES.ALL().get(ri), am);
                            break;
                        }
                        else
                        {
                            ri++;
                        }
                    }

                    if (ri >= RESOURCES.ALL().size())
                        break;

                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (!SETT.PATH().solidity.is(t, d))
                            f.pushSmaller(t, d, t.getValue() + 1);
                    }
                }
            }
            f.done();
        }

        public void setMode(string selectedMode)
        {
            if (!PATHS.PLAYER().folder("mode").init.exists(selectedMode))
                return;
            var j = new Json(PATHS.PLAYER().folder("mode").init.gets(selectedMode));

            j = j.json(BOOSTING.KEY);

            foreach (string k in j.keys())
            {
                ParseValue v = new ParseValue(j, k);
                add(v);
            }

            mode = selectedMode;
        }
    }
}