using System;
using System.Collections.Generic;
using System.IO;

using game.battle.div;
using init.constant;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.employment;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using snake2d.util.file;
using snake2d.util.sets;
using util.data;
using util.info;
using util.text;
using view.sett.ui.room;

public sealed class ROOM_GUARD : RoomBlueprintIns<GuardInstance>
{
    private static readonly string ¤¤guard = "Active Duty";
    private static readonly string ¤¤guardD = "When a division is on active duty, the soldiers will become guards who actively protects your city against crime, and instills order and law.";

    static
    {
        D.ts(typeof(ROOM_GUARD));
    }

    public const int maxRadius = 90;

    private readonly SFinderRoomService finder;

    private readonly Constructor constructor;
    private readonly Service service = new Service(this);

    public readonly EmployerSimple emp = new EmployerSimple(employment());

    public readonly Patrols patrols = new Patrols();
    public readonly GuardPower power = new GuardPower();
    public readonly CrimeReporter reporter;

    private readonly Bitmap1D guardMode;

    public ROOM_GUARD(RoomInitData init, RoomCategorySub block) : base(0, init, "_GUARD", block)
    {
        finder = new SFinderRoomService("Guards")
        {
            public FSERVICE get(int tx, int ty)
            {
                GuardInstance ins = getter.get(tx, ty);
                if (ins != null && ins.body().cX() == tx && ins.body().cY() == ty)
                    return service.get(ins);
                return null;
            }
        };
        constructor = new Constructor(this, init);
        reporter = new CrimeReporter(this);
        guardMode = new Bitmap1D(Config.battle().DIVISIONS_PER_ARMY, false);
    }

    protected override void update(double ds)
    {
        patrols.update(ds);
    }

    public override Furnisher constructor()
    {
        return constructor;
    }

    public override SFinderRoomService service(int tx, int ty)
    {
        return finder;
    }

    protected override void saveP(FilePutter saveFile)
    {
        guardMode.save(saveFile);
        power.save(saveFile);
        patrols.save(saveFile);
        reporter.save(saveFile);
    }

    protected override void loadP(FileGetter saveFile)
    {
        guardMode.load(saveFile);
        power.load(saveFile);
        patrols.load(saveFile);
        reporter.load(saveFile);
    }

    protected override void clearP()
    {
        guardMode.clear();
        power.clear();
        patrols.clear();
        reporter.clear();
    }

    public override void appendView(List<UIRoomModule> mm)
    {
        mm.Add(new Gui(this).make());
    }

    public BOOLEAN_OE<Div> activeDuty = new BOOLEAN_OE<Div>()
    {
        private readonly INFO info = new INFO(¤¤guard, ¤¤guardD);

        public bool is(Div t)
        {
            return guardMode.get(t.indexArmy());
        }

        public BOOLEAN_OE<Div> set(Div t, bool b)
        {
            guardMode.set(t.indexArmy(), b);
            return this;
        }

        public INFO info()
        {
            return info;
        }
    };
}