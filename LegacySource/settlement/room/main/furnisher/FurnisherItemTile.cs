using System;
using System.Collections.Generic;

namespace Settlement.Room.Main.Furnisher
{
    public class FurnisherItemTile : INDEXED
    {
        private readonly int index;
        public readonly bool CanGoCandle;
        public readonly RoomSprite Sprite;
        public readonly AVAILABILITY Availability;
        public readonly bool MustBeReachable;
        public bool NoWalls;
        private int data;

        public FurnisherItemTile(Furnisher p, bool mustBeReachable, RoomSprite sprite, AVAILABILITY availability, bool canGoCandle)
        {
            this.index = p.Tiles.Add(this);
            this.CanGoCandle = canGoCandle;
            this.Sprite = sprite;
            this.Availability = availability;
            this.MustBeReachable = mustBeReachable;
        }

        public FurnisherItemTile(Furnisher p, RoomSprite sprite, AVAILABILITY availability, bool canGoCandle)
            : this(p, false, sprite, availability, canGoCandle)
        {
        }

        public bool IsBlocker()
        {
            return Availability.Player < 0 || Availability.From > 0;
        }

        public bool IsNotBlocker()
        {
            return !IsBlocker() && Availability.Player <= AVAILABILITY.ROOM.Player;
        }

        public string IsPlacable(int tx, int ty, MAP_BOOLEAN roomIs, FurnisherItem it, int rx, int ry)
        {
            return null;
        }

        public int Index()
        {
            return index;
        }

        public FurnisherItemTile SetData(int data)
        {
            this.data = data;
            return this;
        }

        public int Data()
        {
            return data;
        }

        public RoomSprite Sprite()
        {
            return Sprite;
        }
    }
}