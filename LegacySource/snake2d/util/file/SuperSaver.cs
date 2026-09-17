using System;
using System.Collections.Generic;
using System.IO;

namespace snake2d.util.file
{
    public abstract class SuperSaver<T> : SAVABLE
    {
        private readonly LIST<T> tt;
        private readonly KeyMap<T> map = new KeyMap<T>();
        private Type clas;

        public SuperSaver(Type clas, LIST<T> tt)
        {
            this.tt = tt;
            foreach (T t in tt)
            {
                map.put(key(t), t);
            }
            this.clas = clas;
        }

        protected abstract string key(T t);
        protected abstract void save(T t, FilePutter f);
        protected abstract void load(T t, FileGetter f);
        protected abstract void clear(T t);

        public override void save(FilePutter f)
        {
            f.i(tt.size());

            for (int i = 0; i < tt.size(); i++)
            {
                T e = tt.get(i);
                f.chars(key(e));
                int pos = f.getPosition();
                f.i(0);
                save(e, f);
                int le = f.getPosition() - pos - 4;
                f.setAtPosition(pos, le);
            }
        }

        public override void load(FileGetter f)
        {
            clear();
            int am = f.i();

            for (int i = 0; i < am; i++)
            {
                string k = f.chars();

                int pos = f.getPosition() + f.i() + 4;
                T e = map.get(k);
                if (e != null)
                {
                    load(e, f);
                    if (f.getPosition() != pos)
                    {
                        LOG.ln(clas + " " + k + " " + f.getPosition() + " " + pos + " " + e.GetType().Name);
                        f.setPosition(pos);
                        clear(e);
                    }
                }
                else
                {
                    LOG.ln(clas + " " + k);
                    f.setPosition(pos);
                }
            }
        }

        public override void clear()
        {
            for (int i = 0; i < tt.size(); i++)
            {
                T e = tt.get(i);
                clear(e);
            }
        }
    }
}