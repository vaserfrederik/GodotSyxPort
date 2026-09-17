using System;
using System.IO;
using settlement.constant;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.rendering;

namespace settlement.room.service.arena.pit
{
    final class ArenaConstructor : Furnisher
    {
        private readonly ROOM_FIGHTPIT blue;

        public readonly FurnisherStatI workers;
        public readonly FurnisherStat spectators;
        private const int STATION = 1;
        private const int ARENA = 2;
        private readonly FurnisherItemTile cc;

        protected ArenaConstructor(ROOM_FIGHTPIT blue, RoomInitData init) : base(init, 1, 2)
        {
            this.blue = blue;
            workers = new FurnisherStatI(this);
            spectators = new FurnisherStat.FurnisherStatServices(this, blue);
            Json sp = init.data().json("SPRITES");
            final RoomSprite sSeats = new RoomSpriteBoxN(sp, "SEAT_BOX")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return getLevel(rx, ry, item) >= getLevel(rx - d.x(), ry - d.y(), item);
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    return base.render(r, s, data, it, degrade);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return (byte)getLevel(rx, ry, item);
                }
            };

            final RoomSprite sRim = new RoomSprite1x1(sp, "RIM")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return getLevel(rx, ry, item) == getLevel(rx - d.x(), ry - d.y(), item);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return 0;
                }
            };

            final RoomSprite sTower = new RoomSprite1x1(sp, "TOWER")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return getLevel(rx, ry, item) > getLevel(rx - d.x(), ry - d.y(), item);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return (byte)getLevel(rx, ry, item);
                }
            };

            final RoomSprite sStairs = new SStairs(sp, "STAIRS")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return getLevel(rx, ry, item) > getLevel(rx - d.x(), ry - d.y(), item);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return (byte)getLevel(rx, ry, item);
                }
            };

            cc = new FurnisherItemTile(new RoomSprite1x1(sp, "TOWER_CENTER"), STATION);
            new FurnisherItem(new FurnisherItemTile[][]
            {
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(cc, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(cc, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(cc, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(sStairs, 2), new FurnisherItemTile(cc, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sRim, 2), new FurnisherItemTile(sTower, 2) },
                { new FurnisherItemTile(sTower, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sSeats, 2), new FurnisherItemTile(sTower, 2) }
            });

            flush();
        }

        private int getLevel(int x, int y, FurnisherItem item)
        {
            int level = 0;
            for (int i = 0; i < DIR4.length; i++)
            {
                COOR c = DIR4[i].move(x, y);
                if (item.getTile(c.x, c.y) != null)
                {
                    level = Math.max(level, 1 + getLevel(c.x, c.y, item));
                }
            }
            return level;
        }

        private void flush()
        {
            // Implementation of the flush method
        }

        public override bool usesWater()
        {
            return false;
        }

        public override bool connectsToWater()
        {
            return false;
        }

        public override bool connectsToRoof()
        {
            return false;
        }

        public override bool isRoom()
        {
            return true;
        }

        public override bool isSecret()
        {
            return false;
        }

        public override bool isIndoor()
        {
            return true;
        }

        public override bool isOutdoor()
        {
            return false;
        }

        public override bool isMaze()
        {
            return false;
        }

        public override bool isWater()
        {
            return false;
        }

        public override bool isRoof()
        {
            return false;
        }

        public override bool isWall()
        {
            return false;
        }

        public override bool isFloor()
        {
            return true;
        }

        public override bool isCeiling()
        {
            return false;
        }

        public override bool isLadder()
        {
            return false;
        }

        public override bool isStair()
        {
            return false;
        }

        public override bool isDoor()
        {
            return false;
        }

        public override bool isWindow()
        {
            return false;
        }

        public override bool isTrap()
        {
            return false;
        }

        public override bool isSecretExit()
        {
            return false;
        }

        public override bool isSecretEntry()
        {
            return false;
        }

        public override bool isSecretRoom()
        {
            return false;
        }

        public override bool isSecretPassage()
        {
            return false;
        }

        public override bool isSecretStair()
        {
            return false;
        }

        public override bool isSecretLadder()
        {
            return false;
        }

        public override bool isSecretDoor()
        {
            return false;
        }

        public override bool isSecretWindow()
        {
            return false;
        }

        public override bool isSecretTrap()
        {
            return false;
        }

        public override bool isSecretSecretExit()
        {
            return false;
        }

        public override bool isSecretSecretEntry()
        {
            return false;
        }

        public override bool isSecretSecretRoom()
        {
            return false;
        }

        public override bool isSecretSecretPassage()
        {
            return false;
        }

        public override bool isSecretSecretStair()
        {
            return false;
        }

        public override bool isSecretSecretLadder()
        {
            return false;
        }

        public override bool isSecretSecretDoor()
        {
            return false;
        }

        public override bool isSecretSecretWindow()
        {
            return false;
        }

        public override bool isSecretSecretTrap()
        {
            return false;
        }
    }
}