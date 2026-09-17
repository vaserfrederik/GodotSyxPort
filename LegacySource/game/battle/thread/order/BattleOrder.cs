using System;
using System.IO;

namespace Game.Battle.Thread.Order
{
    public class BattleOrder
    {
        public readonly Locked<BattleOrderPath> path = new Locked<BattleOrderPath>(new BattleOrderPath());
        public readonly Locked<DivFormationImp> dest = new Locked<DivFormationImp>(new DivFormationImp());
        public readonly Locked<BattleOrderTask> task = new Locked<BattleOrderTask>(new BattleOrderTask());

        public BattleOrder()
        {
        }

        public void Save(FilePutter file)
        {
            path.Save(file);
            dest.Save(file);
            task.Save(file);
        }

        public void Load(FileGetter file)
        {
            path.Load(file);
            dest.Load(file);
            task.Load(file);
        }

        public void Clear()
        {
            path.Clear();
            dest.Clear();
            task.Clear();
        }
    }

    public class Locked<T> where T : Copyable<T>, SAVABLE
    {
        private volatile bool hasNew;
        private volatile bool lock;
        private volatile int setI = 0;
        private readonly T t;

        public Locked(T t)
        {
            this.t = t;
        }

        private void Lock()
        {
            while (lock)
                ;
            lock = true;
        }

        public void Get(T to)
        {
            Lock();
            to.Copy(t);
            lock = false;
        }

        public void Set(T from)
        {
            Lock();
            t.Copy(from);
            setI++;
            lock = false;
            hasNew = true;
        }

        public bool ConsumeNew(T copyTo)
        {
            if (hasNew)
            {
                Get(copyTo);
                hasNew = false;
                return true;
            }
            return false;
        }

        public int SetI()
        {
            return setI;
        }

        public bool IsNew(short i)
        {
            return (short)(setI & 0x0FFFF) != i;
        }

        public void Save(FilePutter file)
        {
            t.Save(file);
            file.Write(setI);
        }

        public void Load(FileGetter file)
        {
            t.Load(file);
            setI = file.Read();
            hasNew = false;
        }

        public void Clear()
        {
            t.Clear();
            setI = 0;
            hasNew = false;
        }
    }
}