using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Settlement.Room.Military.Artillery
{
    public sealed class RoomArtillery : RoomBlueprintIns<ArtilleryInstance>
    {
        public static readonly string Type = "ARTILLERY";
        private readonly Constructor constructor;

        public readonly RESOURCE Projectile;

        private volatile bool threadLock;
        private readonly List<ArtilleryInstance> threadSafe = new List<ArtilleryInstance>(256);
        public readonly Projectile ProjectileInstance;

        private readonly Service service = new Service(this);
        private double reference = 0;

        private double updateInterval = 0;

        private static readonly string controlText = "¤Control artillery piece in the battle view.";

        public readonly int Services = 6;

        static RoomArtillery()
        {
            D.ts(typeof(RoomArtillery));
        }

        public PlacableFixed Placer { get; private set; }

        public RoomArtillery(int ti, RoomInitData data, string key, RoomCategorySub cat) : base(ti, data, key, cat)
        {
            constructor = new Constructor(data, this)
            {
                Create = (area, init) => new ArtilleryInstance(this, area, init)
            };
            PushBo(data.Data(), Type, true);
            Projectile = RESOURCES.Map().Read("PROJECTILE_RESOURCE", data.Data());
            ProjectileInstance = new ProjectileImp(data.Data(), "ROOM_" + key);
            Placer = new Placer(this, data.M);
        }

        protected override void SaveP(FilePutter f)
        {
            
        }

        protected override void LoadP(FileGetter f)
        {
            
        }

        protected override void ClearP()
        {
            
        }

        protected override void Update(double ds)
        {
            updateInterval -= ds;
            if (updateInterval < 0)
            {
                Lock();
                threadSafe.Clear();
                for (int k = 0; k < InstancesSize(); k++)
                    threadSafe.Add(GetInstance(k));
                threadLock = false;
                reference = Bonus().Get(HCLASS_RACE.clP(null, null)) / Bonus().Max(HCLASS_RACE.Class);
                updateInterval += 3;
            }
        }

        public override SFinderFindable Service(int tx, int ty)
        {
            ArtilleryInstance ins = Get(tx, ty);
            if (ins != null)
                return SETT.PATH().Finders.Manning(ins.Army());
            return null;
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public override void AppendView(List<UIRoomModule> modules)
        {
            modules.Add(new UIRoomModule
            {
                Hover = (box, room, rx, ry) =>
                {
                    ArtilleryInstance ins = (ArtilleryInstance)room;
                    Hoverer.Hover(box, ins);
                    if (ins.Army() == GAME.ARMIES().Player())
                    {
                        box.NL();
                        box.Text(controlText);
                    }
                }
            });
        }

        public double Reference()
        {
            return reference;
        }

        private void Lock()
        {
            while (threadLock)
                Thread.Yield();
            threadLock = true;
        }

        public void ThreadInstances(List<ArtilleryInstance> res)
        {
            Lock();
            res.AddRange(threadSafe);
            threadLock = false;
        }
    }
}