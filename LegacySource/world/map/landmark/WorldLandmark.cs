using System;
using System.IO;

namespace World.Map.Landmark
{
    public class WorldLandmark
    {
        public readonly short index;
        public readonly Str name = new Str(32);
        public readonly Str description = new Str(256);
        private int size = 0;
        public short cx, cy;
        public byte textSize;

        public WorldLandmark(int index)
        {
            this.index = (short)index;
            Clear();
        }

        public int Index()
        {
            return index;
        }

        void Save(FilePutter f)
        {
            name.Save(f);
            description.Save(f);
            f.I(cx).I(cy);
            f.I(textSize);
            f.I(size);
        }

        void Load(FileGetter f)
        {
            name.Load(f);
            description.Load(f);
            cx = (short)f.I();
            cy = (short)f.I();
            textSize = (byte)f.I();
            size = f.I();
        }

        void Clear()
        {
            name.Clear().Add(index);
            description.Clear();
            cx = -1;
            cy = -1;
            textSize = -1;
            size = 0;
        }

        void Init(int cx, int cy, int area, int textDir)
        {
            this.cx = (short)cx;
            this.cy = (short)cy;
            this.textSize = (byte)textDir;
        }
    }
}