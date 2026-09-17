using System.IO;
using game.time;
using settlement.main;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d.util.datatypes;
using snake2d.util.file;

public sealed class ROOM_BARBER : RoomBlueprintIns<Instance>, ROOM_SERVICE_NEED_HASER, ROOM_EMPLOY_AUTO
{
    public static readonly string TYPE = "BARBER";
    private readonly RoomServiceNeed data;

    private readonly Constructor constructor;
    private readonly Tile ll;

    public ROOM_BARBER(RoomInitData init, int typeIndex, string key, RoomCategorySub block) : base(typeIndex, init, key, block)
    {
        data = new RoomServiceNeed(this, init)
        {
            service = (int tx, int ty) => ll.service(tx, ty)
        };
        constructor = new Constructor(this, init);
        ll = new Tile(this, (int)(TIME.workSeconds() * init.data().d("WORK_TIME_IN_DAYS")));
    }

    public override Furnisher constructor()
    {
        return constructor;
    }

    protected override void update(double ds)
    {
        // TODO Auto-generated method stub
    }

    public SFinderRoomService service(int tx, int ty)
    {
        return data.finder;
    }

    protected override void saveP(FilePutter saveFile)
    {
        data.saver.save(saveFile);
    }

    protected override void loadP(FileGetter saveFile)
    {
        data.saver.load(saveFile);
    }

    protected override void clearP()
    {
        data.saver.clear();
    }

    public RoomServiceNeed service()
    {
        return data;
    }

    public bool autoEmploy(Room r)
    {
        return ((Instance)r).auto;
    }

    public void autoEmploy(Room r, bool b)
    {
        ((Instance)r).auto = b;
    }

    public DIR dir(int tx, int ty)
    {
        FurnisherItem it = SETT.ROOMS().fData.item.get(tx, ty);
        if (it == null)
            return DIR.N;
        return DIR.W.next(2 * it.rotation);
    }
}