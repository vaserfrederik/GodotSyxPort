using System.IO;
using settlement.main;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.job;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d.util.file;

public sealed class ROOM_LAVATORY : RoomBlueprintIns<LavatoryInstance>, ROOM_SERVICE_NEED_HASER, ROOM_EMPLOY_AUTO
{
    private readonly RoomServiceNeed data;
    private readonly Constructor constructor;

    public ROOM_LAVATORY(RoomInitData init, int typeIndex, string key, RoomCategorySub block) : base(typeIndex, init, key, block)
    {
        data = new RoomServiceNeed(this, init)
        {
            service = (tx, ty) => Lavatory.Get(tx, ty)
        };
        constructor = new Constructor(this, init);
    }

    public override Furnisher Constructor() => constructor;

    protected override void Update(double ds)
    {
        // TODO Auto-generated method stub
    }

    public Lavatory GetService(int tx, int ty) => Lavatory.Get(tx, ty);

    public override SFinderRoomService Service(int tx, int ty) => data.Finder;

    public bool IsExtra(int tx, int ty)
    {
        if (Is(tx, ty))
        {
            int d = ROOMS.Data.Get(tx, ty);
            return (d & Lavatory.BIT_WASH) == Lavatory.BIT_WASH;
        }
        return false;
    }

    protected override void SaveP(FilePutter saveFile)
    {
        data.Saver.Save(saveFile);
    }

    protected override void LoadP(FileGetter saveFile)
    {
        data.Saver.Load(saveFile);
    }

    protected override void ClearP()
    {
        data.Saver.Clear();
    }

    public override RoomServiceNeed Service() => data;

    public override bool AutoEmploy(Room r) => ((LavatoryInstance)r).Auto;

    public override void AutoEmploy(Room r, bool b) => ((LavatoryInstance)r).Auto = b;
}