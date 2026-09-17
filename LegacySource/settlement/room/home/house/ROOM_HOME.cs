using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Home.House
{
    using Init.Type;
    using Settlement.Main;
    using Settlement.Path.Finders;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Category;
    using Settlement.Room.Main.Furnisher;
    using Settlement.Room.Main.Util;
    using Settlement.Thing.Pointlight;
    using Snake2D.Util.Color;
    using Snake2D.Util.File;
    using Snake2D.Util.Map;
    using Snake2D.Util.Sets;
    using View.Sett.Ui.Room;

    public class RoomHome : RoomBlueprintImp
    {
        private readonly int[] total;
        private readonly int[] used;

        private int totalT;
        private int usedT;

        public readonly HomeContructor constructor;
        public readonly OddHome odd = new OddHome();

        public RoomHome(RoomInitData init, RoomCategorySub cat) : base(init, 0, "_HOME", cat)
        {
            constructor = new HomeContructor(init, this);
            total = new int[HGROUP.All().Count];
            used = new int[HGROUP.All().Count];
        }

        protected override void Update(double ds)
        {
        }

        protected override void Save(FilePutter file)
        {
            odd.saver.Save(file);
            HGROUP.MAP().saver().Save(total, file);
            HGROUP.MAP().saver().Save(used, file);
        }

        protected override void Load(FileGetter file)
        {
            odd.saver.Load(file);
            HGROUP.MAP().loader().Load(total, file, 0);
            HGROUP.MAP().loader().Load(used, file, 0);

            totalT = 0;
            usedT = 0;
            foreach (var t in HGROUP.All())
            {
                totalT += total[t.Index()];
                usedT += used[t.Index()];
            }
        }

        protected override void Clear()
        {
            odd.saver.Clear();
            Array.Fill(total, 0);
            Array.Fill(used, 0);
            totalT = 0;
            usedT = 0;
        }

        public void Report(int used, int total, HTypeBits s)
        {
            for (int i = 0; i < HGROUP.All().Count; i++)
            {
                if (s.Is(i))
                {
                    HGROUP t = HGROUP.All()[i];
                    this.usedT += used;
                    this.totalT += total;
                    this.used[t.Index()] += used;
                    this.total[t.Index()] += total;
                }
            }
        }

        public int Total(HGROUP t)
        {
            if (t == null)
                return totalT;
            return total[t.Index()];
        }

        public int Used(HGROUP t)
        {
            if (t == null)
                return usedT;
            return used[t.Index()];
        }

        public override SFinderFindable Service(int tx, int ty)
        {
            return null;
        }

        public override COLOR MiniC(int tx, int ty)
        {
            return constructor.miniColor;
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public MAP_OBJECT<HomeInstance> Getter = new MAP_OBJECT<HomeInstance>()
        {
            public HomeInstance Get(int tx, int ty)
            {
                if (SETT.ROOMS().Map.Blueprint.Get(tx, ty) == this)
                {
                    return (HomeInstance)SETT.ROOMS().Map.Get(tx, ty);
                }
                return null;
            }

            public HomeInstance Get(int tile)
            {
                if (SETT.ROOMS().Map.Blueprint.Get(tile) == this)
                {
                    return (HomeInstance)SETT.ROOMS().Map.Get(tile);
                }
                return null;
            }
        };

        public MAP_OBJECT<HomeInstance> Service = new MAP_OBJECT<HomeInstance>()
        {
            public HomeInstance Get(int tx, int ty)
            {
                if (SETT.ROOMS().Map.Blueprint.Get(tx, ty) == this)
                {
                    HomeInstance h = (HomeInstance)SETT.ROOMS().Map.Get(tx, ty);
                    if (tx == h.ServiceX() && ty == h.ServiceY())
                        return h;
                }
                return null;
            }

            public HomeInstance Get(int tile)
            {
                return Get(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }
        };

        private readonly LOS los = new LOS()
        {
            public bool PassesToOtherFromThis(int fx, int fy, int tx, int ty)
            {
                if (SETT.ROOMS().fData.Tile.Get(fx, fy) == constructor.tOpening)
                    return true;
                return Getter.Get(fx, fy).Is(tx, ty);
            }

            public bool PassesFromOtherToThis(int fx, int fy, int tx, int ty)
            {
                if (SETT.ROOMS().fData.Tile.Get(tx, ty) == constructor.tOpening)
                    return true;
                return Getter.Get(tx, ty).Is(fx, fy);
            }

            public bool BlocksEnv(int tx, int ty)
            {
                return false;
            }

            public bool IsLightBlocker(int tx, int ty)
            {
                return false;
            }
        };

        public override void AppendView(LISTE<UIRoomModule> mm)
        {
            mm.Add(new HomeHoverer());
            base.AppendView(mm);
        }

        public override LOS LOS(int tx, int ty)
        {
            return los;
        }
    }
}