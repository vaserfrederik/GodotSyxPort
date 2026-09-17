using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Service.Pleasure
{
    public class RoomPleasure : RoomBlueprintIns<PleasureInstance>, ROOM_EMPLOY_AUTO, ROOM_SERVICE_NEED_HASER
    {
        public static readonly string TYPE = "PLEASURE";
        private Constructor constructor;
        private ABed bed;
        private RoomServiceNeed service;

        public RoomPleasure(string key, int tindex, RoomInitData init, RoomCategorySub block) : base(tindex, init, key, block)
        {
            bed = new ABed(this);
            constructor = new Constructor(this, init);
            service = new RoomServiceNeed(this, init)
            {
                Service = (tx, ty) =>
                {
                    if (bed.Init(tx, ty) != null)
                        return bed.service;
                    return null;
                }
            };
        }

        protected override void Update(double ds)
        {
            
        }

        public void AppendView(List<UIRoomModule> mm)
        {
            
        }

        public Furnisher Constructor()
        {
            return constructor;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            service.Saver.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            service.Saver.Load(saveFile);
        }

        protected override void ClearP()
        {
            service.Saver.Clear();
        }

        public bool AutoEmploy(Room r)
        {
            return ((PleasureInstance)r).Auto;
        }

        public void AutoEmploy(Room r, bool b)
        {
            ((PleasureInstance)r).Auto = b;
        }

        public SFinderFindable Service(int tx, int ty)
        {
            if (bed.Init(tx, ty) != null)
                return service.Finder;
            return null;
        }

        public RoomServiceNeed Service()
        {
            return service;
        }

        public bool ClientShouldUndress(int tx, int ty)
        {
            if (bed.Init(tx, ty) != null)
            {
                return bed.ClientShouldUndress();
            }
            return false;
        }

        public void ClientUndress(int tx, int ty)
        {
            if (bed.Init(tx, ty) != null)
            {
                bed.ClientUndress();
            }
        }

        public bool WorkerReadyShouldUndress(int tx, int ty)
        {
            if (bed.Init(tx, ty) != null)
            {
                return bed.WorkerReadyShouldUndress();
            }
            return false;
        }
    }
}