using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.TileMap
{
    public sealed class SettMarks : TileMap.Resource
    {
        public readonly int Max = 32;
        public int State { get; private set; }
        public readonly LIST<SettMark> All;
        private readonly ArrayList<SettMark> active = new ArrayList<SettMark>(Max);

        public SettMarks()
        {
            ArrayList<SettMark> all = new ArrayList<SettMark>(Max);
            while (all.HasRoom())
                all.Add(new SettMark());
            this.All = all;
        }

        int upI = 0;

        public LIST<SettMark> Active()
        {
            if (upI == GAME.UpdateI())
                return active;
            upI = GAME.UpdateI();
            active.ClearSloppy();
            foreach (SettMark s in All)
                if (s.Active)
                    active.Add(s);
            return active;
        }

        public int State()
        {
            return State;
        }

        protected override void Save(FilePutter file)
        {
            file.i(State);
            foreach (SettMark d in All)
            {
                file.bool(d.Active);
                file.i(d.Tile.X());
                file.i(d.Tile.Y());

                d.Color.Save(file);
                d.Name.Save(file);
            }
        }

        protected override void Load(FileGetter file) throws IOException
        {
            active.Clear();
            State = file.i();
            foreach (SettMark d in All)
            {
                d.Active = file.bool();
                d.Tile.Set(file.i(), file.i());
                d.Color.Load(file);
                d.Name.Load(file);
            }
        }

        protected override void ClearAll()
        {
            State = 0;
            foreach (SettMark d in All)
            {
                d.Active = false;
            }
        }

        public SettMark Make()
        {
            foreach (SettMark d in All)
                if (!d.Active)
                {
                    d.Active = true;
                    State++;
                    return d;
                }

            return null;
        }

        public sealed class SettMark : SAVABLE
        {
            public readonly ShortCoo Tile = new ShortCoo();
            public readonly ColorImp Color = new ColorImp();
            public readonly Str Name = new Str(20);
            public bool Active;

            public SettMark()
            {
            }

            public void SetPosition(int position)
            {
                int i = 0;
                foreach (SettMark d in Active())
                {
                    if (i == position)
                    {
                        if (d == this)
                            return;
                        if (!d.Active)
                            return;
                        State++;
                        int tx = d.Tile.X();
                        int ty = d.Tile.Y();
                        ColorImp.TMP.Set(d.Color);
                        Str.TMP.Clear().Add(d.Name);

                        d.Tile.Set(Tile);
                        d.Color.Set(color);
                        d.Name.Clear().Add(Name);

                        Tile.Set(tx, ty);
                        Color.Set(ColorImp.TMP);
                        Name.Clear().Add(Str.TMP);
                    }
                    if (d.Active)
                        i++;
                }
            }

            public void Set(int tx, int ty)
            {
                Active = true;
                Tile.Set(tx, ty);
                Color.Set(RND.rInt(127), RND.rInt(127), RND.rInt(127));

                Name.Clear().Add('?');
                State++;
            }

            public void Remove()
            {
                if (Active)
                {
                    Active = false;
                    State++;
                }
            }

            public override void Save(FilePutter file)
            {
                file.bool(Active);
                file.i(Tile.X());
                file.i(Tile.Y());

                Color.Save(file);
                Name.Save(file);
            }

            public override void Load(FileGetter file) throws IOException
            {
                Active = file.bool();
                Tile.Set(file.i(), file.i());
                Color.Load(file);
                Name.Load(file);
            }

            public override void Clear()
            {
                Active = false;
            }
        }
    }
}