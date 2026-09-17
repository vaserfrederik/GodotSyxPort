using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using game;
using game.battle.div;
using game.boosting;
using game.faction.npc;
using game.faction.player;
using init.race.bio;
using init.type;
using settlement.main;
using settlement.path;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.stats;
using settlement.stats.standing;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room;
using world.map.regions;

namespace settlement.room.infra.monument
{
    public abstract class ROOM_MONUMENT : RoomBlueprintImp
    {
        public readonly Bitsmap2D mapData = new Bitsmap2D(0, 4, SETT.TILE_BOUNDS);
        public readonly Bitsmap2D mapUpgrade = new Bitsmap2D(0, 2, SETT.TILE_BOUNDS);

        private readonly AVAILABILITY avail;
        private readonly int MAX_VALUE;
        protected readonly RoomSingleton instance;
        private int area;
        private int degrade;
        private int upgrade;
        public readonly StandingDef defaultStanding;
        public readonly StandingDef defaultStandingUp;
        public readonly Opinion opinion;
        public readonly int monumentIndex;
        public readonly BoostSpecs boosts;

        private static int ii = 0;
        static ROOM_MONUMENT()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    ii = 0;
                }
            };
        }

        protected ROOM_MONUMENT(RoomInitData init, int tindex, string key, RoomCategorySub cat) : base(init, tindex, key, cat)
        {
            this.instance = new Instance(init.m, this);
            defaultStanding = new StandingDef(init.data());
            if (init.data().Has("STANDING_UPGRADE"))
                defaultStandingUp = new StandingDef(init.data().Json("STANDING_UPGRADE"));
            else
                defaultStandingUp = null;

            opinion = new Opinion();
            opinion.Read(init.text());
            monumentIndex = ii++;
            MAX_VALUE = init.data().I("MAX_VALUE");
            boosts = new BoostSpecs(info.names, icon, false);
            avail = init.data().Bool("SOLID", true) ? AVAILABILITY.SOLID : AVAILABILITY.ROOM;
            BValue v = new BValue
            {
                public double vGet(FactionNPC f) => 0,
                public double vGet(Player f) => vGet(HCLASS_RACE.clP()),
                public double vGet(HCLASS_RACE t) => STATS.ACCESS().MONUMENTS.ALL().Get(tindex).data(t.cl).GetD(t.race),
                public double vGet(Div div) => STATS.ACCESS().MONUMENTS.ALL().Get(tindex).div().GetD(div),
                public double vGet(Induvidual indu) => STATS.ACCESS().MONUMENTS.ALL().Get(tindex).indu().GetD(indu),
                public double vGet(Region reg) => vGet(reg.faction())
            };
            boosts.Read("FULFILLMENT_BONUS", init.data(), v);
        }

        protected override void Save(FilePutter f)
        {
            f.I(area);
            f.I(degrade);
            f.I(upgrade);
        }

        protected override void Load(FileGetter f)
        {
            area = f.I();
            degrade = f.I();
            upgrade = f.I();
        }

        protected override void Clear()
        {
            area = 0;
            degrade = 0;
            upgrade = 0;
        }

        public int Area() => area;

        public double Degrade()
        {
            if (area == 0)
                return 0;
            return (double)degrade / area;
        }

        public double Upgrade()
        {
            if (area == 0)
                return 0;
            if (upgrades().Max() == 0)
                return 1;
            return (double)(upgrade + area) / (area * (upgrades().Max() + 1));
        }

        public override Room Get(int tx, int ty)
        {
            if (ROOMS().map.Get(tx, ty) == instance)
                return instance;
            return null;
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public override SFinderRoomService Service(int tx, int ty) => null;

        public override void AppendView(LISTE<UIRoomModule> mm)
        {
            mm.Add(new UIRoomModule
            {
                public void Hover(GBox box, Room room, int rx, int ry)
                {
                    box.NL();
                    if (upgrades().Max() > 0)
                    {
                        box.NL();
                        box.Text(Dic.¤¤Upgrade);
                        box.Tab(6);
                        box.Add(GFORMAT.iofkInv(box.Text(), room.Upgrade(rx, ry), upgrades().Max()));
                        box.NL();
                    }
                    box.Text(Dic.¤¤Degrade);
                    box.Tab(6);
                    box.Add(GFORMAT.percInv(box.Text(), room.GetDegrade(rx, ry)));
                    box.NL();
                }
            });
        }

        private sealed class Instance : RoomSingleton
        {
            private static readonly long serialVersionUID = 1L;

            public Instance(ROOMS m, RoomBlueprint p) : base(m, p)
            {
            }

            protected override object ReadResolve() => blueprintI().instance;

            public new ROOM_MONUMENT blueprintI() => (ROOM_MONUMENT)blueprint();

            protected override void AddAction(ROOMA ins)
            {
                blueprintI().area += ins.Area();
                blueprintI().area = CLAMP.I(blueprintI().area, 0, SETT.TAREA);
                blueprintI().degrade += ins.Area() * GetDegrade(ins.mX(), ins.mY());
                blueprintI().upgrade += ins.Area() * upgrade(ins.mX(), ins.mY());
            }

            protected override void RemoveAction(ROOMA ins)
            {
                blueprintI().area -= ins.Area();
                blueprintI().area = CLAMP.I(blueprintI().area, 0, SETT.TAREA);
                blueprintI().degrade -= ins.Area() * GetDegrade(ins.mX(), ins.mY());
                blueprintI().upgrade -= ins.Area() * upgrade(ins.mX(), ins.mY());
                base.RemoveAction(ins);
            }

            protected override void DegradeChange(int mx, int my, double oldD, double newD, bool realDegradeChange)
            {
                blueprintI().degrade -= Area(mx, my) * oldD;
                base.DegradeChange(mx, my, oldD, newD, realDegradeChange);
                blueprintI().degrade += Area(mx, my) * oldD;
                if (realDegradeChange)
                    SETT.ENV().map.MONUMENT.ChangeDegrade(mx, my);
            }

            public override int Upgrade(int tx, int ty)
            {
                return CLAMP.I(SETT.ROOMS().extraBit.Get(mX(tx, ty), mY(tx, ty)), 0, blueprintI().upgrades().Max());
            }

            public override void UpgradeSet(int tx, int ty, int upgrade)
            {
                if (upgrade == Upgrade(tx, ty))
                    return;
                blueprintI().upgrade -= Area(tx, ty) * Upgrade(tx, ty);
                int up = CLAMP.I(upgrade, 0, blueprintI().upgrades().Max());
                SETT.ROOMS().extraBit.Set(tx, ty, up);
                blueprintI().upgrade += Area(tx, ty) * Upgrade(tx, ty);
                ROOMA a = SETT.ROOMS().map.rooma.Get(tx, ty);
                foreach (COORDINATE c in a.Body())
                {
                    if (a.Is(c))
                        SETT.MAINTENANCE().SetChanged(c.x(), c.y());
                }
                SETT.ENV().map.MONUMENT.ChangeUpgrade(tx, ty);
            }
        }

        public int[] radius = new int[] { 5, 10, 15 };

        public double Radius(FurnisherItem it)
        {
            if (it == null)
                return 0;
            return radius[it.width() - 1];
        }

        public int MaxEnv() => MAX_VALUE;

        public readonly MAP_DOUBLE envValue = new MAP_DOUBLE
        {
            public double Get(int tx, int ty) => (double)mapData.Get(tx, ty) / MAX_VALUE,
            public double Get(int tile) => (double)mapData.Get(tile) / MAX_VALUE
        };
    }
}