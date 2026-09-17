using System;
using System.Collections.Generic;
using System.IO;

namespace game.battle.div
{
    public sealed class Div : ArmyDiv, BOOSTABLE_O, DIV_SIMPLE
    {
        private readonly short index;
        private readonly short armyIndex;
        private readonly Army army;
        public readonly DivMen men;
        public readonly DivTargets targets;

        private readonly DivPositionCopyable current;

        public readonly DivInfo info;
        public readonly DivReporter reporter = new DivReporter();

        public Div(List<Div> all, List<Div> armyAll, Army army)
        {
            this.index = (short)all.Add(this);
            armyIndex = (short)armyAll.Add(this);
            this.army = army;
            men = new DivMen();

            current = new DivPositionCopyable();
            targets = new DivTargets(this);
            info = new DivInfo(this);
        }

        public override Race race()
        {
            return info.race();
        }

        public override int men()
        {
            return men.men();
        }

        public int menPrevious()
        {
            return GAME.ARMIES().prevMen(this);
        }

        protected override void save(FilePutter file)
        {
            men.save(file);
            current.save(file);
            targets.saver().save(file);
            info.saver.save(file);
            reporter.unreachablem.save(file);
            file.s(reporter.unreachable);
        }

        protected override void load(FileGetter file)
        {
            men.load(file);
            current.load(file);
            targets.saver().load(file);
            info.saver.load(file);
            reporter.unreachablem.load(file);
            reporter.unreachable = file.s();
            army.men.recount();
        }

        protected override void clear()
        {
            men.clear();
            current.clear();
            targets.saver().clear();
            info.saver.clear();
            reporter.unreachablem.clear();
            reporter.unreachable = 0;
            settings().clear();
        }

        public short index()
        {
            return index;
        }

        public short indexArmy()
        {
            return armyIndex;
        }

        public Army army()
        {
            return army;
        }

        public bool player()
        {
            return army == GAME.ARMIES().player();
        }

        public Army armyEnemy()
        {
            return GAME.ARMIES().armies().getC(army.index() + 1);
        }

        public int menNrOf()
        {
            return men.men();
        }

        public int deployed()
        {
            return position().deployed();
        }

        public DIR dir()
        {
            return position().dir();
        }

        public Faction faction()
        {
            if (army() == GAME.ARMIES().player())
                return FACTIONS.player();
            return FACTIONS.otherFaction();
        }

        private static readonly Coo tmp = new Coo();

        public sealed class DivReporter : HDivStat
        {
            private readonly Bitmap1D unreachablem = new Bitmap1D(Config.battle().MEN_PER_DIVISION, false);
            private short unreachable;

            private DivReporter() { }

            public bool posHas(Humanoid a)
            {
                return position().deployed() >= men.getSpot(a.divSpot());
            }

            public COORDINATE getTile(Humanoid a)
            {
                COORDINATE c = position().tile(men.getSpot(a.divSpot()));
                if (c == null)
                    return a.tc();
                return c;
            }

            public COORDINATE getPixel(Humanoid a)
            {
                COORDINATE c = position().pixel(men.getSpot(a.divSpot()));
                if (c == null)
                {
                    tmp.set(a.body().cX(), a.body().cY());
                    return tmp;
                }
                return c;
            }

            public RECTANGLE body()
            {
                return position().body();
            }

            public COORDINATE getDestTile(Humanoid a)
            {
                COORDINATE c = position().centreTile();
                if (c == null)
                    return a.tc();
                return c;
            }

            public void reportPosition(short spot, int x, int y)
            {
                current.set(men.getSpot(spot), x, y);
            }

            public override short signUpAndGetPosition(int x, int y, Race r)
            {
                if (menNrOf() == 0)
                {
                    info.raceSet(r);
                    GAME.ARMIES().factors.init(this);
                }
                army.men.recount();
                short sp = men.getNewSpot();
                reportPosition(sp, x, y);
                current.init(men.men());

                return sp;
            }

            public override void returnPosition(short pos)
            {
                army.men.recount();
                men.returnSpot(pos);
                if (men.men() == 0)
                    settings().musteringSet(false);
                reportReachable(pos, true);
                current.init(men.men());
            }

            private void reportReachable(int spot, bool reachable)
            {
                if (unreachablem.get(spot) == true)
                    unreachable--;
                unreachablem.set(spot, !reachable);
                if (!reachable)
                    unreachable++;
            }

            public void reportReachable(Humanoid a, bool reachable)
            {
                int spot = positionSpot(a);
                reportReachable(spot, reachable);
            }

            public bool reachable(int i)
            {
                return !unreachablem.get(i);
            }

            public int unreachable()
            {
                return unreachable;
            }

            public int positionSpot(Humanoid h)
            {
                int i = STATS.BATTLE().position(h.indu());
                return men.getSpot(i);
            }

            public int positionSpot(int ui)
            {
                return men.spotTranslate(ui);
            }
        }

        public void hoverInfo(GBox text)
        {
            VIEW.UI().div.battle.hover(text, this);
        }

        public DivPositionCopyable current()
        {
            return current;
        }

        public void debug()
        {
            string s = Environment.NewLine;
            string res = "";
            res += "Div: " + index + " " + armyIndex + s;
            res += "Men: " + menNrOf() + " Deployed:" + deployed() + s;
            res += "Current: " + current.deployed() + " " + " " + s;

            GAME.Notify(res);
        }

        public bool active()
        {
            return menNrOf() > 0 && settings().mustering();
        }

        public override double boostableValue(BValue v)
        {
            return v.vGet(this);
        }

        public DivCentre centre()
        {
            return GAME.BATTLE_THREADS().centres.centre(index);
        }

        public DivStatus status()
        {
            return BattleStatus.status(this);
        }

        public Trajectory traj(Humanoid h)
        {
            return BattleTrajectories.request(h, this);
        }

        public DivFormation position()
        {
            return BattleOrders.next(this);
        }

        public BattleOrder order()
        {
            return BattleOrders.get(this);
        }

        public double morale()
        {
            return GAME.ARMIES().factors.morale(this);
        }

        public DivSettings settings()
        {
            return BattleSettings.get(this);
        }
    }
}