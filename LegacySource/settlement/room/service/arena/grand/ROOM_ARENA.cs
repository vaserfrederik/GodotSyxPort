using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Room.Service.Arena.Grand
{
    public class ROOM_ARENA : RoomBlueprintIns<ArenaInstance>, ROOM_SERVICE_NEED_HASER, ROOM_SPECTATOR.ROOM_SPECTATOR_HASER, PUNISHMENT_SERVICE
    {
        private readonly RoomServiceNeed data;
        private readonly Service ser;
        private readonly ArenaConstructor constructor;
        private int executions = 0;
        private int executionsMax = 0;
        private readonly BooleanOEImp<Race> permission;

        public ROOM_ARENA(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            constructor = new ArenaConstructor(this, init);
            ser = new Service(this);
            data = new RoomServiceNeed(this, init)
            {
                Service = (tx, ty) => ser.Get(tx, ty),
                IsGoodTime = () => Spec.isOpenNow()
            };
            Employment().SetShiftStart(ROOM_SPECTATOR.WORK_STARTSD, false);
            permission.Info = new INFO(ROOM_FIGHTPIT.¤¤kill, ROOM_FIGHTPIT.¤¤killD);
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public Furnisher Constructor()
        {
            return constructor;
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return data.Finder;
        }

        protected override void SaveP(FilePutter f)
        {
            data.Saver.Save(f);
            f.I(executions);
            f.I(executionsMax);
            permission.Save(f);
        }

        protected override void LoadP(FileGetter f)
        {
            data.Saver.Load(f);
            executions = f.I();
            executionsMax = f.I();
            permission.Load(f);
        }

        protected override void ClearP()
        {
            data.Saver.Clear();
            executions = 0;
            executionsMax = 0;
            permission.Clear();
        }

        public int PunishTotal()
        {
            return executionsMax;
        }

        public int PunishUsed()
        {
            return executions;
        }

        public BOOLEAN_OE<Race> PunishEnabled()
        {
            return permission;
        }

        public RoomServiceNeed Service()
        {
            return data;
        }

        public void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new RoomArenaGui(Work));
        }

        private readonly ROOM_SPECTATOR spec = new ROOM_SPECTATOR()
        {
            private Coo coo = new Coo();

            public RoomServiceAccess Service()
            {
                return ROOM_ARENA.this.Service();
            }

            public COORDINATE LookAt(int sx, int sy)
            {
                ArenaInstance ins = Getter.Get(sx, sy);
                if (ins == null)
                    coo.Set(sx, sy);
                else
                {
                    coo.Set(sx, sy);
                    RECTANGLE rec = Work.GladiatorArea(sx, sy);
                    int w = Math.Min(4, rec.Width());
                    int h = Math.Min(4, rec.Height());
                    int a = w * h;
                    int i = (sx + sy) % a;
                    coo.Set(rec.CX() - w / 2 + (i % (w)), rec.CY() - h / 2 + (i / h));
                }
                coo.Set(coo.X() * C.TILE_SIZE + C.TILE_SIZEH, coo.Y() * C.TILE_SIZE + C.TILE_SIZEH);
                return coo;
            }

            public bool Is(int sx, int sy)
            {
                ArenaInstance ins = Getter.Get(sx, sy);
                return ins != null;
            }

            private int Activity(int sx, int sy)
            {
                ArenaInstance ins = Getter.Get(sx, sy);
                if (ins == null)
                    return 0;

                int d = (int)TIME.CurrentSecond - ins.CheerTime;

                if (d > ArenaInstance.CHEER_TIME * 8)
                {
                    ins.CheerTime = (int)TIME.CurrentSecond;
                    ins.Cheer = false;
                    d = 0;
                }

                if (d <= ArenaInstance.CHEER_TIME)
                {
                    if (ins.Cheer)
                        return 1;
                    return 2;
                }
                return 0;
            }

            public bool IsActive(int sx, int sy)
            {
                return true;
            }

            public bool ShouldCheer(int sx, int sy)
            {
                return Activity(sx, sy) == 1;
            }

            public bool ShouldBoo(int sx, int sy)
            {
                return Activity(sx, sy) == 2;
            }

            public COORDINATE GetDestination(COORDINATE roomT)
            {
                coo.Set(roomT.X(), roomT.Y());
                return coo;
            }

            public bool IsSpot(int tx, int ty)
            {
                if (Ser.Init(tx, ty))
                    return true;
                return base.IsSpot(tx, ty);
            }

            public bool isOpenNow()
            {
                return TIME.Hours().BitCurrent() > 11 || TIME.Hours().BitCurrent() < 6;
            }
        };

        public ROOM_SPECTATOR Spec()
        {
            return spec;
        }

        public readonly RoomArenaWork Work = new RoomArenaWork()
        {
            private Coo coo = new Coo();

            public bool GladiatorInArena(int tx, int ty)
            {
                ArenaInstance ins = Getter.Get(tx, ty);
                if (ins != null)
                {
                    return ins.Arena.HoldsPoint(tx, ty);
                }
                return false;
            }

            public COORDINATE GladiatorGetSpot(RoomInstance ins)
            {
                ArenaInstance a = (ArenaInstance)ins;
                int w = a.Arena.Width();
                int h = a.Arena.Height();
                coo.Set(a.Arena.X1() + RND.RInt(w), a.Arena.Y1() + RND.RInt(h));
                return coo;
            }

            public void GladiatorDrawMakeSheer(COORDINATE coo)
            {
                ArenaInstance ins = Getter.Get(coo);
                if (ins != null)
                {
                    ins.CheerTime = (int)TIME.CurrentSecond;
                    ins.Cheer = !RND.OneIn(6);
                }
            }

            public RECTANGLE GladiatorArea(int tx, int ty)
            {
                ArenaInstance ins = Getter.Get(tx, ty);
                if (ins != null)
                {
                    return ins.Arena;
                }
                return null;
            }

            public RoomInstance ReserveDeath(COORDINATE coo)
            {
                if (executions >= executionsMax || InstancesSize() <= 0)
                    return null;

                {
                    ArenaInstance ins = Getter.Get(coo);
                    if (ins != null && ins.Executions < 4)
                    {
                        ins.Executions++;
                        executions++;
                        return ins;
                    }
                }
                int ri = RND.RInt(InstancesSize());

                for (int i = 0; i < InstancesSize(); i++)
                {
                    ArenaInstance ins = GetInstance((i + ri) % InstancesSize());
                    if (ins.Active() && ins.Employees().Employed() > 0 && ins.Executions < 4)
                    {
                        ins.Executions++;
                        executions++;
                        return ins;
                    }
                }
                return null;
            }

            public void UnreserveDeath(int tx, int ty)
            {
                ArenaInstance ins = Getter.Get(tx, ty);
                if (ins != null)
                {
                    ins.Executions--;
                    ins.Executions = (byte)Math.Max(ins.Executions, 0);
                    if (ins.Active() && ins.Employees().Employed() > 0)
                    {
                        executions--;
                    }
                }
            }

            public int Executions()
            {
                return executions;
            }

            public int ExecutionsMax()
            {
                return executionsMax;
            }

            public int Executions(RoomInstance ins)
            {
                if (ins is ArenaInstance i)
                {
                    return i.Executions;
                }
                return 0;
            }

            public int ExecutionsMax(RoomInstance ins)
            {
                return 4;
            }
        };
    }
}