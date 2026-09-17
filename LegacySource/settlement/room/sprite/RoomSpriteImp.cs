using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Util.Rendering;
using Util.Mathematics;
using Util.DataTypes;

namespace Settlement.Room.Sprite
{
    public abstract class RoomSpriteImp : RoomSprite
    {
        protected readonly Sheets[] sheets;

        public readonly bool Rotates;
        private int sData = 0;
        protected double animationSpeed = 1.0;

        public RoomSpriteImp(SheetType type, JObject json, string key) : base(type, json, key)
        {
            if (json.ContainsKey(key))
            {
                var js = json[key].ToObject<List<JObject>>();
                sheets = new Sheets[js.Count];
                for (int i = 0; i < js.Count; i++)
                {
                    sheets[i] = new Sheets(type, js[i]);
                }
            }
            else
            {
                sheets = new Sheets[] { new Sheets(type, json[key].ToObject<JObject>()) };
            }

            bool rot = false;
            foreach (var s in sheets)
            {
                foreach (var ss in s.Sheets)
                {
                    rot |= ss.S.HasRotation & ss.D.Rotates;
                }
            }
            this.Rotates = rot;
        }

        public RoomSpriteImp(RoomSprite others) : base(others)
        {
            var other = others as RoomSpriteImp;
            if (other.Type != Type)
                throw new Exception();
            this.sheets = other.sheets;
            this.Rotates = other.Rotates;
            this.animationSpeed = other.animationSpeed;
        }

        public RoomSpriteImp(SheetType type) : base(type)
        {
            this.sheets = new Sheets[] { new Sheets(type.Dummy(), SheetData.DUMMY) };
            this.Rotates = false;
            this.animationSpeed = 1.0;
        }

        public Sheets Sheet(RenderIterator it)
        {
            if (sheets.Length == 1)
                return sheets[0];
            var r = SETT.ROOMS().Map.Get(it.Tx, it.Ty);
            if (r == null)
                return sheets[0];
            return sheets[Math.Clamp(r.Upgrade(it.Tx, it.Ty), 0, sheets.Length - 1)];
        }

        public SheetPair SheetPair(RenderIterator it, int ran)
        {
            var a = Sheet(it);
            if (a == null)
                return null;
            return a.Get(ran);
        }

        public SheetPair Get(RenderIterator it, int random)
        {
            var a = Sheet(it);
            if (a == null)
                return null;
            return a.Get(random);
        }

        public int Frame(SheetPair a, RenderIterator it)
        {
            if (a == null)
                return 0;
            return a.D.Frame(it.Ran, animationSpeed);
        }

        protected abstract bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item);

        public override void RenderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
        {
        }

        public override int SData()
        {
            return sData;
        }

        public RoomSpriteImp SData(int d)
        {
            sData = d;
            return this;
        }

        public abstract SheetType Type { get; }

        protected int GetData2(RenderIterator it)
        {
            return SETT.ROOMS().FData.SpriteData2.Get(it.Tile);
        }

        public void Animate(double speed)
        {
            this.animationSpeed = speed;
        }
    }
}