using System;
using System.Collections.Generic;
using System.IO;
using Settlement.Main.Sett;
using Settlement.Path.Finders;
using Settlement.Room.Main;
using Settlement.Room.Main.Category;
using Settlement.Room.Main.Util;
using Snake2D.Util.Color;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Sets;
using Snake2D.Util.Sprite;
using Util.Info;
using View.Sett.Ui.Room;
using View.Tool;

namespace Settlement.Room.Main.Throne
{
    public class THRONE : RoomBlueprint
    {
        private readonly Coo instance = new Coo(SETT.TWIDTH / 2, SETT.THEIGHT / 2);
        private int tile = instance.x + instance.y * SETT.TWIDTH;
        private readonly Coo construction = new Coo(-1, -1);

        private readonly SPRITE sprite;
        public readonly INFO info;
        public readonly PLACABLE placer;
        public readonly Initer init;

        public THRONE(RoomInitData init, RoomCategorySub cat) : base("_THRONE")
        {
            init.init("_THRONE");
            info = new INFO(init.text());
            sprite = new SPRITE(init);

            this.init = new Initer(this);
            placer = new Placer(this);
            Clear();
        }

        // public COORDINATE GetThrone()
        // {
        //     return instance;
        // }

        public static int Tile()
        {
            return SETT.ROOMS().THRONE.tile;
        }

        public static COORDINATE Coo()
        {
            return SETT.ROOMS().THRONE.instance;
        }

        public static DIR Rot()
        {
            Room r = SETT.ROOMS().map.Get(Coo());
            if (r != null && r is Instance)
            {
                return DIR.ORTHO.Get(((Instance)r).rot);
            }
            return DIR.N;
        }

        public override Room Get(int tx, int ty)
        {
            Room r = SETT.ROOMS().map.Get(tx, ty);
            if (r != null && r is Instance)
                return r;
            return null;
        }

        private void SetInstance(int tx, int ty)
        {
            instance.Set(tx, ty);
            tile = instance.x + instance.y * SETT.TWIDTH;
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            return null;
        }

        public override COLOR MiniC(int tx, int ty)
        {
            return sprite.miniC;
        }

        protected override COLOR MiniCPimped(ColorImp original, int tx, int ty, bool northern, bool southern)
        {
            return original;
        }

        protected override void Save(FilePutter saveFile)
        {
            instance.Save(saveFile);
            construction.Save(saveFile);
        }

        protected override void Load(FileGetter saveFile)
        {
            instance.Load(saveFile);
            construction.Load(saveFile);
            tile = instance.x + instance.y * SETT.TWIDTH;
        }

        protected override void Clear()
        {
            SetInstance(SETT.TWIDTH / 2, SETT.THEIGHT / 2);
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            // TODO Auto-generated method stub
        }

        public SPRITE Icon()
        {
            return sprite.icon;
        }
    }
}