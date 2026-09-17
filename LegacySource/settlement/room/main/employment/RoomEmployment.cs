using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Room.Main.Employment
{
    public static class ROOMS
    {
        public static Employment Employment => new Employment();
    }

    public class Employment
    {
        public Employer Employer { get; } = new Employer();
    }

    public class Employer
    {
        public void UpdateAll() { }
    }

    public class HistoryInt
    {
        private readonly int[] _data;
        private readonly int _capacity;
        private int _index;

        public HistoryInt(int capacity, int days, bool circular)
        {
            _capacity = capacity;
            _data = new int[capacity];
        }

        public void Inc(int value)
        {
            _data[_index] += value;
            _index = (_index + 1) % _capacity;
        }

        public void Save(FilePutter file)
        {
            file.Write(_data);
        }

        public void Load(FileGetter file)
        {
            file.Read(_data);
        }

        public void Clear()
        {
            Array.Fill(_data, 0);
        }
    }

    public class FilePutter
    {
        private readonly BinaryWriter _writer;

        public FilePutter(BinaryWriter writer)
        {
            _writer = writer;
        }

        public void I(int value)
        {
            _writer.Write(value);
        }

        public void Is(int[] values)
        {
            _writer.Write(values.Length);
            _writer.Write(values);
        }
    }

    public class FileGetter
    {
        private readonly BinaryReader _reader;

        public FileGetter(BinaryReader reader)
        {
            _reader = reader;
        }

        public int I()
        {
            return _reader.ReadInt32();
        }

        public int[] Is()
        {
            int length = _reader.ReadInt32();
            return _reader.ReadInt32Array(length);
        }
    }

    public class CLAMP
    {
        public static int I(int value, int min, int max)
        {
            return Math.Min(max, Math.Max(min, value));
        }
    }

    public class ArrayListResize<T> : List<T>
    {
        public ArrayListResize(int initialCapacity, int maxCapacity) : base(initialCapacity)
        {
        }

        public int Add(T item)
        {
            base.Add(item);
            return Count - 1;
        }

        public void Clear()
        {
            base.Clear();
        }
    }

    public class INDEXED
    {
        public virtual int Index() => throw new NotImplementedException();
    }

    public class GRoupInt : INT_OE<WGROUP>
    {
        private int[] _racePrio;
        private int _total;
        private readonly int _min;
        private readonly int _max;

        public GRoupInt(int min, int max)
        {
            _min = min;
            _max = max;
            _racePrio = new int[WGROUP.All().Count];
        }

        public void Save(FilePutter file)
        {
            file.Is(_racePrio);
            file.I(_total);
        }

        public void Load(FileGetter file)
        {
            _racePrio = file.Is();
            _total = file.I();
        }

        public void Clear()
        {
            Array.Fill(_racePrio, 0);
            _total = 0;
        }

        public override int Max(WGROUP t) => _max;
        public override int Min(WGROUP t) => _min;
        public override int Get(WGROUP t) => t == null ? _total : _racePrio[t.Index()];
        public override void Set(WGROUP t, int i)
        {
            i = CLAMP.I(i, Min(t), Max(t));
            if (_racePrio[t.Index()] != i)
            {
                _total -= _racePrio[t.Index()];
                _racePrio[t.Index()] = i;
                _total += _racePrio[t.Index()];
            }
        }
    }

    public class Target
    {
        private int _target;
        private readonly int[] _perGroup;

        public Target(RoomEmployment p)
        {
            _perGroup = new int[WGROUP.All().Count];
        }

        public int Get() => _target;

        public int Group(WGROUP g) => _perGroup[g.Index()];

        public void Clear()
        {
            _target = 0;
            foreach (var e in WGROUP.All())
            {
                ROOMS.Employment.ChangeTarget(-_perGroup[e.Index()], e);
                _perGroup[e.Index()] = 0;
            }
        }

        public void Add(WGROUP g, int amount)
        {
            ROOMS.Employment.ChangeTarget(amount, g);
            _target += amount;
            _perGroup[g.Index()] += amount;
        }

        public void Save(FilePutter file)
        {
            file.I(_target);
            file.Is(_perGroup);
        }

        public void Load(FileGetter file)
        {
            _target = file.I();
            _perGroup = file.Is();
        }
    }

    public class Priority : INTE
    {
        private readonly RoomEmployment _p;
        private int _prio = 10;

        public Priority(RoomEmployment p)
        {
            _p = p;
        }

        public void Save(FilePutter file)
        {
            file.I(_prio);
        }

        public void Load(FileGetter file)
        {
            _prio = file.I();
        }

        public void Clear()
        {
            _prio = 10;
        }

        public override int Max() => 30;
        public override int Min() => 0;
        public override int Get() => _prio;
        public override void Set(int i)
        {
            i = CLAMP.I(i, Min(), Max());
            if (_prio != i)
            {
                _prio = i;
                ROOMS.Employment.Employer.UpdateAll();
            }
        }
    }

    public class RoomEmployment : RoomEmploymentSimple, INDEXED
    {
        private static readonly ArrayListResize<RoomEmployment> WORK = new ArrayListResize<RoomEmployment>(10, 512);

        static RoomEmployment()
        {
            new GameDisposable
            {
                Dispose = () => WORK.Clear()
            };
        }

        private readonly int _index = WORK.Add(this);
        private readonly HistoryInt _history = new HistoryInt(32, TIME.Days(), true);
        public readonly Target target = new Target(this);
        public readonly Priority priority = new Priority(this);
        public readonly GRoupInt priorities = new GRoupInt(0, 5);

        public RoomEmployment(RoomBlueprintIns<?> p, RoomInitData init) : base("WORK", p, init)
        {
        }

        public HISTORY_INT History() => _history;

        protected override void Employ(Humanoid h, int delta)
        {
            ROOMS.Employment.History.Inc(-Employed());
            base.Employ(h, delta);
            ROOMS.Employment.ChangeCurrent(delta, WGROUP.Get(h.Indu()));
            ROOMS.Employment.History.Inc(Employed());
            _history.Set(Employed());
        }

        protected override void Register(RoomEmploymentIns ins, int delta)
        {
            ROOMS.Employment.ChangeNeeded(Blueprint(), -WorkersNeeded);
            base.Register(ins, delta);
            ROOMS.Employment.ChangeNeeded(Blueprint(), WorkersNeeded);
        }

        public override void Save(FilePutter file)
        {
            base.Save(file);
            target.Save(file);
            priority.Save(file);
            priorities.Save(file);
            _history.Save(file);
        }

        public override void Load(FileGetter file)
        {
            base.Load(file);
            target.Load(file);
            priority.Load(file);
            priorities.Load(file);
            _history.Load(file);
        }

        public override void Clear()
        {
            base.Clear();
            target.Clear();
            priority.Clear();
            priorities.Clear();
            _history.Clear();
            SetPrioOnSkill();
        }

        public void SetPrioOnSkill()
        {
            foreach (var g in WGROUP.All())
            {
                SetPrioOnSkill(g);
            }
        }

        public void SetPrioOnFullfillment()
        {
            foreach (var g in WGROUP.All())
            {
                SetPrioOnFullfillment(g);
            }
        }

        public void SetPrioOnSkill(WGROUP g)
        {
            int p = CLAMP.I((int)Math.Round(RACES.Boosts().GetNorSkill(g.Race, this) * priorities.Max), 1, priorities.Max);
            priorities.Set(g, p);
        }

        public void SetPrioOnFullfillment(WGROUP g)
        {
            int p = CLAMP.I((int)Math.Round(g.Race.Pref().GetWork(this) * priorities.Max), 1, priorities.Max);
            priorities.Set(g, p);
        }

        public override int Index() => _index;

        public override double Efficiency()
        {
            return base.Efficiency();
        }
    }

    public abstract class RoomEmploymentSimple
    {
        protected RoomEmploymentSimple(string name, RoomBlueprintIns<?> p, RoomInitData init) { }

        protected abstract void Employ(Humanoid h, int delta);
        protected abstract void Register(RoomEmploymentIns ins, int delta);
        public abstract void Save(FilePutter file);
        public abstract void Load(FileGetter file) throws IOException;
        public abstract void Clear();
        public abstract double Efficiency();
    }

    public class GameDisposable
    {
        public Action Dispose { get; set; }
    }

    public class TIME
    {
        public static int Days() => 0; // Placeholder
    }

    public class RACES
    {
        public Boosts Boosts() => new Boosts();
    }

    public class Boosts
    {
        public double GetNorSkill(Race race, RoomEmploymentSimple roomEmployment) => 0; // Placeholder
    }

    public class Race
    {
        public Pref Pref() => new Pref();
    }

    public class Pref
    {
        public double GetWork(RoomEmploymentSimple roomEmployment) => 0; // Placeholder
    }

    public class WGROUP
    {
        public static IEnumerable<WGROUP> All() => new List<WGROUP>();
        public static WGROUP Get(int index) => new WGROUP();
        public Race Race { get; }
        public int Index() => 0; // Placeholder
    }

    public class Humanoid
    {
        public int Indu() => 0; // Placeholder
    }

    public class SETT
    {
        public static Rooms Rooms() => new Rooms();
    }

    public class Rooms
    {
        public Employment Employment => new Employment();
    }

    public class RoomBlueprintIns<T>
    {
    }

    public class RoomInitData
    {
    }

    public class RoomEmploymentIns
    {
    }

    public class INTE
    {
        public abstract int Max();
        public abstract int Min();
        public abstract int Get();
        public abstract void Set(int i);
    }

    public class INT_OE<T>
    {
        public abstract int Max(T t);
        public abstract int Min(T t);
        public abstract int Get(T t);
        public abstract void Set(T t, int i);
    }
}