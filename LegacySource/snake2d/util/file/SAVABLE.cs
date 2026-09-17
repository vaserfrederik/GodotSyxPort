using System;
using System.IO;

namespace snake2d.util.file
{
    public interface SAVABLE
    {
        void Save(FilePutter file);
        void Load(FileGetter file);
        void Clear();
    }

    public abstract class SuperSavable : SAVABLE
    {
        private readonly string key;

        protected SuperSavable(string key)
        {
            this.key = key;
        }

        public void Save(FilePutter f)
        {
            f.Chars(key);
            int pos = f.GetPosition();
            f.I(0);
            PSave(f);
            int le = f.GetPosition() - pos - 4;
            f.SetAtPosition(pos, le);
        }

        public abstract void Load(FileGetter file);

        public abstract void Clear();

        protected abstract void PSave(FilePutter f);
    }
}