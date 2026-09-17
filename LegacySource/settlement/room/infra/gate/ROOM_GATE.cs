using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Infra.Gate
{
    public class RoomGate : RoomBlueprintImp
    {
        public static string Type = "GATEHOUSE";
        private readonly MConstructor constructor;
        private readonly Instance instance;

        private static readonly string ¤¤Locked = "¤Locked. Subjects are unable to pass. Click to unlock.";
        private static readonly string ¤¤Unlocked = "¤Unlocked. Subjects can pass, but not enemies. Click to lock gate for subjects.";

        static RoomGate()
        {
            D.Ts(typeof(RoomGate));
        }

        public RoomGate(RoomInitData init, int typeIndex, string key, RoomCategorySub cat) : base(init, typeIndex, key, cat)
        {
            this.constructor = new MConstructor(this, init);
            this.instance = new Instance(init.m, this);
        }

        public override Room Get(int tx, int ty)
        {
            if (ROOMS.Map.Get(tx, ty) == instance)
                return instance;
            return null;
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new UIRoomModule
            {
                Hover = (box, i, rx, ry) =>
                {
                    box.NL();
                    if (Locked(rx, ry))
                    {
                        box.Add(box.Text().Errorify().Add(¤¤Locked));
                    }
                    else
                    {
                        box.Add(box.Text().Normalify2().Add(¤¤Unlocked));
                    }
                }
            });
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            return null;
        }

        public override MConstructor Constructor()
        {
            return constructor;
        }

        private static readonly Coo cooLock = new Coo();

        public void Lock(int tx, int ty, bool lockStatus)
        {
            if (ROOMS.Map.Get(tx, ty) == instance)
            {
                FurnisherItem it = SETT.ROOMS.FData.Item.Get(tx, ty);
                COORDINATE x1y1 = SETT.ROOMS.FData.ItemX1Y1(tx, ty, cooLock);
                if (it == null || x1y1 == null)
                    return;

                for (int y = 0; y < it.Height; y++)
                {
                    for (int x = 0; x < it.Width; x++)
                    {
                        if (!it.Is(x, y))
                            continue;
                        int dx = x1y1.X + x;
                        int dy = x1y1.Y + y;
                        if (!instance.IsSame(tx, ty, dx, dy))
                            continue;
                        SETT.ROOMS.FData.SpriteData2.Set(dx, dy, lockStatus ? 1 : 0);
                    }
                }
                for (int y = 0; y < it.Height; y++)
                {
                    for (int x = 0; x < it.Width; x++)
                    {
                        if (!it.Is(x, y))
                            continue;
                        int dx = x1y1.X + x;
                        int dy = x1y1.Y + y;
                        if (!instance.IsSame(tx, ty, dx, dy))
                            continue;
                        SETT.PATH.Availability.UpdateAvailability(dx, dy);
                    }
                }
            }
        }

        public bool Locked(int tx, int ty)
        {
            if (ROOMS.Map.Get(tx, ty) == instance)
            {
                return SETT.ROOMS.FData.SpriteData2.Get(tx, ty) == 1;
            }
            return false;
        }

        private sealed class Instance : RoomSingleton
        {
            private static readonly long SerialVersionUID = 1L;

            public Instance(ROOMS m, RoomBlueprint p) : base(m, p)
            {
            }

            protected override object ReadResolve()
            {
                return BlueprintI().Instance;
            }

            public override RoomGate BlueprintI()
            {
                return (RoomGate)base.Blueprint();
            }

            protected override void RemoveAction(ROOMA a)
            {
                foreach (COORDINATE c in a.Body())
                {
                    if (a.Is(c) && TERRAIN.TREES.IsTree(c.X, c.Y))
                        TERRAIN.NADA.PlaceFixed(c.X, c.Y);
                }
            }

            public override AVAILABILITY GetAvailability(int tile)
            {
                return base.GetAvailability(tile);
            }
        }

        private sealed class MConstructor : Furnisher
        {
            private readonly RoomGate blue;

            public MConstructor(RoomGate blue, RoomInitData init) : base(init)
            {
                this.blue = blue;
            }

            public override bool UsesArea()
            {
                return false;
            }

            public override bool MustBeIndoors()
            {
                return false;
            }

            public override Room Create(TmpArea area, RoomInit init)
            {
                return blue.instance.Place(area);
            }

            public override RoomBlueprintImp Blue()
            {
                return blue;
            }

            public override void PutFloor(int tx, int ty, int upgrade, AREA area)
            {
                if (SETT.FLOOR.Getter.Get(tx, ty) != null)
                    return;
                base.PutFloor(tx, ty, upgrade, area);
            }

            private sealed class Sprite : RoomSpriteRot
            {
                private int off;

                public Sprite(TILE_SHEET sheet, int startTile) : base(sheet, startTile, 1, SPRITES.Cons().ROT.Full)
                {
                    off = 0;
                    SetShadow(16, 0);
                }

                public Sprite(TILE_SHEET sheet, int startTile, bool off) : base(sheet, startTile, 1, SPRITES.Cons().ROT.Full)
                {
                    this.off = 2;
                    SetShadow(16, 0);
                }

                protected override bool JoinsWith(RoomSprite s, bool outof, int dir, DIR test, int rx, int ry, FurnisherItem item)
                {
                    return DIR.ORTHO.Get(dir + off) == test;
                }

                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.Render(SPRITE_RENDERER.DUMMY, s, data, it, degrade, false);
                    return false;
                }

                public override void RenderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                }

                public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    base.Render(r, ShadowBatch.DUMMY, data, it, degrade, false);
                }
            }

            private class Tile : FurnisherItemTile
            {
                public Tile(MConstructor constructor, bool isWall, RoomSprite sprite, AVAILABILITY availability, bool isFloor) : base(constructor, isWall, sprite, availability, isFloor)
                {
                }
            }
        }

        public override double Strength(int tile)
        {
            FurnisherItem it = SETT.ROOMS.FData.Item.Get(tile);
            return 400 * Math.Max(it.Width, it.Height) * C.TILE_SIZE;
        }

        protected override void Save(BinaryWriter saveFile)
        {
            // TODO Auto-generated method stub
        }

        protected override void Load(BinaryReader saveFile)
        {
            // TODO Auto-generated method stub
        }

        protected override void Clear()
        {
        }
    }
}