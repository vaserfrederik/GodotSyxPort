using System;
using System.IO;
using snake2d.util.file;

namespace util.data
{
    public interface BOOLEAN
    {
        bool Is();
    }

    public interface BOOLEAN_MUTABLE : BOOLEAN
    {
        BOOLEAN_MUTABLE Set(bool b);
        BOOLEAN_MUTABLE Toggle();
        BOOLEAN_MUTABLE SetOn();
        BOOLEAN_MUTABLE SetOff();
    }

    public static class BOOLEANImp : BOOLEAN_MUTABLE, SAVABLE
    {
        public bool B;
        public INFO Info;

        public BOOLEANImp()
        {
        }

        public BOOLEANImp(string name, string desc)
        {
            Info = new INFO(name, desc);
        }

        public BOOLEANImp(bool b)
        {
            B = b;
        }

        public bool Is()
        {
            return B;
        }

        public BOOLEAN_MUTABLE Set(bool b)
        {
            B = b;
            return this;
        }

        public BOOLEAN_MUTABLE Toggle()
        {
            return Set(!Is());
        }

        public BOOLEAN_MUTABLE SetOn()
        {
            return Set(true);
        }

        public BOOLEAN_MUTABLE SetOff()
        {
            return Set(false);
        }

        public void Save(FilePutter file)
        {
            file.Bool(B);
        }

        public void Load(FileGetter file)
        {
            B = file.Bool();
        }

        public void Clear()
        {
            B = false;
        }
    }
}