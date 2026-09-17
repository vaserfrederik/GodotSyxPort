using System;
using System.IO;

using init.religion;
using init.type;
using settlement.misc.util;
using settlement.path.finders;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.service.module;
using snake2d.util.file;

public sealed class ROOM_SHRINE : RoomBlueprintIns<ShrineInstance>, ROOM_SERVICE_HASER
{
    public readonly Religion religion;
    private readonly RoomService data;

    private readonly Constructor constructor;
    private readonly Service bed;

    public ROOM_SHRINE(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
    {
        religion = RELIGIONS.MAP().Read(init.data());
        bed = new Service(this);
        data = new RoomService(this, init, NEEDS.TYPES().SHRINE)
        {
            public FSERVICE Service(int tx, int ty)
            {
                return bed.Get(tx, ty);
            }
        };
        constructor = new Constructor(this, init);
    }

    protected override void Update(double ds)
    {
        // TODO Auto-generated method stub
    }

    public Service Bed(int tx, int ty)
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

    public RoomService Service()
    {
        return data;
    }
}