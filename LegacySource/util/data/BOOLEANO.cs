using System;
using System.IO;

namespace Util.Data
{
    public interface BooleanO<T>
    {
        bool Is(T t);

        default INFO Info()
        {
            return null;
        }

        public interface BooleanOE<T> : BooleanO<T>
        {
            BooleanOE<T> Set(T t, bool b);

            default BooleanOE<T> Toggle(T t)
            {
                return Set(t, !Is(t));
            }

            default BooleanOE<T> SetOn(T t)
            {
                return Set(t, true);
            }

            default BooleanOE<T> SetOff(T t)
            {
                return Set(t, false);
            }
        }

        public class BooleanOEImp<T> : BooleanOE<T>, SAVABLE where T : INDEXED
        {
            private readonly Bitmap1D data;
            public INFO Info;
            private readonly bool def;

            public BooleanOEImp(int size, bool def)
            {
                data = new Bitmap1D(size, def);
                this.def = def;
            }

            public bool Is(T t)
            {
                if (t == null)
                {
                    for (int i = 0; i < data.Size; i++)
                        if (data.Get(i))
                            return true;
                    return false;
                }
                return data.Get(t.Index());
            }

            public BooleanOE<T> Set(T t, bool b)
            {
                if (t == null)
                {
                    data.SetAll(b);
                }
                data.Set(t.Index(), b);
                return this;
            }

            public void Save(FilePutter file)
            {
                data.Save(file);
            }

            public void Load(FileGetter file)
            {
                data.Load(file);
            }

            public void Clear()
            {
                data.SetAll(def);
            }

            public INFO Info()
            {
                return Info;
            }
        }
    }
}