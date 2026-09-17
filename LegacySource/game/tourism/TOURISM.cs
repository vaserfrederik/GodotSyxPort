using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game;
using game.debug;
using game.faction;
using game.time;
using init.constant;
using init.paths;
using init.race;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.main;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.statistics;
using view.sett;
using view.ui.wiki;

namespace game.tourism
{
    public sealed class TOURISM : GameResource
    {
        private static readonly double CREDITS = Config.sett().TOURIST_CRETIDS;
        private static readonly int MIN_EMPLOYEES = 100;
        private static readonly double MAX_EMPLOYEES = 1000;
        public static readonly int AMOUNT = Config.sett().TOURIST_PER_YEAR_MAX;

        private static TOURISM self;

        private readonly Review[] reviews = new Review[32];
        private readonly ArrayList<Review> list = new ArrayList<Review>(32);
        private readonly LIST<Race> tourists;
        public readonly HistoryInt history = new HistoryInt(24, TIME.seasons(), false);
        private readonly Updater updater;
        private readonly Bitmap1D permit;
        private double score = 0;
        private readonly ACTION wiki;

        private readonly double[] races;

        public TOURISM() : base("TOURISTS", false)
        {
            self = this;
            for (int i = 0; i < reviews.Length; i++)
                reviews[i] = new Review();

            LinkedList<Race> li = new LinkedList<Race>();
            foreach (Race r in RACES.all())
            {
                if (r.tourism().occurence > 0)
                {
                    li.add(r);
                }
            }
            tourists = new ArrayList<Race>(li);
            updater = new Updater();
            permit = new Bitmap1D(RACES.all().size(), false);
            wiki = WIKI.add(new Json(PATHS.RACE().text.getFolder("tourist").gets("_WIKI")));

            IDebugPanelSett.add("TOURIST_REVIEW", new ACTION
            {
                exe = () =>
                {
                    Induvidual i = new Induvidual(HTYPES.TOURIST(), RACES.playable().rnd());
                    service(i).cheatSetTotal(i, RND.rFloat());
                    RoomInstance ii = SETT.ROOMS().INN.instancesSize() > 0 ? SETT.ROOMS().INN.getInstance(RND.rInt(SETT.ROOMS().INN.instancesSize())) : null;
                    touristFinish(i, ii != null ? new Coo(ii.mX(), ii.mY()) : new Coo(-1, -1));
                }
            });

            races = new double[RACES.all().size()];
        }

        protected override void save(FilePutter file)
        {
            foreach (Review r in reviews)
                r.save(file);
            history.save(file);
            updater.save(file);
            permit.save(file);
            file.d(score);
            RACES.map().saver().save(races, file);
        }

        protected override void load(FileGetter file) => throw new NotImplementedException();

        protected override void update(double ds, Profiler prof)
        {
            prof.logStart(typeof(TOURISM));
            updater.update(ds);
            prof.logEnd(typeof(TOURISM));
        }

        public static RoomBlueprintIns attraction(Induvidual indu) => Updater.attraction(indu);

        public static int perYear() => (int)Math.Ceiling(self.updater.chance() * AMOUNT);

        public static StatService service(Induvidual i)
        {
            NEED n = need(i);
            if (n == null)
            {
                GAME.Notify("here");
            }
            return AI.modules().needs.service(i, n, STATS.RAN().getD(i, 15));
        }

        public static AiPlanActivation servicePlan(Humanoid a, AIManager d)
        {
            NEED n = need(a.indu());
            return AI.modules().needs.plan(a, d, n, STATS.RAN().getD(a.indu(), 15));
        }

        private static NEED need(Induvidual a)
        {
            long max = 0;
            foreach (NEED n in NEEDS.ALLSIMPLE())
            {
                if (n != NEEDS.TYPES().SHRINE && n != NEEDS.TYPES().TEMPLE && n != NEEDS.TYPES().SKINNYDIP)
                    max += (long)(1000 * a.race().bvalue(n.rate));
            }

            max *= STATS.RAN().getD(a, 21);
            foreach (NEED n in NEEDS.ALLSIMPLE())
            {
                if (n != NEEDS.TYPES().SHRINE && n != NEEDS.TYPES().TEMPLE && n != NEEDS.TYPES().SKINNYDIP)
                {
                    max -= (long)(1000 * a.race().bvalue(n.rate));
                    if (max <= 0)
                    {
                        return n;
                    }
                }
            }
            return NEEDS.ALLSIMPLE().get(0);
        }

        public static int credits(Race race) => (int)(race.tourism().credits * CREDITS);

        public static LIST<Race> races() => self.tourists;

        public static HISTORY_INT history() => self.history;

        public static bool permit(Race race) => self.permit.get(race.index());

        public static void permit(Race race, bool perm) => self.permit.set(race.index(), perm);

        public static double score() => self.score;

        public static void touristFinish(Induvidual tourist, COORDINATE inn)
        {
            if (SETT.ENTRY().beseiged())
            {
                return;
            }

            Review v = self.reviews[self.reviews.Length - 1];
            for (int i = self.reviews.Length - 1; i > 0; i--)
            {
                self.reviews[i] = self.reviews[i - 1];
            }
            self.reviews[0] = v;
            v.make(tourist, inn);

            self.score = (15 * self.score + v.score) / 16.0;
            self.score = CLAMP.d(self.score, 0, 1);

            if (SETT.ROOMS().INN.is(inn))
                SETT.ROOMS().INN.setReview(inn.x(), inn.y(), v);
            FACTIONS.player().credits().inc(v.credits, CTYPE.TOURISM);

            self.races[tourist.race().index()]++;
            for (int i = 0; i < self.races.Length; i++)
                self.races[i] /= 2;
        }

        public static LIST<Review> reviews()
        {
            self.list.clear();
            for (int i = 0; i < self.reviews.Length; i++)
            {
                if (self.reviews[i].has())
                    self.list.add(self.reviews[i]);
                else
                    break;
            }
            return self.list;
        }

        public static ACTION wiki() => self.wiki;

        public static double race(Race race) => TOURISM.self.races[race.index()];
    }
}