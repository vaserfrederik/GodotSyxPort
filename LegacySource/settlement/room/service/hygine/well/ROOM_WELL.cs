using System.IO;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d.util.file;

public sealed class ROOM_WELL : RoomBlueprintIns<WellInstance>, ROOM_SERVICE_NEED_HASER
{
    private readonly RoomServiceNeed data;
    private readonly Constructor constructor;
    private readonly Wash bed;

    public ROOM_WELL(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
    {
        bed = new Wash(this);
        data = new RoomServiceNeed(this, init)
        {
            Service = (tx, ty) => bed.Get(tx, ty),
            TotalMultiplier = () => base.TotalMultiplier()
        };
        constructor = new Constructor(this, init);
    }

    protected override void Update(double ds)
    {
    }

    public Wash Bed(int tx, int ty)
    {
        return bed.Get(tx, ty);
    }

    public Furnisher Constructor()
    {
        return constructor;
    }

    public SFinderRoomService Service(int tx, int ty)
    {
        return data.Finder;
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

    public RoomServiceNeed Service()
    {
        return data;
    }

    public bool RegistersEnvironment()
    {
        return true;
    }
}