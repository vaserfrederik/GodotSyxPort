using System;
using init.race.home;
using init.sprite.game;
using settlement.main;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.rendering.RenderData;
using util.rendering;

namespace settlement.room.home.house
{
    abstract class Sprite : RoomSprite.Imp
    {
        public readonly bool Service;
        public readonly bool Bed;
        public readonly bool Solid;
        public HomeInstance House;

        public Sprite(bool service, bool bed, bool solid)
        {
            Service = service;
            Bed = bed;
            Solid = solid;
        }

        public Sprite() : this(false, false, true) { }

        public DIR Dir(int data)
        {
            return DIR.ORTHO.Get(data & 0b011);
        }

        static abstract class Rot : Sprite
        {
            Rot() : base() { }

            Rot(bool service, bool bed, bool solid) : base(service, bed, solid) { }

            public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
            {
                Sheets a = A(House.Race().Home().Clas(House.Occupant(0)));
                int ran = it.Ran();
                Sprites.Render1x1(ran, a, r, s, data, it, degrade);
                return false;
            }

            public override byte GetData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
            {
                Room room = SETT.ROOMS().Map.Get(tx, ty);

                int r = RND.rInt(4);
                for (int i = 0; i < DIR.ORTHO.Size; i++)
                {
                    int q = (i + r) % 4;
                    DIR d = DIR.ORTHO.Get((i + r) % DIR.ORTHO.Size);
                    if (!room.IsSame(tx, ty, tx + d.X(), ty + d.Y()))
                    {
                        return (byte)q;
                    }
                }

                for (int i = 0; i < DIR.ORTHO.Size; i++)
                {
                    DIR d = DIR.ORTHO.Get(i);
                    if (House.Sprite(tx + d.X(), ty + d.Y()) == SETT.ROOMS().HOME.Constructor.Spacetable)
                    {
                        return (byte)((i + 2) % 4);
                    }
                }

                return (byte)RND.rInt(4);
            }

            abstract Sheets A(RaceHomeClass sp);
        }

        protected RaceHomeClass Sp()
        {
            return House.Race().Home().Clas(House.Occupant(0));
        }
    }
}